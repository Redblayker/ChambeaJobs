using ChambeaJobs.Application.DTOs;

namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Lógica de negocio del módulo de monetización: registrar solicitud de
/// pago manual, consultar estado del paquete vigente, y las acciones de
/// aprobación/rechazo que ejecuta el Administrador.
/// </summary>
public interface IPaqueteEmpresaService
{
    Task<PaqueteEstadoDto> ObtenerEstadoPaqueteAsync(int empresaId);
    Task<List<PagoHistorialDto>> ObtenerHistorialAsync(int empresaId);

    /// <summary>Crea un PaqueteEmpresa + Pago en estado Pendiente, a la espera de aprobación.</summary>
    Task RegistrarSolicitudPagoAsync(int empresaId, RegistrarPagoDto datos);

    Task<List<PagoPendienteDto>> ObtenerPagosPendientesAsync();
    Task AprobarPagoAsync(int pagoId, string? comentario);
    Task RechazarPagoAsync(int pagoId, string? comentario);

    /// <summary>Historial de TODOS los pagos de TODAS las empresas (vista del Administrador), con filtros opcionales.</summary>
    Task<List<PagoAdminDto>> ObtenerHistorialCompletoAsync(string? nombreEmpresa, string? estadoPago, DateTime? desde, DateTime? hasta);

    /// <summary>Planes activos disponibles para elegir (registro de empresa, o al comprar/renovar paquete).</summary>
    Task<List<PlanSuscripcionOptionDto>> ObtenerPlanesDisponiblesAsync();

    /// <summary>Todos los planes (activos e inactivos), para la pantalla de Configuración del Admin.</summary>
    Task<List<EditarPlanSuscripcionDto>> ObtenerTodosLosPlanesAsync();

    Task ActualizarPlanAsync(EditarPlanSuscripcionDto datos, string adminId);

    /// <summary>Marca como Vencido cualquier paquete Vigente cuya FechaVencimiento ya pasó.</summary>
    Task ActualizarVencimientosAsync();
}
