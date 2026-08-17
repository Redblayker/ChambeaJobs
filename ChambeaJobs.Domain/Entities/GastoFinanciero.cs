using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Domain.Entities;

/// <summary>Gasto registrado manualmente por un usuario autorizado (hosting,
/// dominio, publicidad, etc.). Nunca se borra: se anula.</summary>
public class GastoFinanciero : BaseEntity
{
    public int CategoriaFinancieraId { get; set; }
    public CategoriaFinanciera? CategoriaFinanciera { get; set; }

    public string Descripcion { get; set; } = string.Empty;
    public string? Proveedor { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public MonedaFinanciera Moneda { get; set; } = MonedaFinanciera.USD;
    public string MetodoPago { get; set; } = string.Empty;
    public string? NumeroReferencia { get; set; }

    /// <summary>Comprobante adjunto (factura/recibo) — reutiliza la entidad Archivo ya existente, con las mismas validaciones de extensión/tamaño.</summary>
    public int? ArchivoComprobanteId { get; set; }
    public Archivo? ArchivoComprobante { get; set; }

    public EstadoMovimientoFinanciero Estado { get; set; } = EstadoMovimientoFinanciero.Activo;

    public int PeriodoFinancieroId { get; set; }
    public PeriodoFinanciero? PeriodoFinanciero { get; set; }

    public string RegistradoPorUsuarioId { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public string? MotivoAnulacion { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public string? AnuladoPorUsuarioId { get; set; }
}
