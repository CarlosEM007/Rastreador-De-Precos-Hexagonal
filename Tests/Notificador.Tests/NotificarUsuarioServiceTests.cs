using Application.Ports.Driven;
using Application.UseCases;
using Moq;

namespace Notificador.Tests;

public class NotificarUsuarioServiceTests
{
    private readonly Mock<IEnviadorDeAlerta> _enviador = new();

    private NotificarUsuarioService CriarServico() => new(_enviador.Object);

    [Fact]
    public async Task Notificar_MensagemValida_EntregaParaOEnviador()
    {
        await CriarServico().Notificar("O produto Monitor atingiu o preço de R$75");

        _enviador.Verify(e => e.EnviarAlerta(
            "O produto Monitor atingiu o preço de R$75",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Notificar_MensagemVazia_FalhaENaoEnvia(string mensagem)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CriarServico().Notificar(mensagem));

        _enviador.Verify(e => e.EnviarAlerta(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Notificar_MensagemNula_FalhaENaoEnvia()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CriarServico().Notificar(null!));

        _enviador.Verify(e => e.EnviarAlerta(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Notificar_EnviadorFalha_PropagaOErro()
    {
        // O consumer precisa ver a exceção: é ela que decide se a mensagem
        // volta pra fila ou é descartada.
        _enviador.Setup(e => e.EnviarAlerta(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new InvalidOperationException("SMTP fora do ar"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => CriarServico().Notificar("qualquer coisa"));
    }
}
