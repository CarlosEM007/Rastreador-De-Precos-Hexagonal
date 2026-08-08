using RabbitMQ.Client;

namespace Adapters.Messaging
{
    /// <summary>
    /// Dona da conexão com o RabbitMQ. Registrada como singleton: a conexão é cara de abrir,
    /// então é criada uma vez e reaproveitada. Quem consome não descarta — só ela descarta,
    /// no encerramento da aplicação.
    /// </summary>
    public class RabbitMqConnection : IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public RabbitMqConnection(ConnectionFactory factory) => _factory = factory;

        public async Task<IConnection> GetAsync()
        {
            if (_connection is { IsOpen: true }) return _connection;

            await _lock.WaitAsync();
            try
            {
                if (_connection is { IsOpen: true }) return _connection;
                _connection = await _factory.CreateConnectionAsync();
                return _connection;
            }
            finally { _lock.Release(); }
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
