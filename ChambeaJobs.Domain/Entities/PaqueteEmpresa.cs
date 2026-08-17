namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Paquete de publicación comprado (o asignado como prueba gratis) por una
/// empresa. Desde julio 2026 existen dos planes (ver <see cref="PlanSuscripcion"/>):
/// Básico ($20/10 vacantes) y Empresarial ($50/sin límite). El primer mes de
/// cualquier empresa nueva es gratis (EsPruebaGratis=true); después, si
/// RenovacionAutomatica está activo, el sistema genera automáticamente el
/// siguiente ciclo de cobro según el plan elegido (ver
/// RenovacionSuscripcionBackgroundService). El pago de cada ciclo sigue
/// siendo revisado y aprobado manualmente por el Administrador — el sistema
/// no cobra tarjetas de forma automática, solo genera el cobro pendiente.
/// </summary>
public class PaqueteEmpresa : BaseEntity
{
    public int EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public int PlanSuscripcionId { get; set; }
    public PlanSuscripcion? PlanSuscripcion { get; set; }

    public DateTime FechaCompra { get; set; } = DateTime.UtcNow;
    public DateTime FechaVencimiento { get; set; }

    /// <summary>Copiado del plan al momento de la compra (null = ilimitado). Se copia aquí para que el historial no cambie si el plan se edita después.</summary>
    public int? VacantesIncluidas { get; set; } = 10;
    public int VacantesConsumidas { get; set; }
    public string Estado { get; set; } = EstadosPaquete.Pendiente;

    /// <summary>true en el primer ciclo de cada empresa (mes de prueba, sin costo).</summary>
    public bool EsPruebaGratis { get; set; }

    /// <summary>Si está activo, al vencer este paquete el sistema genera automáticamente el siguiente ciclo de cobro (no cobra tarjetas, solo crea el pago pendiente).</summary>
    public bool RenovacionAutomatica { get; set; } = true;

    public Pago? Pago { get; set; }
    public ICollection<Vacante> Vacantes { get; set; } = new List<Vacante>();

    /// <summary>
    /// Regla de negocio central: el paquete solo
    /// habilita publicar si está Vigente, con cupo, y no ha vencido.
    /// VacantesIncluidas en null (plan Empresarial) significa que nunca se
    /// queda sin cupo por cantidad.
    /// </summary>
    public bool TieneCupoDisponible() =>
        Estado == EstadosPaquete.Vigente
        && (VacantesIncluidas is null || VacantesConsumidas < VacantesIncluidas)
        && DateTime.UtcNow <= FechaVencimiento;
}
