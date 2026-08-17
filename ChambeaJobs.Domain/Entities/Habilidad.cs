namespace ChambeaJobs.Domain.Entities;

/// <summary>Catálogo de habilidades que un Candidato puede asociar a su perfil.</summary>
public class Habilidad : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public ICollection<CandidatoHabilidad> Candidatos { get; set; } = new List<CandidatoHabilidad>();
}
