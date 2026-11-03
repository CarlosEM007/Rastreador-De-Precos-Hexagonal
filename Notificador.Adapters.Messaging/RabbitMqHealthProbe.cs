using Rastreador.Health;

namespace Adapters.Messaging
{
    /// <summary>
    /// Para um worker que só existe para consumir fila, estar saudável é ter a conexão
    /// com o broker aberta. Processo vivo com conexão caída não é serviço saudável.
    /// </summary>
    public class RabbitMqHealthProbe : IHealthProbe
    {
        private readonly RabbitMqConnection _connection;

        public RabbitMqHealthProbe(RabbitMqConnection connection) => _connection = connection;

        public async Task<bool> EstaSaudavel(CancellationToken cancellationToken = default)
        {
            var connection = await _connection.GetAsync(cancellationToken);

            return connection.IsOpen;
        }
    }
}
