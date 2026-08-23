using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Adapters.Messaging
{
    public class RabbitMqConnection : IAsyncDisposable
    {
        private const int MaxTentativas = 10;
        private const double EsperaMaximaEmSegundos = 30;

        private readonly ConnectionFactory _factory;
        private readonly ILogger<RabbitMqConnection> _logger;
        private IConnection? _connection;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public RabbitMqConnection(ConnectionFactory factory, ILogger<RabbitMqConnection> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task<IConnection> GetAsync(CancellationToken cancellationToken = default)
        {
            if (_connection is { IsOpen: true }) return _connection;

            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_connection is { IsOpen: true }) return _connection;
                _connection = await ConectarComRetentativaAsync(cancellationToken);
                return _connection;
            }
            finally { _lock.Release(); }
        }

        private async Task<IConnection> ConectarComRetentativaAsync(CancellationToken cancellationToken)
        {
            for (var tentativa = 1; ; tentativa++)
            {
                try
                {
                    return await _factory.CreateConnectionAsync(cancellationToken);
                }
                catch (Exception ex) when (tentativa < MaxTentativas && !cancellationToken.IsCancellationRequested)
                {
                    var espera = TimeSpan.FromSeconds(Math.Min(EsperaMaximaEmSegundos, Math.Pow(2, tentativa)));

                    _logger.LogWarning(ex,
                        "Falha ao conectar no RabbitMQ (tentativa {Tentativa}/{Max}). Tentando de novo em {Espera}s.",
                        tentativa, MaxTentativas, espera.TotalSeconds);

                    await Task.Delay(espera, cancellationToken);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

            if (_connection is not null)
            {
                if (_connection.IsOpen)
                {
                    await _connection.CloseAsync();
                }

                await _connection.DisposeAsync();
                _connection = null;
            }

            _lock.Dispose();
        }
    }
}
