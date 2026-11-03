using Application.Ports.Driven;
using Application.UseCases;
using Domain.Entities;
using Moq;

namespace Principal.Tests;

public class RastreamentoServiceTests
{
    private readonly Mock<IProductRepository> _repositorio = new();
    private readonly Mock<IScraperGateway> _scraper = new();

    private RastreamentoService CriarServico() => new(_repositorio.Object, _scraper.Object);

    private void DadosOsProdutos(params Produto[] produtos) =>
        _repositorio.Setup(r => r.ListarProdutos()).ReturnsAsync(produtos);

    [Fact]
    public async Task RastrearTodos_ComProdutosCadastrados_EnfileiraTodosEles()
    {
        DadosOsProdutos(
            new Produto("Monitor", "http://loja/monitor", 100, 80) { Id = 1 },
            new Produto("Teclado", "http://loja/teclado", 200, 150) { Id = 2 });

        var enfileirados = await CriarServico().RastrearTodos();

        Assert.Equal(2, enfileirados);
        _scraper.Verify(s => s.RastrearPrecoProduto(1, "http://loja/monitor"), Times.Once);
        _scraper.Verify(s => s.RastrearPrecoProduto(2, "http://loja/teclado"), Times.Once);
    }

    [Fact]
    public async Task RastrearTodos_ProdutoSemUrl_EhIgnorado()
    {
        DadosOsProdutos(
            new Produto { Id = 1, EnderecoProduto = null },
            new Produto("Teclado", "http://loja/teclado", 200, 150) { Id = 2 });

        var enfileirados = await CriarServico().RastrearTodos();

        Assert.Equal(1, enfileirados);
        _scraper.Verify(s => s.RastrearPrecoProduto(1, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RastrearTodos_SemProdutos_NaoChamaOScraper()
    {
        DadosOsProdutos();

        var enfileirados = await CriarServico().RastrearTodos();

        Assert.Equal(0, enfileirados);
        _scraper.Verify(s => s.RastrearPrecoProduto(It.IsAny<long>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RastrearTodos_Cancelado_Interrompe()
    {
        DadosOsProdutos(new Produto("Monitor", "http://loja/monitor", 100, 80) { Id = 1 });

        using var cancelado = new CancellationTokenSource();
        await cancelado.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => CriarServico().RastrearTodos(cancelado.Token));
    }
}
