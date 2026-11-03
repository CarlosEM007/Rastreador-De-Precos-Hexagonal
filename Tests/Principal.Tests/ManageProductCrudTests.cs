using Application.Dto;
using Application.Ports.Driven;
using Application.UseCases;
using Domain.Entities;
using Domain.Exceptions;
using Moq;

namespace Principal.Tests;

public class ManageProductCrudTests
{
    private readonly Mock<IProductRepository> _repositorio = new();
    private readonly Mock<IScraperGateway> _scraper = new();

    private ManageProductService CriarServico() => new(_repositorio.Object, _scraper.Object);

    [Fact]
    public async Task ObterProduto_ProdutoExiste_DevolveOsDados()
    {
        var produto = new Produto("Monitor", "http://loja/monitor", 100, 80) { Id = 7 };
        _repositorio.Setup(r => r.BuscarProduto(7)).ReturnsAsync(produto);

        var resultado = await CriarServico().ObterProduto(7);

        Assert.Equal(7, resultado.Id);
        Assert.Equal("Monitor", resultado.Nome);
        Assert.Equal("http://loja/monitor", resultado.Url);
        Assert.Equal(100, resultado.PrecoAtual);
        Assert.Equal(80, resultado.PrecoAlvo);
    }

    [Fact]
    public async Task ListarProdutos_DevolveTodosOsCadastrados()
    {
        _repositorio.Setup(r => r.ListarProdutos()).ReturnsAsync(new[]
        {
            new Produto("Monitor", "http://loja/monitor", 100, 80) { Id = 1 },
            new Produto("Teclado", "http://loja/teclado", 200, 150) { Id = 2 }
        });

        var resultado = await CriarServico().ListarProdutos();

        Assert.Equal(2, resultado.Count);
        Assert.Collection(resultado,
            p => Assert.Equal("Monitor", p.Nome),
            p => Assert.Equal("Teclado", p.Nome));
    }

    [Fact]
    public async Task AtualizarProduto_DadosValidos_SalvaAsAlteracoes()
    {
        var produto = new Produto("Monitor", "http://loja/monitor", 100, 80) { Id = 7 };
        _repositorio.Setup(r => r.BuscarProduto(7)).ReturnsAsync(produto);

        await CriarServico().AtualizarProduto(7, new ProductDto("Monitor 27pol", "http://loja/monitor-27", null, precoAlvo: 70));

        Assert.Equal("Monitor 27pol", produto.Nome);
        Assert.Equal("http://loja/monitor-27", produto.EnderecoProduto);
        Assert.Equal(70, produto.PrecoAlvo);
        _repositorio.Verify(r => r.AtualizarProduto(produto), Times.Once);
    }

    [Fact]
    public async Task AtualizarProduto_NaoMexeNoPrecoAtual()
    {
        // Quem dita o preço atual é o scraper, não quem edita o cadastro.
        var produto = new Produto("Monitor", "http://loja/monitor", 100, 80) { Id = 7 };
        _repositorio.Setup(r => r.BuscarProduto(7)).ReturnsAsync(produto);

        await CriarServico().AtualizarProduto(7, new ProductDto("Monitor", "http://loja/monitor", precoAtual: 999, precoAlvo: 70));

        Assert.Equal(100, produto.PrecoAtual);
    }

    [Fact]
    public async Task AtualizarProduto_PrecoAlvoInvalido_Falha()
    {
        var produto = new Produto("Monitor", "http://loja/monitor", 100, 80) { Id = 7 };
        _repositorio.Setup(r => r.BuscarProduto(7)).ReturnsAsync(produto);

        await Assert.ThrowsAsync<InvalidProductValueException>(
            () => CriarServico().AtualizarProduto(7, new ProductDto("Monitor", "http://loja/monitor", null, precoAlvo: 0)));

        _repositorio.Verify(r => r.AtualizarProduto(It.IsAny<Produto>()), Times.Never);
    }

    [Fact]
    public async Task RemoverProduto_ProdutoExiste_Remove()
    {
        _repositorio.Setup(r => r.BuscarProduto(7))
                    .ReturnsAsync(new Produto("Monitor", "http://loja/monitor", 100, 80) { Id = 7 });

        await CriarServico().RemoverProduto(7);

        _repositorio.Verify(r => r.RemoverProduto(7), Times.Once);
    }

    [Fact]
    public async Task RemoverProduto_ProdutoNaoExiste_FalhaENaoRemove()
    {
        _repositorio.Setup(r => r.BuscarProduto(7)).ThrowsAsync(new ProductNotFoundException("Produto não encontrado."));

        await Assert.ThrowsAsync<ProductNotFoundException>(() => CriarServico().RemoverProduto(7));

        _repositorio.Verify(r => r.RemoverProduto(It.IsAny<long>()), Times.Never);
    }
}
