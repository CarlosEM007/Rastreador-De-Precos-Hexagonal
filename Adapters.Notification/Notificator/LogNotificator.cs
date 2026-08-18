using Application.Ports.Driven;
using System.IO;

namespace Adapters.Notification.Notificator
{
    // Classe legada. Que não será mais usada, mas ficará aqui para ver a
    // evolução do projeto

    public class LogNotificator : INotificationGateway
    {
        public async Task NotificarPrecoAlvoAtingido(string Message)
        {
            string caminhobase = Path.GetTempPath();

            string caminho = Path.Combine(caminhobase, "RastreadorDePrecos");

            Directory.CreateDirectory(caminho);

            using (StreamWriter writer = new StreamWriter(Path.Combine(caminho, $"RegistroDePreco({DateTime.Now:dd.MM.yy}).txt")))
            {
                await writer.WriteLineAsync(Message);
            }
        }
    }
}
