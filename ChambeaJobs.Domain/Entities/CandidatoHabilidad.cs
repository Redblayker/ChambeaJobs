namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Tabla intermedia N:M entre Candidato y Habilidad./// Clave primaria compuesta (CandidatoId, HabilidadId) configurada en
/// ApplicationDbContext.OnModelCreating.
/// </summary>
public class CandidatoHabilidad
{
    public int CandidatoId { get; set; }
    public Candidato? Candidato { get; set; }
    public int HabilidadId { get; set; }
    public Habilidad? Habilidad { get; set; }
    public string? NivelDominio { get; set; } // Básico/Intermedio/Avanzado
}
