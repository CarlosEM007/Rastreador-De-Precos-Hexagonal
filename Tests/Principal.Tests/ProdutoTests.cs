using Domain.Entities;
using Domain.Exceptions;

namespace Principal.Tests;

public class ProdutoTests
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Construtor_UrlVazia_LancaInvalidProductURLException(string? url)
    {
        Assert.Throws<InvalidProductURLException>(
            () => new Produto("Monitor", url!, precoAtual: 100, precoAlvo: 80));
    }

    [Theory]
    [InlineData(80, 80)]   // igual
    [InlineData(70, 80)]   // menor
    public void Construtor_PrecoAtualNaoMaiorQueAlvo_LancaInvalidProductValueException(
        decimal precoAtual, decimal precoAlvo)
    {
        Assert.Throws<InvalidProductValueException>(
            () => new Produto("Monitor", "http://loja/monitor", precoAtual, precoAlvo));
    }

    [Fact]
    public void Construtor_DadosValidos_PreencheAsPropriedades()
    {
        var produto = new Produto("Monitor", "http://loja/monitor", precoAtual: 100, precoAlvo: 80);

        Assert.Equal("Monitor", produto.Nome);
        Assert.Equal("http://loja/monitor", produto.EnderecoProduto);
        Assert.Equal(100, produto.PrecoAtual);
        Assert.Equal(80, produto.PrecoAlvo);
    }

    [Theory]
    [InlineData(79)]   // abaixo do alvo
    [InlineData(80)]   // exatamente no alvo
    public void AtingiuPrecoAlvo_PrecoMenorOuIgualAoAlvo_RetornaTrueComMensagem(decimal precoColetado)
    {
        var produto = new Produto("Monitor", "http://loja/monitor", precoAtual: 100, precoAlvo: 80);

        var atingiu = produto.AtingiuPrecoAlvo(precoColetado, out var mensagem);

        Assert.True(atingiu);
        Assert.Contains("Monitor", mensagem);
    }

    [Fact]
    public void AtingiuPrecoAlvo_PrecoAcimaDoAlvo_RetornaFalseSemMensagem()
    {
        var produto = new Produto("Monitor", "http://loja/monitor", precoAtual: 100, precoAlvo: 80);

        var atingiu = produto.AtingiuPrecoAlvo(precoRecebidoDoScraper: 81, out var mensagem);

        Assert.False(atingiu);
        Assert.Empty(mensagem);
    }
}
