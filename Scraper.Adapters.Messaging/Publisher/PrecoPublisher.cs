using Application.Ports.Driven;
using RabbitMQ.Client;
using Rastreador.Contracts;
using Rastreador.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Adapters.Messaging.Publisher
{
    public class PrecoPublisher : IPrecoPublisher
    {
        private const string Fila = Queues.PrecosColetados;

        private readonly RabbitMqConnection _connection;

        public PrecoPublisher(RabbitMqConnection connection) => _connection = connection;

        public async Task PublicarPreco(ProdutoPrecoDto produto)
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

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(produto));
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
