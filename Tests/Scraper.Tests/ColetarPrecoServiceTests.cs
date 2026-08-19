using Application.Ports.Driven;
using Application.UseCases;
using Moq;
using Rastreador.Contracts;

namespace Scraper.Tests;

public class ColetarPrecoServiceTests
{
    private readonly Mock<IColetorDePreco> _coletor = new();
    private readonly Mock<IPrecoPublisher> _publisher = new();

    private ColetarPrecoService CriarServico() => new(_coletor.Object, _publisher.Object);

    [Fact]
    public async Task ColetarPrecoProduto_PrecoColetado_PublicaOResultado()
    {
        var entrada = new ProdutoContratoDto(IdProduto: 7, Url: "http://loja/monitor");
        var coletado = new ProdutoPrecoDto(IdProduto: 7, Preco: 42);
        _coletor.Setup(c => c.BuscarPrecoDoProduto(entrada)).ReturnsAsync(coletado);

        await CriarServico().ColetarPrecoProduto(entrada);

        _publisher.Verify(p => p.PublicarPreco(coletado), Times.Once);
    }

    [Fact]
    public async Task ColetarPrecoProduto_ColetorFalha_NaoPublicaNada()
    {
        var entrada = new ProdutoContratoDto(IdProduto: 7, Url: "http://loja/monitor");
        _coletor.Setup(c => c.BuscarPrecoDoProduto(entrada)).ThrowsAsync(new HttpRequestException("loja fora do ar"));

        await Assert.ThrowsAsync<HttpRequestException>(() => CriarServico().ColetarPrecoProduto(entrada));

        _publisher.Verify(p => p.PublicarPreco(It.IsAny<ProdutoPrecoDto>()), Times.Never);
    }
}
