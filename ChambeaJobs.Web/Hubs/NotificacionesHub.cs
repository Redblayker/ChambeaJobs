using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChambeaJobs.Web.Hubs;

/// <summary>
/// Hub de SignalR para notificaciones en tiempo real. No expone métodos
/// invocables por el cliente — solo recibe conexiones para que el servidor
/// pueda enviarle eventos a un usuario específico (ver
/// <see cref="ChambeaJobs.Application.Services.NotificacionService"/>,
/// que llama a Clients.User(usuarioId) al crear una notificación).
/// </summary>
[Authorize]
public class NotificacionesHub : Hub
{
}
