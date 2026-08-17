namespace ChambeaJobs.Domain.Entities;

/// <summary>Experiencia laboral registrada en el perfil de un Candidato.</summary>
public class ExperienciaLaboral : BaseEntity
{
    public int CandidatoId { get; set; }
    public Candidato? Candidato { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; } // null = trabajo actual
    public string? Descripcion { get; set; }
}
