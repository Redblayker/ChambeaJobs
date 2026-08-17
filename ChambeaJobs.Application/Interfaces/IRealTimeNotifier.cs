namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Abstracción sobre el mecanismo de tiempo real (SignalR) para que
/// NotificacionService pueda avisarle a un usuario específico sin que la
/// capa Application dependa directamente de SignalR ni de la capa Web.
/// </summary>
public interface IRealTimeNotifier
{
    Task NotificarNuevaNotificacionAsync(string usuarioId, int contadorNoLeidas);
}
