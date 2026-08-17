using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Ingreso financiero — se genera AUTOMÁTICAMENTE cuando un Pago es
/// aprobado (ver FinanzasManejadorPagoAprobado). PagoId es único: un pago
/// nunca puede generar dos ingresos (constraint UNIQUE en la base de datos,
/// más el chequeo del servicio, como doble seguro contra duplicados por
/// condiciones de carrera).
/// Nunca se borra: se anula (Estado = Anulado) dejando el motivo y quién lo
/// hizo, para mantener el historial completo.
/// </summary>
public class IngresoFinanciero : BaseEntity
{
    /// <summary>Único — un pago solo puede generar un ingreso.</summary>
    public int PagoId { get; set; }
    public Pago? Pago { get; set; }

    public int EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public int? PlanSuscripcionId { get; set; }
    public PlanSuscripcion? PlanSuscripcion { get; set; }

    public int CategoriaFinancieraId { get; set; }
    public CategoriaFinanciera? CategoriaFinanciera { get; set; }

    public int PeriodoFinancieroId { get; set; }
    public PeriodoFinanciero? PeriodoFinanciero { get; set; }

    public decimal Monto { get; set; }
    public MonedaFinanciera Moneda { get; set; } = MonedaFinanciera.USD;

    public string MetodoPago { get; set; } = string.Empty;
    public string? Referencia { get; set; }

    public EstadoMovimientoFinanciero Estado { get; set; } = EstadoMovimientoFinanciero.Activo;

    /// <summary>"Sistema" cuando lo generó el evento automático; el UsuarioId de quien lo generó si fue un proceso manual excepcional.</summary>
    public string GeneradoPor { get; set; } = "Sistema";

    public DateTime FechaIngreso { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public string? MotivoAnulacion { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public string? AnuladoPorUsuarioId { get; set; }
}
