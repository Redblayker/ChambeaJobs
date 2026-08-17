using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="INotificacionService"/>
public class NotificacionService : INotificacionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealTimeNotifier _realTimeNotifier;

    public NotificacionService(IUnitOfWork unitOfWork, IRealTimeNotifier realTimeNotifier)
    {
        _unitOfWork = unitOfWork;
        _realTimeNotifier = realTimeNotifier;
    }

    public async Task CrearAsync(string usuarioId, string tipo, string mensaje, string? urlDestino = null)
    {
        if (string.IsNullOrWhiteSpace(usuarioId)) return; // nada que notificar sin destinatario

        var notificacion = new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = tipo,
            Mensaje = mensaje,
            UrlDestino = urlDestino,
            Leida = false,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.Notificaciones.AgregarAsync(notificacion);
        await _unitOfWork.GuardarCambiosAsync();

        // Tiempo real: si el usuario tiene la app abierta, la campanita se
        // actualiza al instante, sin que tenga que recargar la página.
        var contadorNoLeidas = await _unitOfWork.Notificaciones.ContarNoLeidasAsync(usuarioId);
        await _realTimeNotifier.NotificarNuevaNotificacionAsync(usuarioId, contadorNoLeidas);
    }

    public async Task<List<NotificacionDto>> ObtenerRecientesAsync(string usuarioId, int cantidad = 20)
    {
        var notificaciones = await _unitOfWork.Notificaciones.ObtenerPorUsuarioAsync(usuarioId, cantidad);

        return notificaciones
            .Select(n => new NotificacionDto
            {
                Id = n.Id,
                Tipo = n.Tipo,
                Mensaje = n.Mensaje,
                UrlDestino = n.UrlDestino,
                Leida = n.Leida,
                FechaCreacion = n.FechaCreacion
            })
            .ToList();
    }

    public async Task<int> ContarNoLeidasAsync(string usuarioId) =>
        await _unitOfWork.Notificaciones.ContarNoLeidasAsync(usuarioId);

    public async Task MarcarComoLeidaAsync(int notificacionId, string usuarioId)
    {
        var notificacion = await _unitOfWork.Notificaciones.ObtenerPorIdAsync(notificacionId);
        if (notificacion is null || notificacion.UsuarioId != usuarioId) return; // silencioso: no es del usuario o ya no existe

        if (!notificacion.Leida)
        {
            notificacion.Leida = true;
            _unitOfWork.Notificaciones.Actualizar(notificacion);
            await _unitOfWork.GuardarCambiosAsync();
        }
    }

    public async Task MarcarTodasComoLeidasAsync(string usuarioId) =>
        await _unitOfWork.Notificaciones.MarcarTodasComoLeidasAsync(usuarioId);

    public async Task EliminarAsync(int notificacionId, string usuarioId)
    {
        var notificacion = await _unitOfWork.Notificaciones.ObtenerPorIdAsync(notificacionId);
        if (notificacion is null || notificacion.UsuarioId != usuarioId) return; // silencioso: no es del usuario o ya no existe

        _unitOfWork.Notificaciones.Eliminar(notificacion);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task EliminarTodasAsync(string usuarioId) =>
        await _unitOfWork.Notificaciones.EliminarTodasAsync(usuarioId);
}
