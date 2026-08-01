using Application.Ports.Driven;

namespace Adapters.Scraping.Gateway
{
    /// <summary>
    /// Classe apenas para Mockar um valor
    /// Futuramente eu faço a busca real de valores
    /// Afinal, é um projeto de estudo de arquitetura.
    /// </summary>
    public class ScraperGateway : IScraperGateway
    {
        public Task<decimal> ConsultarPrecoProduto(string Url)
        {
            decimal valorAtualDoProduto = Convert.ToDecimal(Random.Shared.Next(1, 50));

            return Task.FromResult(valorAtualDoProduto);
        }
    }
}
