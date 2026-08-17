using Microsoft.AspNetCore.SignalR;

namespace ChambeaJobs.Web.Hubs;

/// <summary>
/// Hub del widget flotante del chatbot. Igual que SoporteChatHub, agrupa
/// por "chatbot-{conversacionId}" en vez de por usuario, para que funcione
/// también con visitantes que todavía no iniciaron sesión.
/// </summary>
public class ChatbotHub : Hub
{
    public async Task UnirseAConversacion(int conversacionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"chatbot-{conversacionId}");
    }
}
