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

Cadastra um produto:

```bash
curl -X POST http://localhost:8000/api/ManageProduct \
  -H "Content-Type: application/json" \
  -d '{"nome":"Teclado","url":"https://exemplo.com/teclado","precoAtual":300,"precoAlvo":200}'
```

E pronto — daí em diante o sistema se vira sozinho. O agendador reenfileira todos os produtos de tempos em tempos, e o alerta aparece quando o preço cai:

```bash
docker compose logs -f notificador
```

Se não quiser esperar o próximo ciclo, dá pra forçar o rastreamento de um produto:

```bash
curl -X POST http://localhost:8000/api/ManageProduct/1/rastrear
```

### Endpoints

| Método | Rota | O que faz |
| --- | --- | --- |
| `POST` | `/api/ManageProduct` | Cadastra um produto |
| `GET` | `/api/ManageProduct` | Lista todos |
| `GET` | `/api/ManageProduct/{id}` | Devolve um produto |
| `PUT` | `/api/ManageProduct/{id}` | Atualiza nome, URL e preço-alvo |
| `DELETE` | `/api/ManageProduct/{id}` | Remove |
| `POST` | `/api/ManageProduct/{id}/rastrear` | Força o rastreamento agora (responde `202`: o preço chega depois, pela fila) |

### Desenvolvimento e produção

O `docker-compose.override.yml` é carregado automaticamente e é o que define o modo de desenvolvimento: porta direta na API pra depurar sem o gateway, logs em `Debug`, ciclo de rastreamento curto (1 minuto) e o coletor de preço no modo **Fake**, que sorteia um valor em vez de ir na rede — assim o fluxo inteiro roda sem depender de nenhuma loja real estar no ar.

Pra subir no modo produção, é só ignorar o override:

```bash
docker compose -f docker-compose.yml up --build
```

Aí entram o coletor HTTP de verdade, o ciclo de 5 minutos e o gateway como único caminho de entrada.

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

1. O **Principal** publica o produto na fila.
2. O **Scraper** consome, e o produto começa a ser rastreado.
3. O Scraper extrai o preço e devolve pro RabbitMQ.
4. O **Principal** consome esse preço, guarda no banco e valida se atingiu o objetivo.
   - **4.5.** Se atingiu, publica o alerta de volta na fila.
5. O serviço de **Notificações** consome o alerta e avisa o usuário.

Quem dá a partida no passo 1 é um **agendador** dentro do Principal: um `BackgroundService` que, de tempos em tempos, reenfileira todos os produtos cadastrados. É o que faz o rastreador rastrear sozinho, sem ninguém chamar endpoint nenhum — o `POST /{id}/rastrear` existe só pra forçar uma rodada na hora.

Uma coisa que aparece quando os serviços passam a conversar por fila: **mensagem que falha não pode voltar pra fila pra sempre**. Se o produto foi removido entre o rastreamento e a coleta, insistir não resolve — o erro é permanente e a mensagem é descartada. Já uma falha de rede merece uma segunda tentativa, mas só uma: os consumers olham a flag `Redelivered` e, na segunda falha seguida, param de insistir. Sem isso, uma única mensagem ruim fica girando em loop e queimando CPU.

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

E cada serviço passou a declarar quando está realmente pronto, via healthcheck no compose. O Kong só sobe depois que a API responde no `/health`, que por sua vez só sobe depois que Postgres e RabbitMQ estão saudáveis.

Os dois workers deram mais trabalho: eles não falam HTTP, então não existe endpoint pro compose chamar. A saída foi inverter a lógica — cada worker toca um arquivo a cada 15 segundos enquanto a conexão com o RabbitMQ estiver aberta, e o healthcheck só olha se esse carimbo é recente. Arquivo velho quer dizer serviço travado, o que é mais honesto do que "o processo existe".

---

## Estrutura dos projetos

Cada serviço repete a mesma divisão hexagonal: um host, um núcleo (`Application` + portas) e os adaptadores nas bordas.

```
Principal (API)
├── Bootstrap                      Host da Web API
├── Adapters.Rest                  Controllers + middleware de erro
├── Adapters.Persistence           EF Core + Postgres (migrations no start)
├── Adapters.Messaging             Publishers e consumer do RabbitMQ
├── Adapters.Agendador             Dispara o rastreamento em loop
├── Application                    Casos de uso e portas
└── Domain                         Entidades e regras

Scraper
├── RastreadorDePreco.Scraper      Host (Worker)
├── Scraper.Application
├── Scraper.Adapters.Messaging
└── Adapters.Coletor               ColetorHttp (real) e ColetorFake (dev)

Notificações
├── RastreadorDePreco.Notificador  Host (Worker)
├── Notificador.Application
├── Notificador.Adapters.Messaging
└── Notificador.Adapters.Email     "E-mail" fake no console

Compartilhado
├── Contracts                      DTOs que trafegam nas filas
├── Rastreador.Queues              Nomes das filas
└── Rastreador.Health              Heartbeat dos workers

Tests
├── Principal.Tests
├── Scraper.Tests
└── Notificador.Tests
```

### Coletando o preço

O `ColetorHttp` baixa a página e procura o preço no dado estruturado que as lojas publicam pro Google — JSON-LD e meta tags do Open Graph / schema.org — em vez de caçar classe de CSS, que muda a cada redesign. Ele entende tanto `1.234,56` quanto `1,234.56`.

Os dois coletores vivem atrás da mesma porta (`IColetorDePreco`), e qual deles sobe é decidido por uma variável de ambiente. Trocar o jeito de obter o preço não toca em mais nada do sistema — que é o ponto inteiro da hexagonal.

## Stack

- .NET 8 (Web API + Worker Services)
- PostgreSQL com Entity Framework Core
- RabbitMQ
- Docker e Docker Compose
- Kong (API Gateway)
- xUnit + Moq

## Próximos passos

- **Dead-letter queue** pras mensagens descartadas: hoje elas só viram log, e o certo é mandar pra uma fila de análise.
- **Autenticação** no gateway — o Kong tem plugins de JWT e rate limiting que resolveriam isso sem tocar na API.
- **Histórico de preços**, pra conseguir mostrar a variação ao longo do tempo em vez de só o último valor.

---

Esse README foi crescendo junto com o projeto — a ideia era documentar cada fase conforme ela ia saindo do papel.
