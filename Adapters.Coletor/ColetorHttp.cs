using System.Globalization;
using System.Text.RegularExpressions;
using Application.Ports.Driven;
using Microsoft.Extensions.Logging;
using Rastreador.Contracts;

namespace Adapters.Coletor;

/// <summary>
/// Coletor de verdade: baixa a página e procura o preço nos lugares onde as lojas publicam
/// dado estruturado para o Google — JSON-LD e meta tags do Open Graph / schema.org. É muito
/// mais estável do que caçar classe de CSS, que muda a cada redesign.
/// </summary>
public class ColetorHttp : IColetorDePreco
{
    // Ordem importa: do mais confiável (dado estruturado declarado pela própria loja)
    // para o menos.
    private static readonly Regex[] Padroes =
    [
        new(@"""price""\s*:\s*""?(?<preco>[\d.,]+)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"<meta[^>]+(?:property|name)=""(?:product:price:amount|og:price:amount)""[^>]+content=""(?<preco>[\d.,]+)""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"<meta[^>]+itemprop=""price""[^>]+content=""(?<preco>[\d.,]+)""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled)
    ];

    private readonly HttpClient _http;
    private readonly ILogger<ColetorHttp> _logger;

    public ColetorHttp(HttpClient http, ILogger<ColetorHttp> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ProdutoPrecoDto> BuscarPrecoDoProduto(ProdutoContratoDto produto)
    {
        if (string.IsNullOrWhiteSpace(produto.Url))
        {
            throw new InvalidOperationException($"Produto {produto.IdProduto} não tem URL para rastrear.");
        }

        var html = await _http.GetStringAsync(produto.Url);

        if (!TentarExtrairPreco(html, out var preco))
        {
            throw new InvalidOperationException(
                $"Não encontrei preço na página do produto {produto.IdProduto} ({produto.Url}).");
        }

        _logger.LogInformation("Produto {IdProduto}: preço coletado R${Preco}.", produto.IdProduto, preco);

        return new ProdutoPrecoDto(produto.IdProduto, preco);
    }

    private static bool TentarExtrairPreco(string html, out decimal preco)
    {
        foreach (var padrao in Padroes)
        {
            var match = padrao.Match(html);

            if (match.Success && TentarConverter(match.Groups["preco"].Value, out preco))
            {
                return true;
            }
        }

        preco = 0;
        return false;
    }

    /// <summary>
    /// "1.234,56" (pt-BR) e "1,234.56" (en-US) querem dizer a mesma coisa. O separador
    /// decimal é o último ponto ou vírgula que aparece; o resto é separador de milhar.
    /// </summary>
    private static bool TentarConverter(string texto, out decimal preco)
    {
        preco = 0;

        if (string.IsNullOrWhiteSpace(texto)) return false;

        var ultimoPonto = texto.LastIndexOf('.');
        var ultimaVirgula = texto.LastIndexOf(',');

        string normalizado;

        if (ultimoPonto < 0 && ultimaVirgula < 0)
        {
            normalizado = texto;
        }
        else if (ultimaVirgula > ultimoPonto)
        {
            normalizado = texto.Replace(".", "").Replace(',', '.');
        }
        else
        {
            normalizado = texto.Replace(",", "");
        }

        return decimal.TryParse(normalizado, NumberStyles.Any, CultureInfo.InvariantCulture, out preco)
               && preco > 0;
    }
}
