namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Ticket de soporte creado por un Candidato o Empresa (o visitante) y
/// atendido por un Administrador. Alimenta la sección "Soporte" del
/// panel de administración.
/// </summary>
public class TicketSoporte : BaseEntity
{
    /// <summary>UsuarioId (Identity) de quien crea el ticket. Null si lo envía un visitante no autenticado.</summary>
    public string? UsuarioId { get; set; }
    public string NombreContacto { get; set; } = string.Empty;
    public string CorreoContacto { get; set; } = string.Empty;

    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;

    public string Estado { get; set; } = Estados.Abierto;
    public string? RespuestaAdmin { get; set; }
    public string? AdminUsuarioId { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaRespuesta { get; set; }

    public static class Estados
    {
        public const string Abierto = "Abierto";
        public const string Resuelto = "Resuelto";
    }
}
