using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Rastreador.Health
{
    /// <summary>
    /// Os workers não falam HTTP, então não há endpoint para o compose chamar. A saída é
    /// inverter: o worker toca um arquivo enquanto estiver saudável, e o healthcheck olha
    /// a data de modificação desse arquivo. Arquivo velho = serviço travado.
    /// </summary>
    public class HeartbeatService : BackgroundService
    {
        private readonly IHealthProbe _probe;
        private readonly HeartbeatOptions _options;
        private readonly ILogger<HeartbeatService> _logger;

        public HeartbeatService(IHealthProbe probe, HeartbeatOptions options, ILogger<HeartbeatService> logger)
        {
            _probe = probe;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(_options.Intervalo);

            try
            {
                do
                {
                    await Pulsar(stoppingToken);
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
                // Encerramento normal do host. O arquivo para de ser tocado e, em um
                // intervalo, o container passa a ser reportado como unhealthy.
            }
        }

        private async Task Pulsar(CancellationToken stoppingToken)
        {
            try
            {
                if (await _probe.EstaSaudavel(stoppingToken))
                {
                    // Reescrever o arquivo é o que atualiza o mtime — é esse carimbo,
                    // e não o conteúdo, que o healthcheck lê.
                    await File.WriteAllTextAsync(_options.Caminho, DateTimeOffset.UtcNow.ToString("O"), stoppingToken);
                }
                else
                {
                    _logger.LogWarning("Probe reportou o serviço como não saudável. Heartbeat não foi atualizado.");
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao escrever o heartbeat em {Caminho}.", _options.Caminho);
            }
        }
    }
}
