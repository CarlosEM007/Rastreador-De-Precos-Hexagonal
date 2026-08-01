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
        private readonly INotificationGateway _notification;

        public ManageProductService(IProductRepository repository, IScraperGateway gateway, INotificationGateway notification)
        {
            _repository = repository;
            _gateway = gateway;
            _notification = notification;
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

            decimal precoRetornado = await _gateway.ConsultarPrecoProduto(produto.EnderecoProduto ?? "");

            if(produto.AtingiuPrecoAlvo(precoRetornado, out string Mensagem))
            {
                await _notification.NotificarPrecoAlvoAtingido(Mensagem);
            }
        }
    }
}
