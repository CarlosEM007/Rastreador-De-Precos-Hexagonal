using Adapters.Messaging.Consumer;
using Adapters.Messaging.Publisher;
using Application.Ports.Driven;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Rastreador.Health;

namespace Adapters.Messaging
{
    public static class AdaptersMessagingDependency
    {
        public static void AddAdaptersMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            var secao = configuration.GetSection("RabbitMq");

            services.AddSingleton(new ConnectionFactory
            {
                HostName = secao["Host"] ?? "localhost",
                UserName = secao["User"] ?? "guest",
                Password = secao["Password"] ?? "guest",
                Port = int.TryParse(secao["Port"], out var porta) ? porta : AmqpTcpEndpoint.UseDefaultPort
            });

            services.AddSingleton<RabbitMqConnection>();
            services.AddSingleton<IPrecoPublisher, PrecoPublisher>();
            services.AddHostedService<ProdutoParaRastrearConsumer>();

            // Worker não fala HTTP: o healthcheck do compose lê um arquivo de heartbeat.
            services.AddHeartbeat<RabbitMqHealthProbe>(configuration);
        }
    }
}
