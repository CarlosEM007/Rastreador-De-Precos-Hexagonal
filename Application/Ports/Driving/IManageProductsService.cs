using Application.Dto;

namespace Application.Ports.Driving
{
    public interface IManageProductsService
    {
        Task RegistrarProduto(ProductDto product);
        Task<ProdutoResumoDto> ObterProduto(long id);
        Task<IReadOnlyList<ProdutoResumoDto>> ListarProdutos();
        Task AtualizarProduto(long id, ProductDto product);
        Task RemoverProduto(long id);

        /// <summary>
        /// Dispara o rastreamento sob demanda. O agendador faz o mesmo em loop, para todos.
        /// </summary>
        Task ConsultarPrecoProduto(long id);
    }
}
