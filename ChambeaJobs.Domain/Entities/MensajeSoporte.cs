namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Un mensaje individual dentro de la conversación de un TicketSoporte.
/// Antes un ticket solo tenía un mensaje inicial + una respuesta del admin
/// (Estados.Abierto/Resuelto); ahora es una conversación real de varios
/// mensajes de ida y vuelta, en tiempo real vía SoporteChatHub.
/// </summary>
public class MensajeSoporte
{
    public int Id { get; set; }
    public int TicketSoporteId { get; set; }
    public TicketSoporte? Ticket { get; set; }

    /// <summary>UsuarioId (Identity) del autor. Null si lo envió un visitante no autenticado.</summary>
    public string? AutorUsuarioId { get; set; }
    public string AutorNombre { get; set; } = string.Empty;

    /// <summary>true si lo envió un Administrador — determina de qué lado se pinta la burbuja en el chat.</summary>
    public bool EsAdmin { get; set; }

    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
}
