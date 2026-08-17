namespace ChambeaJobs.Application.DTOs;

/// <summary>Fila del centro de notificaciones 🔔 (campanita del layout).</summary>
public class NotificacionDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? UrlDestino { get; set; }
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }

    /// <summary>Ícono sugerido según el tipo, para pintar en la campanita.</summary>
    public string Icono => Tipo switch
    {
        "NuevaPostulacion" => "fa-file-circle-plus",
        "PruebaRespondida" => "fa-clipboard-check",
        "EntrevistaProxima" => "fa-calendar-day",
        "VideoCvDisponible" => "fa-video",
        "CambioEstadoPostulacion" => "fa-arrows-rotate",
        "ComentarioVideoCv" => "fa-comment",
        "PagoAprobado" => "fa-circle-check",
        "PagoRechazado" => "fa-circle-xmark",
        "PagoPendiente" => "fa-money-check-dollar",
        "EvaluacionPsicometricaRecibida" => "fa-clipboard-question",
        "TicketSoporteNuevo" => "fa-ticket",
        "TicketSoporteResuelto" => "fa-circle-check",
        "RenovacionPendiente" => "fa-rotate",
        "NuevaPostulacionPasantia" => "fa-graduation-cap",
        "CambioEstadoPostulacionPasantia" => "fa-graduation-cap",
        _ => "fa-bell"
    };
}
