using Adapters.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Adapters.Persistence
{
    public static class MigrationRunner
    {
        /// <summary>
        /// Aplica as migrations pendentes no start. Sem isso, um `docker compose up` do zero
        /// sobe com o banco vazio e a primeira chamada quebra: as tabelas não existem.
        /// </summary>
        public static async Task AplicarMigrations(this IServiceProvider services, CancellationToken cancellationToken = default)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(MigrationRunner));

            var pendentes = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

            if (pendentes.Count == 0)
            {
                logger.LogInformation("Banco já está na última migration.");
                return;
            }

            logger.LogInformation("Aplicando {Quantidade} migration(s): {Migrations}", pendentes.Count, string.Join(", ", pendentes));

            await context.Database.MigrateAsync(cancellationToken);
        }
    }
}
