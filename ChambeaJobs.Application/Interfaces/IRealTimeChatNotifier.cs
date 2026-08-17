using ChambeaJobs.Application.DTOs;

namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Abstracción sobre el mecanismo de tiempo real (SignalR) para el chat de
/// soporte, para que SoporteService no dependa directamente de SignalR ni
/// de la capa Web — mismo criterio que IRealTimeNotifier.
/// </summary>
public interface IRealTimeChatNotifier
{
    Task NotificarNuevoMensajeAsync(int ticketId, MensajeSoporteDto mensaje);
}
