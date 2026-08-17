using ChambeaJobs.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ChambeaJobs.Web.Hubs;

/// <inheritdoc cref="IRealTimeNotifier"/>
public class SignalRNotifier : IRealTimeNotifier
{
    private readonly IHubContext<NotificacionesHub> _hubContext;

    public SignalRNotifier(IHubContext<NotificacionesHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotificarNuevaNotificacionAsync(string usuarioId, int contadorNoLeidas)
    {
        await _hubContext.Clients.User(usuarioId).SendAsync("NuevaNotificacion", contadorNoLeidas);
    }
}
