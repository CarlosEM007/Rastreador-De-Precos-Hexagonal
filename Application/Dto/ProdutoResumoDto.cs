namespace Application.Dto
{
    /// <summary>
    /// O que a API devolve. Separado da entidade de propósito: o Domain não atravessa a borda.
    /// </summary>
    public record ProdutoResumoDto(
        long Id,
        string? Nome,
        string? Url,
        decimal PrecoAtual,
        decimal PrecoAlvo);
}
