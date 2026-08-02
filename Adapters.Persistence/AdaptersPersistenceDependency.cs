using Adapters.Persistence.Data;
using Adapters.Persistence.Repository;
using Application.Ports.Driven;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adapters.Persistence
{
    public static class AdaptersPersistenceDependency
    {
        public static void AddAdaptersPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IProductRepository, ProdutoRepository>();


            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("Postgres"),
                    sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                ));

        }
    }
}
