using Application.Ports.Driven;
using Application.UseCases;
using Domain.Entities;
using Moq;
using Rastreador.Contracts;

namespace Principal.Tests;

public class NotificatorServiceTests
{
    private readonly Mock<IProductRepository> _repositorio = new();
    private readonly Mock<INotificationGateway> _notificacao = new();

    private NotificatorService CriarServico() =>
        new(_repositorio.Object, _notificacao.Object);

    private Produto DadoOProdutoComAlvo(decimal precoAlvo)
    {
        var produto = new Produto("Monitor", "http://loja/monitor", precoAtual: 100, precoAlvo) { Id = 7 };
        _repositorio.Setup(r => r.BuscarProduto(7)).ReturnsAsync(produto);

        return produto;
    }

    [Fact]
    public async Task NotificarPrecoEncontrado_PrecoAtingiuOAlvo_Notifica()
    {
        DadoOProdutoComAlvo(precoAlvo: 80);

        await CriarServico().NotificarPrecoEncontrado(new ProdutoPrecoDto(IdProduto: 7, Preco: 75));

        _notificacao.Verify(n => n.NotificarPrecoAlvoAtingido(It.Is<string>(m => m.Contains("Monitor"))), Times.Once);
    }

    [Fact]
    public async Task NotificarPrecoEncontrado_PrecoAcimaDoAlvo_NaoNotifica()
    {
        DadoOProdutoComAlvo(precoAlvo: 80);

        await CriarServico().NotificarPrecoEncontrado(new ProdutoPrecoDto(IdProduto: 7, Preco: 95));

        _notificacao.Verify(n => n.NotificarPrecoAlvoAtingido(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task NotificarPrecoEncontrado_PrecoAbaixoDoAlvo_GuardaONovoPreco()
    {
        var produto = DadoOProdutoComAlvo(precoAlvo: 80);

        await CriarServico().NotificarPrecoEncontrado(new ProdutoPrecoDto(IdProduto: 7, Preco: 75));

        Assert.Equal(75, produto.PrecoAtual);
        _repositorio.Verify(r => r.AtualizarProduto(produto), Times.Once);
    }

    [Fact]
    public async Task NotificarPrecoEncontrado_PrecoAcimaDoAlvo_GuardaONovoPrecoMesmoAssim()
    {
        // Mesmo sem alerta, o cadastro precisa refletir o preço que o scraper viu.
        var produto = DadoOProdutoComAlvo(precoAlvo: 80);

        await CriarServico().NotificarPrecoEncontrado(new ProdutoPrecoDto(IdProduto: 7, Preco: 95));

        Assert.Equal(95, produto.PrecoAtual);
        _repositorio.Verify(r => r.AtualizarProduto(produto), Times.Once);
    }
}
