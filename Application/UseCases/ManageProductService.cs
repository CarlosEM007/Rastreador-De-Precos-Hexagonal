using Application.Dto;
using Application.Ports.Driven;
using Application.Ports.Driving;
using Domain.Entities;

namespace Application.UseCases
{
    public class ManageProductService: IManageProductsService
    {
        private readonly IProductRepository _repository;
        private readonly IScraperAdapter _adapter;

        public ManageProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public void RegistrarProduto(ProductDto product)
        {
            var produto = new Produto(
                product.nome,
                product.url,
                product.precoAtual ?? 0,
                product.precoAlvo ?? 0);

            _repository.SalvarProduto(produto);
            _adapter.SendProduct(produto.EnderecoProduto);
        }
    }
}
