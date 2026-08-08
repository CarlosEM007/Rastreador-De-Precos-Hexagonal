using Application.Ports.Driven;
using RabbitMQ.Client;
using Rastreador.Contracts;
using System.Text;
using System.Text.Json;

namespace Adapters.Messaging.Publisher
{
    public class UrlPublisher : IScraperGateway
    {
        private const string Fila = "produtos-para-rastrear";

        private readonly RabbitMqConnection _connection;

        public UrlPublisher(RabbitMqConnection connection) => _connection = connection;

        public async Task RastrearPrecoProduto(long idProduto, string url)
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

            var produto = new ProdutoContratoDto(idProduto, url);
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
