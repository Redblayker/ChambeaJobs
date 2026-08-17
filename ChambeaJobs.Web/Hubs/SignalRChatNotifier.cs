using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ChambeaJobs.Web.Hubs;

/// <inheritdoc cref="IRealTimeChatNotifier"/>
public class SignalRChatNotifier : IRealTimeChatNotifier
{
    private readonly IHubContext<SoporteChatHub> _hubContext;

    public SignalRChatNotifier(IHubContext<SoporteChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotificarNuevoMensajeAsync(int ticketId, MensajeSoporteDto mensaje)
    {
        await _hubContext.Clients.Group($"ticket-{ticketId}").SendAsync("NuevoMensaje", mensaje);
    }
}
