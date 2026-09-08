# Rastreador de preços

Esse projeto nasceu como um jeito de colocar em prática alguns conceitos que estou estudando na pós-graduação: **Arquitetura Hexagonal**, **Microsserviços** (com RabbitMQ), **Docker** e **API Gateway**. A ideia central é simples — um sistema que rastreia o preço de produtos e avisa quando ele cai — mas o objetivo real é usar esse sistema como campo de treino pra essas arquiteturas.

Dividi o desenvolvimento em fases, evoluindo aos poucos de um monolito pra uma arquitetura de microsserviços. Cada fase só começou quando a anterior estava rodando de ponta a ponta.

---

## Rodando o projeto

O único pré-requisito é ter o **Docker** instalado. Todo o resto — API, banco, fila, gateway — sobe junto.

```bash
cp .env.example .env
docker compose up --build
```

Com tudo de pé:

| O quê | Onde |
| --- | --- |
| API (pelo gateway) | http://localhost:8000/api/ManageProduct |
| Swagger | http://localhost:8000/swagger |
| Painel do RabbitMQ | http://localhost:15672 |
| Admin API do Kong | http://localhost:8001 |

Primeiro cadastra o produto:

```bash
curl -X POST http://localhost:8000/api/ManageProduct \
  -H "Content-Type: application/json" \
  -d '{"nome":"Teclado","url":"https://exemplo.com/teclado","precoAtual":300,"precoAlvo":200}'
```

E depois consulta ele, que é o que dispara o rastreamento:

```bash
curl http://localhost:8000/api/ManageProduct/1
```

Agora é só acompanhar os logs e ver o alerta chegar do outro lado:

```bash
docker compose logs -f notificador
```

> O Scraper hoje devolve um preço aleatório em vez de raspar a página de verdade. Ele é um adaptador atrás de uma porta (`IColetorDePreco`), então trocar o mock por scraping real é mexer numa classe só — e essa é exatamente a ideia que eu queria treinar aqui.

---

## Fase 1 — Monolito com Arquitetura Hexagonal

O primeiro passo é ter uma base sólida antes de sair separando tudo em serviços. Nessa fase, o sistema principal nasce sozinho, mas já estruturado com **Arquitetura Hexagonal**, garantindo que o fluxo sempre respeite a ordem certa:

`Adaptadores → Portas (entrada e saída) → Application → Domain`

Fiz esse diagrama no quadro pra deixar claro como esse fluxo funciona antes de escrever qualquer linha de código:

<br>
<img width="480" height="350" alt="image" src="https://github.com/user-attachments/assets/f7547ed8-e1be-4316-9f30-6a9bfefce4cb" />

O que isso garante na prática: o **Domain** não conhece ninguém, a **Application** só conhece as portas, e quem sabe o que é Postgres, HTTP ou RabbitMQ são os **adaptadores** nas bordas. A regra que decide se o preço-alvo foi atingido mora em `Produto.AtingiuPrecoAlvo` — dentro do domínio, sem saber que existe fila ou banco.

## Fase 2 — Docker e RabbitMQ

Com o monolito rodando, o próximo passo é colocar tudo pra funcionar em containers com **Docker**, e configurar o **RabbitMQ** — que vai ser a peça responsável por fazer os serviços conversarem entre si mais pra frente, quando o sistema for dividido.

Foi aqui que apareceram as dores que só container ensina: multi-stage build pra imagem final não carregar o SDK inteiro, ordem dos comandos no Dockerfile pra aproveitar o cache, rede interna do compose (a API acha o banco por `postgres`, não por `localhost`), volume pra não perder os dados do Postgres a cada `down`, e `depends_on` com `condition: service_healthy` — porque container "no ar" não é a mesma coisa que serviço "pronto".

## Fase 3 — Separando em microsserviços

Por fim, separo o sistema em outros dois serviços independentes:

- **Scraper** — fica responsável por buscar o preço atual do produto.
- **Notificações** — fica responsável por avisar o usuário quando o preço muda.

Os três são projetos .NET separados, cada um com a própria hexagonal, e **nenhum deles chama o outro diretamente**: toda a conversa passa pelo RabbitMQ. O fluxo completo fica assim:

<br>
<img width="480" height="350" alt="WhatsApp Image 2026-07-31 at 17 14 46" src="https://github.com/user-attachments/assets/cc39ee70-cb80-4814-892c-093c4fd4d2ac" />

1. O **Principal** publica o produto na fila (o cadastro salva no banco; é a consulta que dispara o rastreamento).
2. O **Scraper** consome, e o produto começa a ser rastreado.
3. O Scraper extrai o preço e devolve pro RabbitMQ.
4. O **Principal** consome esse preço e valida se atingiu o objetivo.
   - **4.5.** Se atingiu, publica o alerta de volta na fila.
5. O serviço de **Notificações** consome o alerta e avisa o usuário.

As filas que ligam tudo isso ficam num projeto compartilhado (`Rastreador.Queues`), pra nome de fila não virar string solta espalhada pelo código:

| Fila | Quem publica | Quem consome |
| --- | --- | --- |
| `produtos-para-rastrear` | Principal | Scraper |
| `precos-coletados` | Scraper | Principal |
| `produto-notificado` | Principal | Notificações |

## Fase 4 — API Gateway

Com três serviços no ar, faltava um ponto único de entrada. Coloquei o **Kong** na frente, em modo *DB-less*: em vez de guardar as rotas num banco próprio, ele lê tudo de um arquivo declarativo (`kong/kong.yml`) no boot — a configuração do gateway fica versionada no git, junto do código.

A API deixou de publicar porta no host: ela só existe dentro da rede do compose, e quem fala com o mundo é o Kong, na porta 8000. A Admin API (8001) fica presa no `localhost`, porque quem alcança ela reconfigura o gateway inteiro.

Entrou também um plugin de `correlation-id`, que carimba um `X-Request-Id` em toda requisição. Com três serviços conversando por fila, é isso que permite amarrar uma chamada HTTP aos logs que ela gera lá na frente.

E cada serviço passou a declarar quando está realmente pronto, via healthcheck no compose — o Kong só sobe depois que a API responde no `/health`, que por sua vez só sobe depois que Postgres e RabbitMQ estão saudáveis.

---

## Estrutura dos projetos

Cada serviço repete a mesma divisão hexagonal: um host, um núcleo (`Application` + portas) e os adaptadores nas bordas.

```
Principal (API)
├── Bootstrap                      Host da Web API
├── Adapters.Rest                  Controllers + middleware de erro
├── Adapters.Persistence           EF Core + Postgres
├── Adapters.Messaging             Publishers e consumer do RabbitMQ
├── Application                    Casos de uso e portas
└── Domain                         Entidades e regras

Scraper
├── RastreadorDePreco.Scraper      Host (Worker)
├── Scraper.Application
├── Scraper.Adapters.Messaging
└── Adapters.Coletor               Busca o preço (hoje, um mock)

Notificações
├── RastreadorDePreco.Notificador  Host (Worker)
├── Notificador.Application
├── Notificador.Adapters.Messaging
└── Notificador.Adapters.Email     "E-mail" fake no console

Compartilhado
├── Contracts                      DTOs que trafegam nas filas
└── Rastreador.Queues              Nomes das filas

Tests
└── Principal.Tests
```

## Stack

- .NET 8 (Web API + Worker Services)
- PostgreSQL com Entity Framework Core
- RabbitMQ
- Docker e Docker Compose
- Kong (API Gateway)
- xUnit

## Próximos passos

- Healthcheck nos workers — eles não falam HTTP, então precisam de outra estratégia pra dizer que estão vivos.
- `docker-compose.override.yml` separando configuração de desenvolvimento e produção.
- Persistir o preço coletado: hoje ele é usado só pra comparar com o alvo e descartado.
- Trocar o mock do Scraper por scraping de verdade.

---

Esse README foi crescendo junto com o projeto — a ideia era documentar cada fase conforme ela ia saindo do papel.
