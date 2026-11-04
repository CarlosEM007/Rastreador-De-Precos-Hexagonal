using System.Net;
using System.Text;
using Adapters.Coletor;
using Microsoft.Extensions.Logging.Abstractions;
using Rastreador.Contracts;

namespace Scraper.Tests;

public class ColetorHttpTests
{
    /// <summary>
    /// Devolve um HTML fixo sem sair para a rede: o que está sob teste é a extração do
    /// preço, não o HttpClient.
    /// </summary>
    private sealed class HtmlFixoHandler : HttpMessageHandler
    {
        private readonly string _html;

        public HtmlFixoHandler(string html) => _html = html;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_html, Encoding.UTF8, "text/html")
            });
    }

    private static ColetorHttp CriarColetor(string html) =>
        new(new HttpClient(new HtmlFixoHandler(html)), NullLogger<ColetorHttp>.Instance);

    private static ProdutoContratoDto Produto => new(IdProduto: 7, Url: "http://loja/monitor");

    [Fact]
    public async Task BuscarPrecoDoProduto_PrecoEmJsonLd_ExtraiOValor()
    {
        var html = """<script type="application/ld+json">{"@type":"Product","offers":{"price":"199.90"}}</script>""";

        var resultado = await CriarColetor(html).BuscarPrecoDoProduto(Produto);

        Assert.Equal(199.90m, resultado.Preco);
    }

    [Fact]
    public async Task BuscarPrecoDoProduto_PrecoEmMetaTag_ExtraiOValor()
    {
        var html = """<meta property="product:price:amount" content="89.99" />""";

        var resultado = await CriarColetor(html).BuscarPrecoDoProduto(Produto);

        Assert.Equal(89.99m, resultado.Preco);
    }

    [Fact]
    public async Task BuscarPrecoDoProduto_PrecoNoFormatoBrasileiro_InterpretaVirgulaComoDecimal()
    {
        var html = """<meta itemprop="price" content="1.234,56" />""";

        var resultado = await CriarColetor(html).BuscarPrecoDoProduto(Produto);

        Assert.Equal(1234.56m, resultado.Preco);
    }

    [Fact]
    public async Task BuscarPrecoDoProduto_PrecoNoFormatoAmericano_InterpretaPontoComoDecimal()
    {
        var html = """<meta itemprop="price" content="1,234.56" />""";

        var resultado = await CriarColetor(html).BuscarPrecoDoProduto(Produto);

        Assert.Equal(1234.56m, resultado.Preco);
    }

    [Fact]
    public async Task BuscarPrecoDoProduto_PaginaSemPreco_Falha()
    {
        var coletor = CriarColetor("<html><body>Produto indisponível</body></html>");

        await Assert.ThrowsAsync<InvalidOperationException>(() => coletor.BuscarPrecoDoProduto(Produto));
    }

    [Fact]
    public async Task BuscarPrecoDoProduto_ProdutoSemUrl_Falha()
    {
        var coletor = CriarColetor("<html></html>");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => coletor.BuscarPrecoDoProduto(new ProdutoContratoDto(IdProduto: 7, Url: null)));
    }
}
