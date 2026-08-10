using Application.Ports.Driving;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using Rastreador.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Adapters.Messaging.Consumer
{
    public class ProdutoParaRastrearConsumer : BackgroundService
    {
        private const string Fila = "produtos-para-rastrear";

        private readonly int _intervaloMensagenWorker;
        private readonly RabbitMqConnection _connection;
        private readonly IColetarPreco _coletar;

        public ProdutoParaRastrearConsumer(int intervaloMensagenWorker, RabbitMqConnection connection, IColetarPreco coletar)
        {
            _intervaloMensagenWorker = intervaloMensagenWorker;
            _connection = connection;
            _coletar = coletar;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = await _connection.GetAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: Fila,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += Consumer_ReceivedAsync;

            await channel.BasicConsumeAsync(
                queue: Fila,
                autoAck: true,
                consumer: consumer,
                consumerTag: "produto-para-rastrear-consumer",
                noLocal: false,
                exclusive: false,
                arguments: null
            );

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_intervaloMensagenWorker, stoppingToken);
            }
        }

        private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs @event)
        {
            var produto = JsonSerializer.Deserialize<ProdutoContratoDto>(Encoding.UTF8.GetString(@event.Body.ToArray()));

            await _coletar.ColetarPrecoProduto(produto);
        }
    }
}
