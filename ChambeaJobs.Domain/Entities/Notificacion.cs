namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Notificación interna dirigida a un usuario (Candidato, Empresa o Admin).
/// Alimenta la campanita 🔔 del layout. Se genera desde los servicios de
/// aplicación cuando ocurre un evento relevante (nueva postulación, prueba
/// respondida, entrevista próxima, video CV disponible, etc.).
/// </summary>
public class Notificacion : BaseEntity
{
    /// <summary>UsuarioId (Identity) del destinatario.</summary>
    public string UsuarioId { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Ruta relativa a la que navega al hacer clic (ej. detalle de postulación).</summary>
    public string? UrlDestino { get; set; }

    public bool Leida { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public static class Tipos
    {
        public const string NuevaPostulacion = "NuevaPostulacion";
        public const string PruebaRespondida = "PruebaRespondida";
        public const string EntrevistaProxima = "EntrevistaProxima";
        public const string VideoCvDisponible = "VideoCvDisponible";
        public const string CambioEstadoPostulacion = "CambioEstadoPostulacion";
        public const string ComentarioVideoCv = "ComentarioVideoCv";
        public const string PagoAprobado = "PagoAprobado";
        public const string PagoPendiente = "PagoPendiente";
        public const string EvaluacionPsicometricaRecibida = "EvaluacionPsicometricaRecibida";
        public const string TicketSoporteNuevo = "TicketSoporteNuevo";
        public const string TicketSoporteResuelto = "TicketSoporteResuelto";
        public const string RenovacionPendiente = "RenovacionPendiente";
        public const string PagoRechazado = "PagoRechazado";
    }
}
