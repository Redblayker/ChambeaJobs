using ChambeaJobs.Application.DTOs;

namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Centro de notificaciones 🔔. Genera notificaciones internas para
/// candidatos, empresas y administradores ante eventos relevantes:
/// nueva postulación, prueba respondida, entrevista próxima, video CV
/// disponible, cambio de estado de postulación, comentarios en el video CV
/// y resultado de aprobación de pagos.
/// </summary>
public interface INotificacionService
{
    Task CrearAsync(string usuarioId, string tipo, string mensaje, string? urlDestino = null);

    Task<List<NotificacionDto>> ObtenerRecientesAsync(string usuarioId, int cantidad = 20);

    Task<int> ContarNoLeidasAsync(string usuarioId);

    Task MarcarComoLeidaAsync(int notificacionId, string usuarioId);

    Task MarcarTodasComoLeidasAsync(string usuarioId);

    Task EliminarAsync(int notificacionId, string usuarioId);

    Task EliminarTodasAsync(string usuarioId);
}
