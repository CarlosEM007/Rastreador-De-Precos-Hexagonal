namespace Application.Ports.Driving
{
    /// <summary>
    /// Porta usada pelo agendador: enfileira todos os produtos cadastrados para o Scraper.
    /// </summary>
    public interface IRastreamentoService
    {
        Task<int> RastrearTodos(CancellationToken cancellationToken = default);
    }
}
