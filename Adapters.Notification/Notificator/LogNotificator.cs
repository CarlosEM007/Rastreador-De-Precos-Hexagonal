using Application.Ports.Driven;

namespace Adapters.Notification.Notificator
{
    public class LogNotificator : INotificationGateway
    {
        public async Task NotificarPrecoAlvoAtingido(string Message)
        {
            string caminho = $@"C:\RastreadorDePrecos";

            Directory.CreateDirectory(caminho);

            using (StreamWriter writer = new StreamWriter($@"{caminho}\RegistroDePreco({DateTime.Now.ToString("dd.MM.yy")}).txt"))
            {
                await writer.WriteLineAsync(Message);
            }
        }
    }
}
