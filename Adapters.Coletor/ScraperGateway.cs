using Application.Ports.Driven;
using Rastreador.Contracts;

namespace Adapters.Coletor;

public class ScraperGateway : IColetorDePreco
{
    public Task<ProdutoPrecoDto> BuscarPrecoDoProduto(ProdutoContratoDto produto)
    {
        decimal valorAtualDoProduto = Convert.ToDecimal(Random.Shared.Next(1, 50));

        return Task.FromResult(new ProdutoPrecoDto(produto.IdProduto, valorAtualDoProduto));
    }
}
