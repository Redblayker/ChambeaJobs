using Microsoft.AspNetCore.SignalR;

namespace ChambeaJobs.Web.Hubs;

/// <summary>
/// Hub del chat de Soporte. A diferencia de NotificacionesHub, este SÍ
/// expone un método invocable por el cliente (UnirseAlTicket) porque el
/// visitante necesita "entrar" a la conversación de su ticket específico
/// para recibir los mensajes que le lleguen — se agrupan por
/// "ticket-{id}" en vez de por usuario, porque un ticket puede venir de
/// un visitante anónimo que no tiene UsuarioId de Identity.
///
/// Sin [Authorize] a propósito: el Centro de Soporte lo puede usar
/// cualquier visitante, tenga cuenta o no (ver SoporteController).
/// </summary>
public class SoporteChatHub : Hub
{
    public async Task UnirseAlTicket(int ticketId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
    }
}
