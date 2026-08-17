namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Genera el comprobante de pago en PDF que la empresa puede descargar
/// desde su Historial de Pagos. IMPORTANTE: este comprobante es un recibo
/// interno de ChambeaJobs, no una factura fiscal — para eso la empresa
/// necesita estar inscrita ante la DGI con su propio sistema autorizado.
/// </summary>
public interface IComprobantePagoService
{
    /// <summary>Genera el PDF y devuelve sus bytes. Solo puede generarse para pagos ya Aprobados.</summary>
    Task<byte[]> GenerarComprobanteAsync(int pagoId, int empresaId, string correoEmpresa);

    /// <summary>Igual que GenerarComprobanteAsync, pero para el Admin: no valida a qué empresa pertenece el pago.</summary>
    Task<byte[]> GenerarComprobanteAdminAsync(int pagoId);
}
