using Application.Ports.Driving;
using Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rastreador.Contracts;
using Rastreador.Queues;
using System.Text;
using System.Text.Json;

namespace Adapters.Messaging.Consumer
{
    public class PrecoColetadoConsumer : BackgroundService
    {
        private const string Fila = Queues.PrecosColetados;

        private readonly RabbitMqConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PrecoColetadoConsumer> _logger;

        private IChannel? _channel;

        public PrecoColetadoConsumer
        (
            RabbitMqConnection connection,
            IServiceScopeFactory scopeFactory,
            ILogger<PrecoColetadoConsumer> logger
        )
        {
            _connection = connection;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = await _connection.GetAsync(stoppingToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
            _channel = channel;

            await channel.QueueDeclareAsync(
                queue: Fila,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );

            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false,
                cancellationToken: stoppingToken
            );

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += Consumer_ReceivedAsync;

            await channel.BasicConsumeAsync(
                queue: Fila,
                autoAck: false,
                consumer: consumer,
                consumerTag: "preco-coletado-consumer",
                noLocal: false,
                exclusive: false,
                arguments: null,
                cancellationToken: stoppingToken
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs @event)
        {
            var canal = _channel!;

            ProdutoPrecoDto? produto;
            try
            {
                produto = JsonSerializer.Deserialize<ProdutoPrecoDto>(Encoding.UTF8.GetString(@event.Body.ToArray()));
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON inválido na fila {Fila}. Descartando.", Fila);

                await canal.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            if (produto is null)
            {
                _logger.LogError("Mensagem fora do contrato na fila {Fila}. Descartando.", Fila);
                await canal.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            try
            {
                // Cada mensagem é uma unidade de trabalho independente, então ganha o próprio
                // escopo — e com ele um AppDbContext limpo. É o papel que a requisição HTTP
                // cumpre no ASP.NET. Sem isso, o consumer (singleton) não consegue resolver
                // um serviço que depende de algo scoped.
                using var scope = _scopeFactory.CreateScope();
                var notificator = scope.ServiceProvider.GetRequiredService<INotificatorService>();

                await notificator.NotificarPrecoEncontrado(produto);
                await canal.BasicAckAsync(@event.DeliveryTag, multiple: false);
            }
            catch (ProductNotFoundException ex)
            {
                // Erro permanente: o produto não existe mais (foi removido entre o rastreamento
                // e a coleta). Tentar de novo vai falhar igual, então a mensagem morre aqui.
                _logger.LogWarning(ex, "Produto {IdProduto} não existe mais. Descartando a mensagem.", produto.IdProduto);
                await canal.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: false);
            }
            catch (Exception ex) when (@event.Redelivered)
            {
                // Já é a segunda vez que esta mensagem falha. Insistir só alimenta o loop:
                // volta pra fila, falha, volta de novo — queimando CPU para sempre.
                _logger.LogError(ex, "Produto {IdProduto} falhou na reentrega. Descartando a mensagem.", produto.IdProduto);
                await canal.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: false);
            }
            catch (Exception ex)
            {
                // Primeira falha: trata como transitória (banco fora do ar, por exemplo)
                // e devolve para a fila para uma segunda tentativa.
                _logger.LogError(ex, "Erro ao validar preço do produto com id {IdProduto}. Devolvendo para a fila.", produto.IdProduto);
                await canal.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: true);
            }
        }
    }
}
