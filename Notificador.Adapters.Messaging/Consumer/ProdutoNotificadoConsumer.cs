using Application.Ports.Driving;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rastreador.Queues;
using System.Text;
using System.Text.Json;

namespace Adapters.Messaging.Consumer
{
    public class ProdutoNotificadoConsumer : BackgroundService
    {
        private const string Fila = Queues.ProdutoNotificado;

        private readonly RabbitMqConnection _connection;
        private readonly INotificarUsuario _notificar;
        private readonly ILogger<ProdutoNotificadoConsumer> _logger;

        private IChannel? _channel;

        public ProdutoNotificadoConsumer(
            RabbitMqConnection connection,
            INotificarUsuario notificar,
            ILogger<ProdutoNotificadoConsumer> logger)
        {
            _connection = connection;
            _notificar = notificar;
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
                consumerTag: "produto-notificado-consumer",
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

            string? mensagem;
            try
            {
                mensagem = JsonSerializer.Deserialize<string>(Encoding.UTF8.GetString(@event.Body.ToArray()));
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON inválido na fila {Fila}. Descartando.", Fila);

                await canal.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            if (string.IsNullOrWhiteSpace(mensagem))
            {
                _logger.LogError("Mensagem fora do contrato na fila {Fila}. Descartando.", Fila);
                await canal.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            try
            {
                await _notificar.Notificar(mensagem, @event.CancellationToken);
                await canal.BasicAckAsync(@event.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao entregar a notificação vinda da fila {Fila}.", Fila);
                await canal.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: true);
            }
        }
    }
}
