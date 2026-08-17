using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Domain.Entities;

public class ChatMensaje
{
    public int Id { get; set; }

    public int ConversacionId { get; set; }
    public ChatConversacion Conversacion { get; set; } = null!;

    public EmisorMensajeChat Emisor { get; set; }
    public string Texto { get; set; } = string.Empty;
    public string? UrlAccion { get; set; }

    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
}
