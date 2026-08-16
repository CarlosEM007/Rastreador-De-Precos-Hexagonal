using Application.Ports.Driven;
using Microsoft.Extensions.DependencyInjection;

namespace Adapters.Coletor
{
    public static class ColetorModuleDependency
    {
        public static void AddColetorModule(this IServiceCollection services)
        {
            services.AddSingleton<IColetorDePreco, ScraperGateway>();
        }
    }
}
