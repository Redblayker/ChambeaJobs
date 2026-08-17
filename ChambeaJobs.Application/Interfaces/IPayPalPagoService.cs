namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Pagos automáticos vía PayPal — convive con el pago manual (transferencia/
/// depósito) que ya existe, sin reemplazarlo. Un pago por PayPal se aprueba
/// y activa solo, sin que un Admin tenga que revisarlo.
/// </summary>
public interface IPayPalPagoService
{
    /// <summary>true si el sistema tiene configuradas las credenciales de PayPal (Client ID + Secret). Si es false, el botón de PayPal no debe mostrarse en ningún lado.</summary>
    bool PayPalDisponible { get; }

    /// <summary>Client ID público de PayPal — seguro de exponer al navegador (no es el Secret). Cadena vacía si no está configurado.</summary>
    string ClientIdPublico { get; }

    /// <summary>Crea la orden en PayPal para el plan indicado y devuelve el ID de orden de PayPal (para el botón del SDK de JavaScript).</summary>
    Task<string> CrearOrdenAsync(int empresaId, int planSuscripcionId);

    /// <summary>
    /// Confirma con PayPal que la orden realmente se pagó (nunca se confía
    /// solo en lo que mande el navegador), y si es así, activa el paquete
    /// de la empresa automáticamente y registra el pago como Aprobado.
    /// </summary>
    Task CapturarOrdenAsync(int empresaId, string ordenPayPalId);
}
