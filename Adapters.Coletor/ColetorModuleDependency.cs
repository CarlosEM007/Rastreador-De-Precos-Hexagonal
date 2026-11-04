using Application.Ports.Driven;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adapters.Coletor
{
    public static class ColetorModuleDependency
    {
        public static void AddColetorModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Coletor:Modo = Http (padrão) ou Fake. É a mesma porta dos dois lados —
            // trocar o jeito de obter o preço não toca em mais nada do sistema.
            var modo = configuration.GetSection("Coletor")["Modo"];

            if (string.Equals(modo, "Fake", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingleton<IColetorDePreco, ColetorFake>();
                return;
            }

            services.AddHttpClient<IColetorDePreco, ColetorHttp>(http =>
            {
                http.Timeout = TimeSpan.FromSeconds(20);

                // Sem User-Agent de browser, boa parte das lojas devolve 403.
                http.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36");
                http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("pt-BR,pt;q=0.9,en;q=0.8");
            });
        }
    }
}
