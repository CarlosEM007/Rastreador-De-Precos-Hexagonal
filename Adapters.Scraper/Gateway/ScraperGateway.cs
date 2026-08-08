using Application.Ports.Driven;

namespace Adapters.Scraping.Gateway
{
    /// <summary>
    /// (Legado)
    /// Classe apenas para Mockar um valor
    /// Futuramente eu faço a busca real de valores
    /// Afinal, é um projeto de estudo de arquitetura.
    /// 
    /// Está aqui apenas para ver a evolução. Agora o sistema usa um serviço para scraping
    /// </summary>
    public class ScraperGateway : IScraperGateway
    {
        public Task RastrearPrecoProduto(long idProduto, string url)
        {
            decimal valorAtualDoProduto = Convert.ToDecimal(Random.Shared.Next(1, 50));

            return Task.FromResult(valorAtualDoProduto);
        }
    }
}
