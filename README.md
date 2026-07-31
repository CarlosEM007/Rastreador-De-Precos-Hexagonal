# Price Tracker
 
Esse projeto nasceu como um jeito de colocar em prática alguns conceitos que estou estudando na pós-graduação: **Arquitetura Hexagonal**, **Microsserviços** (com RabbitMQ), **Docker** e **API Gateway**. A ideia central é simples — um sistema que rastreia o preço de produtos e avisa quando ele cai — mas o objetivo real é usar esse sistema como campo de treino pra essas arquiteturas.
 
Estou dividindo o desenvolvimento em 3 fases, evoluindo aos poucos de um monolito pra uma arquitetura de microsserviços.
 
## Fase 1 — Monolito com Arquitetura Hexagonal
 
O primeiro passo é ter uma base sólida antes de sair separando tudo em serviços. Nessa fase, o sistema principal nasce sozinho, mas já estruturado com **Arquitetura Hexagonal**, garantindo que o fluxo sempre respeite a ordem certa:
 
`Adaptadores → Portas (entrada e saída) → Application → Domain`
 
Fiz esse diagrama no quadro pra deixar claro como esse fluxo funciona antes de escrever qualquer linha de código:
 
<br>
<img width="480" height="350" alt="image" src="https://github.com/user-attachments/assets/f7547ed8-e1be-4316-9f30-6a9bfefce4cb" />

## Fase 2 — Docker e RabbitMQ
 
Com o monolito rodando, o próximo passo é colocar tudo pra funcionar em containers com **Docker**, e configurar o **RabbitMQ** — que vai ser a peça responsável por fazer os serviços conversarem entre si mais pra frente, quando o sistema for dividido.
 
## Fase 3 — Separando em microsserviços
 
Por fim, separo o sistema em outros dois serviços independentes:
 
- **Scraper** — fica responsável por buscar o preço atual do produto.
- **Notificações** — fica responsável por avisar o usuário quando o preço muda.
O fluxo completo de comunicação entre os três serviços, usando o RabbitMQ como intermediário, fica assim:
 
<br>
<img width="480" height="350" alt="WhatsApp Image 2026-07-31 at 17 14 46" src="https://github.com/user-attachments/assets/cc39ee70-cb80-4814-892c-093c4fd4d2ac" />

1. **Principal** envia o novo produto pro RabbitMQ.
2. O produto começa a ser rastreado pelo **Scraper**.
3. O Scraper extrai o preço do produto e devolve pro RabbitMQ.
4. O **Principal** valida se o preço atingiu o objetivo.
5. Se atingiu, o Principal envia o novo preço de volta pro RabbitMQ (passo 4.5).
6. O RabbitMQ entrega pro serviço de **Notificações**, que avisa o usuário sobre o novo preço.
---
 
Esse README vai crescendo junto com o projeto — a ideia é documentar cada fase conforme ela for saindo do papel.
