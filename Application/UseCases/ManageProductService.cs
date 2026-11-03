using Application.Dto;
using Application.Ports.Driven;
using Application.Ports.Driving;
using Domain.Entities;

namespace Application.UseCases
{
    public class ManageProductService: IManageProductsService
    {
        private readonly IProductRepository _repository;
        private readonly IScraperGateway _gateway;

        public ManageProductService(IProductRepository repository, IScraperGateway gateway)
        {
            _repository = repository;
            _gateway = gateway;
        }

        public async Task RegistrarProduto(ProductDto product)
        {
            var produto = new Produto(
                product.nome ?? "",
                product.url ?? "",
                product.precoAtual ?? 0,
                product.precoAlvo ?? 0);

            await _repository.SalvarProduto(produto);
        }

        public async Task<ProdutoResumoDto> ObterProduto(long id)
        {
            var produto = await _repository.BuscarProduto(id);

            return Resumir(produto);
        }

        public async Task<IReadOnlyList<ProdutoResumoDto>> ListarProdutos()
        {
            var produtos = await _repository.ListarProdutos();

            return produtos.Select(Resumir).ToList();
        }

        public async Task AtualizarProduto(long id, ProductDto product)
        {
            var produto = await _repository.BuscarProduto(id);

            produto.AtualizarCadastro(
                product.nome ?? "",
                product.url ?? "",
                product.precoAlvo ?? 0);

            await _repository.AtualizarProduto(produto);
        }

        public async Task RemoverProduto(long id)
        {
            // Passa pelo BuscarProduto para que remover um id inexistente vire 404,
            // e não um "ok" silencioso.
            await _repository.BuscarProduto(id);

            await _repository.RemoverProduto(id);
        }

        public async Task ConsultarPrecoProduto(long id)
        {
            var produto = await _repository.BuscarProduto(id);

            await _gateway.RastrearPrecoProduto(produto.Id, produto.EnderecoProduto ?? "");
        }

        private static ProdutoResumoDto Resumir(Produto produto) =>
            new(produto.Id, produto.Nome, produto.EnderecoProduto, produto.PrecoAtual, produto.PrecoAlvo);
    }
}
