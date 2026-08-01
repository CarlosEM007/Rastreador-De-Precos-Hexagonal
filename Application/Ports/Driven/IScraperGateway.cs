namespace Application.Ports.Driven
{
    public interface IScraperGateway
    {
        Task<decimal> ConsultarPrecoProduto(string Url);
    }
}
