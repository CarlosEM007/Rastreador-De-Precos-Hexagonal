namespace Rastreador.Health
{
    public class HeartbeatOptions
    {
        /// <summary>Arquivo que o healthcheck do compose inspeciona.</summary>
        public string Caminho { get; init; } = "/tmp/healthy";

        /// <summary>De quanto em quanto tempo o arquivo é tocado.</summary>
        public TimeSpan Intervalo { get; init; } = TimeSpan.FromSeconds(15);
    }
}
