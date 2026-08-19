using Adapters.Coletor;
using Rastreador.Contracts;

namespace Scraper.Tests;

public class ScraperGatewayTests
{
    [Fact]
    public async Task BuscarPrecoDoProduto_PreservaOIdDoProdutoRecebido()
    {
        var gateway = new ScraperGateway();

        var resultado = await gateway.BuscarPrecoDoProduto(new ProdutoContratoDto(IdProduto: 7, Url: "http://loja/monitor"));

        Assert.Equal(7, resultado.IdProduto);
    }

    [Fact]
    public async Task BuscarPrecoDoProduto_RetornaPrecoPositivo()
    {
        var gateway = new ScraperGateway();

        var resultado = await gateway.BuscarPrecoDoProduto(new ProdutoContratoDto(IdProduto: 7, Url: "http://loja/monitor"));

        Assert.True(resultado.Preco > 0, $"Preço deveria ser positivo, veio {resultado.Preco}.");
    }
}
