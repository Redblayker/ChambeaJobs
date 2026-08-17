using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Domain.Entities;

/// <summary>Corrección autorizada sobre un ingreso o gasto que pertenece a
/// un período ya cerrado (o que necesita corregirse sin anularlo del
/// todo) — deja registro de quién, cuándo y por qué.</summary>
public class AjusteFinanciero : BaseEntity
{
    public TipoEntidadAjuste TipoEntidad { get; set; }
    public int EntidadId { get; set; } // Id del IngresoFinanciero o GastoFinanciero afectado

    public string Motivo { get; set; } = string.Empty;
    public decimal MontoAnterior { get; set; }
    public decimal MontoNuevo { get; set; }

    public string UsuarioId { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
