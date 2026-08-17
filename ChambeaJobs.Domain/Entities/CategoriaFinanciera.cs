using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Domain.Entities;

/// <summary>Catálogo administrable de categorías (ingresos, gastos, activos,
/// pasivos, patrimonio) — nunca se codifican fijas en el código.</summary>
public class CategoriaFinanciera : BaseEntity
{
    public TipoCategoriaFinanciera Tipo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activa { get; set; } = true;
}
