namespace Application.Dto
{
    public record ProductDto(
        string? nome,
        string? url,
        decimal? precoAtual,
        decimal? precoAlvo);
}
