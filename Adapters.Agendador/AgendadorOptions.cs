namespace Adapters.Agendador
{
    public class AgendadorOptions
    {
        /// <summary>De quanto em quanto tempo todos os produtos são reenfileirados.</summary>
        public TimeSpan Intervalo { get; init; } = TimeSpan.FromMinutes(5);

        /// <summary>Folga no start, para o RabbitMQ e o banco terminarem de acordar.</summary>
        public TimeSpan AtrasoInicial { get; init; } = TimeSpan.FromSeconds(20);

        /// <summary>Permite desligar o agendador sem remover o serviço.</summary>
        public bool Habilitado { get; init; } = true;
    }
}
