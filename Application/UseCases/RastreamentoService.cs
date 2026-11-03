using Application.Ports.Driven;
using Application.Ports.Driving;

namespace Application.UseCases
{
    public class RastreamentoService : IRastreamentoService
    {
        private readonly IProductRepository _repository;
        private readonly IScraperGateway _gateway;

        public RastreamentoService(IProductRepository repository, IScraperGateway gateway)
        {
            _repository = repository;
            _gateway = gateway;
        }

        public async Task<int> RastrearTodos(CancellationToken cancellationToken = default)
        {
            var produtos = await _repository.ListarProdutos();

            var enfileirados = 0;

            foreach (var produto in produtos)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(produto.EnderecoProduto))
                {
                    continue;
                }

                await _gateway.RastrearPrecoProduto(produto.Id, produto.EnderecoProduto);
                enfileirados++;
            }

            return enfileirados;
        }
    }
}
