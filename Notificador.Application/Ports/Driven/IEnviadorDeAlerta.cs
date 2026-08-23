namespace Application.Ports.Driven
{
    /// <summary>
    /// Porta de saída: quem de fato entrega o alerta. Hoje é um "e-mail" fake no console;
    /// trocar por SMTP, push ou SMS é só escrever outro adapter e mudar o registro na DI.
    /// </summary>
    public interface IEnviadorDeAlerta
    {
        Task EnviarAlerta(string mensagem, CancellationToken cancellationToken = default);
    }
}
