using Adapters.Messaging.Consumer;
using Adapters.Messaging.Publisher;
using Application.Ports.Driven;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

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

            // Conexão
            services.AddSingleton<RabbitMqConnection>();

            // Publisher
            services.AddSingleton<IScraperGateway, UrlPublisher>();
            services.AddSingleton<INotificationGateway, PriceNotificationPublisher>();

            // Consumer
            services.AddHostedService<PrecoColetadoConsumer>();
        }
    }
}
