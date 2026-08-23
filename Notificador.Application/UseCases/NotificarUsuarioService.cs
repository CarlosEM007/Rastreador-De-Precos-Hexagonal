using Application.Ports.Driven;
using Application.Ports.Driving;

namespace Application.UseCases
{
    public class NotificarUsuarioService : INotificarUsuario
    {
        private readonly IEnviadorDeAlerta _enviador;

        public NotificarUsuarioService(IEnviadorDeAlerta enviador) => _enviador = enviador;

        public async Task Notificar(string mensagem, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
            {
                throw new ArgumentException("Não há mensagem para notificar.", nameof(mensagem));
            }

            await _enviador.EnviarAlerta(mensagem, cancellationToken);
        }
    }
}
