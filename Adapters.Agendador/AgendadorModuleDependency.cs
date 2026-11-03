using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adapters.Agendador
{
    public static class AgendadorModuleDependency
    {
        public static void AddAgendador(this IServiceCollection services, IConfiguration configuration)
        {
            var secao = configuration.GetSection("Rastreamento");

            var options = new AgendadorOptions
            {
                Intervalo = TimeSpan.FromMinutes(
                    double.TryParse(secao["IntervaloEmMinutos"], out var minutos) && minutos > 0 ? minutos : 5),
                AtrasoInicial = TimeSpan.FromSeconds(
                    double.TryParse(secao["AtrasoInicialEmSegundos"], out var segundos) && segundos >= 0 ? segundos : 20),
                Habilitado = !bool.TryParse(secao["Habilitado"], out var habilitado) || habilitado
            };

            services.AddSingleton(options);

            if (options.Habilitado)
            {
                services.AddHostedService<RastreamentoPeriodicoWorker>();
            }
        }
    }
}
