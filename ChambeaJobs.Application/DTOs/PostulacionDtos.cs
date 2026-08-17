namespace ChambeaJobs.Application.DTOs;

/// <summary>Fila de "Mis Postulaciones".</summary>
public class PostulacionCandidatoDto
{
    public int Id { get; set; }
    public int VacanteId { get; set; }
    public string VacanteTitulo { get; set; } = string.Empty;
    public string EmpresaNombre { get; set; } = string.Empty;
    public string? EmpresaLogoUrl { get; set; }
    public DateTime FechaPostulacion { get; set; }
    public string Estado { get; set; } = string.Empty;

    // ---------- Seguimiento del candidato (mejora solicitada) ----------
    /// <summary>Etapa actual del flujo: AplicacionEnviada, CvRevisado, VideoVisto, PruebaAprobada, EntrevistaPendiente, Contratacion, Rechazado.</summary>
    public string Etapa { get; set; } = string.Empty;
    public bool CvRevisado { get; set; }
    public bool VideoCvVisto { get; set; }
    public bool? PruebaPsicometricaAprobada { get; set; }
    public int? PruebaPsicometricaPuntaje { get; set; }
    public bool EntrevistaProgramada { get; set; }
    public DateTime? FechaEntrevista { get; set; }
}

/// <summary>Fila de "Candidatos Postulados" para una vacante.</summary>
public class CandidatoPostuladoDto
{
    public int PostulacionId { get; set; }
    public int CandidatoId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public DateTime FechaPostulacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int EstadoId { get; set; }
    public string? CvUrl { get; set; }
    public string? Telefono { get; set; }
    public string? Disponibilidad { get; set; }

    /// <summary>Puntaje de compatibilidad candidato-vacante (0-100). Ver ICompatibilidadService.</summary>
    public decimal PuntajeCompatibilidad { get; set; }
    public bool EntrevistaProgramada { get; set; }

    /// <summary>Compatibilidad expresada en estrellas (0-5) para el ranking automático visual.</summary>
    public int Estrellas => (int)Math.Round(PuntajeCompatibilidad / 20m, MidpointRounding.AwayFromZero);
}

/// <summary>Fila de la pantalla "Comparación de candidatos" (mejora solicitada por RRHH).</summary>
public class CandidatoComparacionDto
{
    public int PostulacionId { get; set; }
    public int CandidatoId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public decimal AniosExperiencia { get; set; }
    public int? PruebaPsicometricaPuntaje { get; set; }
    public decimal? VideoCvPuntaje { get; set; }
    public string? IdiomaPrincipal { get; set; }
    public string? NivelIdiomaPrincipal { get; set; }
    public decimal PuntajeCompatibilidad { get; set; }
    public int Estrellas => (int)Math.Round(PuntajeCompatibilidad / 20m, MidpointRounding.AwayFromZero);
}

/// <summary>Opción para el &lt;select&gt; de cambio de estado de una postulación.</summary>
public class EstadoPostulacionOptionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Fila de la lista de Favoritos.</summary>
public class FavoritoDto
{
    public int VacanteId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string EmpresaNombre { get; set; } = string.Empty;
    public string UbicacionNombre { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public string EstadoVacante { get; set; } = string.Empty;
}

/// <summary>Datos del candidato + su video CV, para la pantalla de comentarios del reclutador.</summary>
public class DetalleVideoCvDto
{
    public int PostulacionId { get; set; }
    public int CandidatoId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public string? VideoCvUrl { get; set; }
    public string VacanteTitulo { get; set; } = string.Empty;
}

/// <summary>Comentario del reclutador sobre el video CV de un candidato.</summary>
public class ComentarioVideoCvDto
{
    public int Id { get; set; }
    public string ReclutadorNombre { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public int? Calificacion { get; set; }
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// KPIs del Dashboard de Empresa (mejora solicitada: vacantes activas,
/// candidatos nuevos, entrevistas hoy, pruebas pendientes, contrataciones
/// del mes). Calculado por PostulacionService.ObtenerResumenDashboardAsync.
/// </summary>
public class EmpresaDashboardResumenDto
{
    public int VacantesActivas { get; set; }
    public int CandidatosNuevos { get; set; }
    public int EntrevistasHoy { get; set; }
    public int PruebasPendientes { get; set; }
    public int ContratacionesDelMes { get; set; }
}
