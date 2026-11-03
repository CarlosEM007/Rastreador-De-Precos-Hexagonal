using Application.Ports.Driving;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Adapters.Agendador
{
    /// <summary>
    /// É o que faz o rastreador rastrear sozinho. De tempos em tempos, enfileira todos os
    /// produtos cadastrados para o Scraper — sem isso, o ciclo só anda quando alguém chama
    /// o endpoint de rastreamento na mão.
    /// </summary>
    public class RastreamentoPeriodicoWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AgendadorOptions _options;
        private readonly ILogger<RastreamentoPeriodicoWorker> _logger;

        public RastreamentoPeriodicoWorker(
            IServiceScopeFactory scopeFactory,
            AgendadorOptions options,
            ILogger<RastreamentoPeriodicoWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Agendador ligado: rastreando todos os produtos a cada {Intervalo}, começando em {Atraso}.",
                _options.Intervalo, _options.AtrasoInicial);

            try
            {
                await Task.Delay(_options.AtrasoInicial, stoppingToken);

                // PeriodicTimer não acumula disparos atrasados: se uma rodada demorar mais que
                // o intervalo, a próxima simplesmente acontece depois, sem fila de tiques.
                using var timer = new PeriodicTimer(_options.Intervalo);

                do
                {
                    await ExecutarRodada(stoppingToken);
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
                // Encerramento normal do host.
            }
        }

        private async Task ExecutarRodada(CancellationToken stoppingToken)
        {
            try
            {
                // Escopo próprio por rodada: o IRastreamentoService depende do repositório,
                // que depende de um AppDbContext (scoped).
                using var scope = _scopeFactory.CreateScope();
                var rastreamento = scope.ServiceProvider.GetRequiredService<IRastreamentoService>();

                var enfileirados = await rastreamento.RastrearTodos(stoppingToken);

                _logger.LogInformation("Rodada de rastreamento: {Enfileirados} produto(s) enviados para o Scraper.", enfileirados);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Uma rodada que falha não pode derrubar o agendador: na próxima ele tenta de novo.
                _logger.LogError(ex, "Falha na rodada de rastreamento. Tentando de novo no próximo ciclo.");
            }
        }
    }
}
