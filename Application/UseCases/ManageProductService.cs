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

        public async Task ConsultarPrecoProduto(long id)
        {
            var produto = await _repository.BuscarProduto(id);

            await _gateway.RastrearPrecoProduto(produto.Id, produto.EnderecoProduto ?? "");
        }
    }
}
