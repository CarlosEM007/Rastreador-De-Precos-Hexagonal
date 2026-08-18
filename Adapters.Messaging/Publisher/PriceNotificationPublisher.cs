using Application.Ports.Driven;
using RabbitMQ.Client;
using Rastreador.Contracts;
using Rastreador.Queues;
using System.Text;
using System.Text.Json;

namespace Adapters.Messaging.Publisher
{
    public class PriceNotificationPublisher : INotificationGateway
    {
        private const string Fila = Queues.ProdutoNotificado;

        private readonly RabbitMqConnection _connection;

        public PriceNotificationPublisher(RabbitMqConnection connection) => _connection = connection;

        public async Task NotificarPrecoAlvoAtingido(string Message)
        {
            // Conexão compartilhada: não é descartada aqui. O channel sim, é por operação.
            var connection = await _connection.GetAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: Fila,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Message));
            var props = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: Fila,
                mandatory: false,
                basicProperties: props,
                body: body);
        }
    }
}
