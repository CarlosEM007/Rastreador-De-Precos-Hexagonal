using Application.Ports.Driven;
using Rastreador.Contracts;

namespace Adapters.Coletor;

/// <summary>
/// Devolve um preço aleatório, sem tocar na rede. É o coletor usado em desenvolvimento:
/// deixa o fluxo inteiro (fila → comparação → notificação) rodar sem depender de nenhum site.
/// </summary>
public class ColetorFake : IColetorDePreco
{
    public Task<ProdutoPrecoDto> BuscarPrecoDoProduto(ProdutoContratoDto produto)
    {
        decimal valorAtualDoProduto = Convert.ToDecimal(Random.Shared.Next(1, 50));

        return Task.FromResult(new ProdutoPrecoDto(produto.IdProduto, valorAtualDoProduto));
    }
}
