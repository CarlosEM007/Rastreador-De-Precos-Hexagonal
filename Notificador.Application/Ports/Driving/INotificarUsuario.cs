namespace Application.Ports.Driving
{
    /// <summary>
    /// Porta de entrada do serviço: é por aqui que o mundo externo (hoje, o consumer da fila)
    /// pede que um alerta seja entregue.
    /// </summary>
    public interface INotificarUsuario
    {
        Task Notificar(string mensagem, CancellationToken cancellationToken = default);
    }
}
