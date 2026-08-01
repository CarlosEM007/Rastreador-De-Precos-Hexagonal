using Adapters.Persistence.Repository;
using Application.Ports.Driven;
using Microsoft.Extensions.DependencyInjection;

namespace Adapters.Persistence
{
    public static class AdaptersPersistenceDependency
    {
        public static void AddAdaptersPersistence(this IServiceCollection services)
        {
            services.AddTransient<IProductRepository, ProdutoRepository>();
        }
    }
}
