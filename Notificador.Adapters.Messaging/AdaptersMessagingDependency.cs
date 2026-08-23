using Adapters.Messaging.Consumer;
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

            services.AddSingleton<RabbitMqConnection>();

            services.AddHostedService<ProdutoNotificadoConsumer>();
        }
    }
}
