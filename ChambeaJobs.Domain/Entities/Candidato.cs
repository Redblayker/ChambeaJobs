namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Perfil profesional de un usuario con rol Candidato./// </summary>
public class Candidato : BaseEntity
{
    public string UsuarioId { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public string? FotoUrl { get; set; }
    public int? CvArchivoId { get; set; }
    public Archivo? CvArchivo { get; set; }
    public string? Direccion { get; set; }

    /// <summary>Cuándo puede empezar a trabajar: "Inmediata" o "15 días".</summary>
    public string? Disponibilidad { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // ---------- Video Currículum ----------
    /// <summary>URL del video CV (archivo propio o link externo tipo YouTube/Vimeo).</summary>
    public string? VideoCvUrl { get; set; }
    public int? VideoCvArchivoId { get; set; }
    public Archivo? VideoCvArchivo { get; set; }
    public DateTime? VideoCvFechaSubida { get; set; }

    public ICollection<ExperienciaLaboral> Experiencias { get; set; } = new List<ExperienciaLaboral>();
    public ICollection<Educacion> Educaciones { get; set; } = new List<Educacion>();
    public ICollection<CandidatoHabilidad> Habilidades { get; set; } = new List<CandidatoHabilidad>();
    public ICollection<CandidatoIdioma> Idiomas { get; set; } = new List<CandidatoIdioma>();
    public ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();
    public ICollection<Curso> Cursos { get; set; } = new List<Curso>();

    /// <summary>
    /// Calcula el porcentaje de completitud del perfil, usado en el
    /// Dashboard Candidato para el CTA "Completar perfil".
    /// Se pondera: datos básicos (25%), al menos 1 experiencia (25%),
    /// al menos 1 educación (25%), CV subido (25%).
    /// </summary>
    public int CalcularPorcentajeCompletitud()
    {
        var puntos = 0;
        if (!string.IsNullOrWhiteSpace(FotoUrl) && !string.IsNullOrWhiteSpace(Direccion)) puntos += 25;
        if (Experiencias.Any()) puntos += 25;
        if (Educaciones.Any()) puntos += 25;
        if (CvArchivoId.HasValue) puntos += 25;
        return puntos;
    }
}

public static class DisponibilidadCandidato
{
    public const string Inmediata = "Inmediata";
    public const string QuinceDias = "15 días";

    public static readonly string[] Todas = { Inmediata, QuinceDias };
}
