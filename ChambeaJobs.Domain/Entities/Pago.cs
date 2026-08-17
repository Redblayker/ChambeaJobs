namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Registro de pago manual asociado a un PaqueteEmpresa (registro manual
/// validado por administrador, sin pasarela real).
/// </summary>
public class Pago : BaseEntity
{
    public int PaqueteEmpresaId { get; set; }
    public PaqueteEmpresa? PaqueteEmpresa { get; set; }
    public decimal Monto { get; set; } = 20.00m;
    public string MetodoPago { get; set; } = "Transferencia/Depósito manual";
    public string? ReferenciaTransaccion { get; set; }
    public string EstadoPago { get; set; } = EstadosPago.Pendiente;
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;

    /// <summary>Comentario del administrador al aprobar/rechazar (opcional, solo aplica a pagos manuales).</summary>
    public string? ComentarioRevision { get; set; }
    public DateTime? FechaRevision { get; set; }

    /// <summary>ID de la orden en PayPal, cuando el pago se hizo por esa vía (null en pagos manuales).</summary>
    public string? IdOrdenPayPal { get; set; }
}

public static class EstadosPago
{
    public const string Pendiente = "Pendiente";
    public const string Aprobado = "Aprobado";
    public const string Rechazado = "Rechazado";
}
