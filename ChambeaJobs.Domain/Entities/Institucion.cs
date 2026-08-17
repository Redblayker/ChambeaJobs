namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Universidad o instituto técnico de Nicaragua (ej. "UNAN-Managua", "INATEC").
/// Se relaciona con Carrera en M:N — una institución ofrece varias carreras,
/// y una misma carrera puede ofrecerla más de una institución.
///
/// Catálogo poblado con datos verificados directamente en los sitios
/// oficiales de cada institución (ago. 2026) — ver la investigación previa
/// a este cambio. Si una carrera no aparece vinculada a una institución es
/// porque no se encontró evidencia oficial de que la ofrezca, no porque se
/// haya olvidado.
/// </summary>
public class Institucion : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;

    /// <summary>"Universidad" o "Técnico" (INATEC) — para poder mostrarlo agrupado o con un rótulo distinto en el catálogo.</summary>
    public string Tipo { get; set; } = "Universidad";

    public List<Facultad> Facultades { get; set; } = new();
}
