using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ChambeaJobs.Web.Hubs;

/// <inheritdoc cref="IRealTimeChatBotNotifier"/>
public class SignalRChatBotNotifier : IRealTimeChatBotNotifier
{
    private readonly IHubContext<ChatbotHub> _hubContext;

    public SignalRChatBotNotifier(IHubContext<ChatbotHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotificarNuevoMensajeAsync(int conversacionId, ChatMensajeDto mensaje)
    {
        await _hubContext.Clients.Group($"chatbot-{conversacionId}").SendAsync("NuevoMensaje", mensaje);
    }
}
