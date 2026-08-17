namespace ChambeaJobs.Domain.Entities;

/// <summary>Postulación de un Candidato a una Vacante.</summary>
public class Postulacion : BaseEntity
{
    public int CandidatoId { get; set; }
    public Candidato? Candidato { get; set; }
    public int VacanteId { get; set; }
    public Vacante? Vacante { get; set; }
    public int EstadoPostulacionId { get; set; }
    public EstadoPostulacion? EstadoPostulacion { get; set; }
    public DateTime FechaPostulacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionEstado { get; set; }

    // ---------- Ranking automático (compatibilidad) ----------
    /// <summary>
    /// Puntaje de compatibilidad candidato-vacante (0-100), calculado por
    /// ICompatibilidadService a partir de habilidades, experiencia e idiomas
    /// vs. los requisitos de la vacante. Se muestra como estrellas (0-5) y
    /// se usa como criterio de ORDER BY por defecto en el listado de RRHH.
    /// </summary>
    public decimal? PuntajeCompatibilidad { get; set; }

    // ---------- Seguimiento del candidato (etapas visibles para el candidato) ----------
    public bool CvRevisado { get; set; } = false;
    public DateTime? FechaCvRevisado { get; set; }

    public bool VideoCvVisto { get; set; } = false;
    public DateTime? FechaVideoCvVisto { get; set; }

    public bool? PruebaPsicometricaAprobada { get; set; } // null = no aplica/pendiente
    public int? PruebaPsicometricaPuntaje { get; set; }
    public DateTime? FechaPruebaPsicometrica { get; set; }

    public bool EntrevistaProgramada { get; set; } = false;
    public DateTime? FechaEntrevista { get; set; }
    public string? NotaEntrevista { get; set; }

    /// <summary>Identificador único de la sala de videollamada (Jitsi Meet) para la entrevista, generado la primera vez que alguna de las partes entra.</summary>
    public string? SalaVideollamadaId { get; set; }

    /// <summary>Evita reenviar el recordatorio "Entrevista mañana" en cada ejecución del job diario.</summary>
    public bool RecordatorioEntrevistaEnviado { get; set; } = false;

    public ICollection<ComentarioVideoCv> ComentariosVideoCv { get; set; } = new List<ComentarioVideoCv>();

    /// <summary>
    /// Deriva la etapa actual del flujo de seguimiento del candidato
    ///. Se usa para pintar los
    /// íconos ✔ / 🟡 / ○ en la vista de Postulaciones del candidato.
    /// </summary>
    public string EtapaActual()
    {
        var estadoActual = EstadoPostulacion;
        if (estadoActual?.Nombre == ChambeaJobs.Domain.Entities.EstadoPostulacion.Nombres.Contratado) return "Contratacion";
        if (estadoActual?.Nombre == ChambeaJobs.Domain.Entities.EstadoPostulacion.Nombres.Rechazado) return "Rechazado";
        if (EntrevistaProgramada) return "EntrevistaPendiente";
        if (PruebaPsicometricaAprobada == true) return "PruebaAprobada";
        if (VideoCvVisto) return "VideoVisto";
        if (CvRevisado) return "CvRevisado";
        return "AplicacionEnviada";
    }
}
