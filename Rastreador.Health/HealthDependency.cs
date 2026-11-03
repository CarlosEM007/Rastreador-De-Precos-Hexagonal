using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Rastreador.Health
{
    public static class HealthDependency
    {
        public static void AddHeartbeat<TProbe>(this IServiceCollection services, IConfiguration configuration)
            where TProbe : class, IHealthProbe
        {
            var secao = configuration.GetSection("Heartbeat");

            services.AddSingleton(new HeartbeatOptions
            {
                Caminho = secao["Caminho"] ?? "/tmp/healthy",
                Intervalo = TimeSpan.FromSeconds(
                    double.TryParse(secao["IntervaloEmSegundos"], out var segundos) && segundos > 0 ? segundos : 15)
            });

            services.AddSingleton<IHealthProbe, TProbe>();
            services.AddHostedService<HeartbeatService>();
        }
    }
}
