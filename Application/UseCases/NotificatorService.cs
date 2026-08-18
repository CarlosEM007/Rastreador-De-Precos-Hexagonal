using Application.Ports.Driven;
using Application.Ports.Driving;
using Domain.Entities;
using Rastreador.Contracts;

namespace Application.UseCases
{
    public class NotificatorService : INotificatorService
    {
        private readonly IProductRepository _repository;
        private readonly INotificationGateway _gateway;

        public NotificatorService(IProductRepository repository, INotificationGateway gateway)
        {
            _repository = repository;
            _gateway = gateway;
        }

        public async  Task NotificarPrecoEncontrado(ProdutoPrecoDto produto)
        {
            Produto p = await _repository.BuscarProduto(produto.IdProduto);

            if(p.AtingiuPrecoAlvo(produto.Preco, out string message))
            {
                await _gateway.NotificarPrecoAlvoAtingido(message);
            }
        }
    }
}
