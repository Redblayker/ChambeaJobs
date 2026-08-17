namespace ChambeaJobs.Domain.Entities;

/// <summary>Tabla intermedia N:M entre Candidato y Vacante.</summary>
public class Favorito
{
    public int CandidatoId { get; set; }
    public Candidato? Candidato { get; set; }
    public int VacanteId { get; set; }
    public Vacante? Vacante { get; set; }
    public DateTime FechaAgregado { get; set; } = DateTime.UtcNow;
}
