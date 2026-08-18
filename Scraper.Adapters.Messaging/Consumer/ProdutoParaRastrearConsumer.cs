using Application.Ports.Driving;
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
    public class ProdutoParaRastrearConsumer : BackgroundService
    {
        private const string Fila = Queues.ProdutosParaRastrear;

        private readonly RabbitMqConnection _connection;
        private readonly IColetarPreco _coletar;
        private readonly ILogger<ProdutoParaRastrearConsumer> _logger;

        // O ack precisa sair pelo mesmo channel que entregou a mensagem.
        private IChannel? _channel;

        public ProdutoParaRastrearConsumer(
            RabbitMqConnection connection,
            IColetarPreco coletar,
            ILogger<ProdutoParaRastrearConsumer> logger)
        {
            _connection = connection;
            _coletar = coletar;
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

            // Sem isso o broker empurra a fila inteira de uma vez: com autoAck: false,
            // toda mensagem não confirmada fica pendurada na memória do consumer.
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
                consumerTag: "produto-para-rastrear-consumer",
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

            ProdutoContratoDto? produto;
            try
            {
                produto = JsonSerializer.Deserialize<ProdutoContratoDto>(Encoding.UTF8.GetString(@event.Body.ToArray()));
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON inválido na fila {Fila}. Descartando.", Fila);

                await canal.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            if (produto is null || string.IsNullOrWhiteSpace(produto.Url))
            {
                _logger.LogError("Mensagem fora do contrato na fila {Fila}. Descartando.", Fila);
                await canal.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            try
            {
                await _coletar.ColetarPrecoProduto(produto);
                await canal.BasicAckAsync(@event.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao coletar preço do produto {IdProduto}.", produto.IdProduto);
                await canal.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: true);
            }
        }
    }
}
