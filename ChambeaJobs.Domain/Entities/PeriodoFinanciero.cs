using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Domain.Entities;

/// <summary>Período contable interno (mes o año). Cuando está cerrado, no se
/// pueden crear/editar/anular movimientos dentro de él directamente — solo
/// mediante un AjusteFinanciero autorizado.</summary>
public class PeriodoFinanciero : BaseEntity
{
    public TipoPeriodoFinanciero Tipo { get; set; }
    public int Anio { get; set; }
    public int? Mes { get; set; } // null cuando Tipo = Anual

    /// <summary>Ej. "Agosto 2026" o "2026".</summary>
    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public bool Cerrado { get; set; }
    public DateTime? FechaCierre { get; set; }
    public string? CerradoPorUsuarioId { get; set; }
}
