using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Domain.Entities;

public class ChatConversacion
{
    public int Id { get; set; }

    public string? UsuarioId { get; set; }
    public string RolUsuario { get; set; } = string.Empty; // "Candidato" | "Empresa" | "Visitante"

    public EstadoConversacionChat Estado { get; set; } = EstadoConversacionChat.EnCurso;
    public int? TicketSoporteId { get; set; }

    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }

    public ICollection<ChatMensaje> Mensajes { get; set; } = new List<ChatMensaje>();
}
