namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Catálogo de idiomas disponibles para el perfil del candidato.
/// Sigue el mismo patrón que <see cref="Habilidad"/> (catálogo + tabla
/// intermedia N:M vía <see cref="CandidatoIdioma"/>).
/// </summary>
public class Idioma : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public ICollection<CandidatoIdioma> Candidatos { get; set; } = new List<CandidatoIdioma>();
}
