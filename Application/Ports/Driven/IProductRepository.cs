using Domain.Entities;

namespace Application.Ports.Driven
{
    public interface IProductRepository
    {
        Task SalvarProduto(Produto produto);
        Task<Produto> BuscarProduto(long id);
        Task<IReadOnlyList<Produto>> ListarProdutos();
        Task AtualizarProduto(Produto produto);
        Task RemoverProduto(long id);
    }
}
