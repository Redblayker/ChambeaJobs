namespace ChambeaJobs.Application.DTOs;

public class ChatMensajeDto
{
    public int Id { get; set; }
    public string Emisor { get; set; } = string.Empty; // "Usuario" | "Bot"
    public string Texto { get; set; } = string.Empty;
    public string? UrlAccion { get; set; }
    public DateTime FechaEnvio { get; set; }
}

public class ChatConversacionDto
{
    public int Id { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int? TicketSoporteId { get; set; }
    public List<ChatMensajeDto> Mensajes { get; set; } = new();
}

public class ChatEnviarMensajeDto
{
    public int? ConversacionId { get; set; }
    public string Texto { get; set; } = string.Empty;
}

public class ChatRespuestaBotDto
{
    public string Texto { get; set; } = string.Empty;
    public string? UrlAccion { get; set; }
    public bool SugiereEscalar { get; set; }
}
