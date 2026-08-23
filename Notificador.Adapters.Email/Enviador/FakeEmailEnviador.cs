using Application.Ports.Driven;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Adapters.Email.Enviador
{
    /// <summary>
    /// "Envia" o alerta desenhando um e-mail no console. Nenhum SMTP é tocado: o objetivo da
    /// fase é provar que o evento atravessou a fila e chegou num serviço independente.
    /// O dia em que virar SMTP de verdade, só esta classe muda — a Application não sabe a diferença.
    /// </summary>
    public class FakeEmailEnviador : IEnviadorDeAlerta
    {
        private const string RemetenteRoteiro = "rastreador@precos.local";
        private const string DestinatarioPadrao = "voce@exemplo.com";

        private readonly ILogger<FakeEmailEnviador> _logger;
        private readonly string _destinatario;

        public FakeEmailEnviador(ILogger<FakeEmailEnviador> logger, IConfiguration configuration)
        {
            _logger = logger;
            _destinatario = configuration.GetSection("Notificacao")["Destinatario"] ?? DestinatarioPadrao;
        }

        public Task EnviarAlerta(string mensagem, CancellationToken cancellationToken = default)
        {
            var email = new StringBuilder()
                .AppendLine()
                .AppendLine("┌───────────────────────────────────────────────")
                .AppendLine($"│ De:      {RemetenteRoteiro}")
                .AppendLine($"│ Para:    {_destinatario}")
                .AppendLine($"│ Assunto: Alerta de preço!")
                .AppendLine($"│ Data:    {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                .AppendLine("├───────────────────────────────────────────────")
                .AppendLine($"│ {mensagem}")
                .AppendLine("└───────────────────────────────────────────────");

            _logger.LogInformation("E-mail (fake) enviado:{Email}", email.ToString());

            return Task.CompletedTask;
        }
    }
}
