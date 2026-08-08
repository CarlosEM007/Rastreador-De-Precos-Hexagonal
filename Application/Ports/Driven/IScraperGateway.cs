namespace Application.Ports.Driven
{
    public interface IScraperGateway
    {
        Task RastrearPrecoProduto(long idProduto, string url);
    }
}
