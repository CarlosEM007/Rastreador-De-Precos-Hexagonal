using Application.Dto;
using Application.Ports.Driven;
using Application.UseCases;
using Domain.Entities;
using Moq;

namespace Principal.Tests;

public class ManageProductServiceTests
{
    private readonly Mock<IProductRepository> _repositorio = new();
    private readonly Mock<IScraperGateway> _scraper = new();

    private ManageProductService CriarServico() =>
        new(_repositorio.Object, _scraper.Object);

    [Fact]
    public async Task RegistrarProduto_DadosValidos_SalvaNoRepositorio()
    {
        var dto = new ProductDto("Monitor", "http://loja/monitor", precoAtual: 100, precoAlvo: 80);

        await CriarServico().RegistrarProduto(dto);

        _repositorio.Verify(r => r.SalvarProduto(It.Is<Produto>(p =>
            p.Nome == "Monitor" &&
            p.EnderecoProduto == "http://loja/monitor" &&
            p.PrecoAtual == 100 &&
            p.PrecoAlvo == 80)), Times.Once);
    }

    [Fact]
    public async Task ConsultarPrecoProduto_ProdutoExiste_EnviaIdEUrlParaOScraper()
    {
        var produto = new Produto("Monitor", "http://loja/monitor", precoAtual: 100, precoAlvo: 80) { Id = 7 };
        _repositorio.Setup(r => r.BuscarProduto(7)).ReturnsAsync(produto);

        await CriarServico().ConsultarPrecoProduto(7);

        _scraper.Verify(s => s.RastrearPrecoProduto(7, "http://loja/monitor"), Times.Once);
    }

    [Fact]
    public async Task ConsultarPrecoProduto_ProdutoSemUrl_EnviaStringVaziaEmVezDeNull()
    {
        var produto = new Produto { Id = 7, EnderecoProduto = null };
        _repositorio.Setup(r => r.BuscarProduto(7)).ReturnsAsync(produto);

        await CriarServico().ConsultarPrecoProduto(7);

        _scraper.Verify(s => s.RastrearPrecoProduto(7, ""), Times.Once);
    }
}
