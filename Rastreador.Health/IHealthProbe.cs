namespace Rastreador.Health
{
    /// <summary>
    /// O que cada worker considera "estar vivo". Quem implementa é o serviço,
    /// porque só ele sabe de que depende para trabalhar.
    /// </summary>
    public interface IHealthProbe
    {
        Task<bool> EstaSaudavel(CancellationToken cancellationToken = default);
    }
}
