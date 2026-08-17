namespace ChambeaJobs.Domain.Entities;

/// <summary>Catálogo de categorías de empleo.</summary>
public class Categoria : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
