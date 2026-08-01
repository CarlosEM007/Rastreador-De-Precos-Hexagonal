using Adapters.Scraping.Gateway;
using Application.Ports.Driven;
using Microsoft.Extensions.DependencyInjection;

namespace Adapters.Scraping
{
    public static class AdapterGatewayDependency
    {
        public static void AddAdapterGateway(this IServiceCollection services)
        {
            services.AddTransient<IScraperGateway, ScraperGateway>();
        }
    }
}
