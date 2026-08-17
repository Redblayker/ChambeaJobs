using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Web.BackgroundJobs;

/// <summary>
/// Job diario que implementa la "renovación automática" de suscripciones.
///
/// IMPORTANTE — qué hace y qué NO hace este servicio:
/// ChambeaJobs no cobra tarjetas de forma automática (no hay integración con
/// una pasarela de pago que permita cargos recurrentes reales). Lo que este
/// job SÍ hace es: cuando el paquete Vigente de una empresa vence y tiene
/// RenovacionAutomatica activo, genera automáticamente el paquete y el pago
/// PENDIENTE del siguiente ciclo (según el plan que la empresa tiene
/// elegido), y notifica a la empresa y al admin para que se complete el
/// pago manualmente — igual que cualquier otra compra de paquete.
///
/// Se ejecuta una vez al iniciar la app y luego cada 24 horas.
/// </summary>
public class RenovacionSuscripcionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RenovacionSuscripcionBackgroundService> _logger;
    private static readonly TimeSpan IntervaloEjecucion = TimeSpan.FromHours(24);

    public RenovacionSuscripcionBackgroundService(IServiceProvider serviceProvider, ILogger<RenovacionSuscripcionBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GenerarRenovacionesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando renovaciones automáticas de suscripción.");
            }

            await Task.Delay(IntervaloEjecucion, stoppingToken);
        }
    }

    private async Task GenerarRenovacionesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var notificacionService = scope.ServiceProvider.GetRequiredService<INotificacionService>();
        var adminUsuarioService = scope.ServiceProvider.GetRequiredService<IAdminUsuarioService>();

        var todosLosPaquetes = await unitOfWork.PaquetesEmpresa.ObtenerTodosConDetalleAsync();

        var vencidosParaRenovar = todosLosPaquetes
            .Where(p => p.Estado == EstadosPaquete.Vigente
                && p.RenovacionAutomatica
                && DateTime.UtcNow > p.FechaVencimiento)
            .ToList();

        if (vencidosParaRenovar.Count == 0) return;

        var idsAdmins = await adminUsuarioService.ObtenerIdsAdministradoresAsync();

        foreach (var paqueteVencido in vencidosParaRenovar)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var plan = await unitOfWork.PlanesSuscripcion.ObtenerPorIdAsync(paqueteVencido.PlanSuscripcionId);
            if (plan is null) continue;

            // El paquete vencido deja de contar como "vigente" y de poder
            // volver a renovarse por su cuenta (su sucesor toma esa posta).
            paqueteVencido.Estado = EstadosPaquete.Vencido;
            paqueteVencido.RenovacionAutomatica = false;
            unitOfWork.PaquetesEmpresa.Actualizar(paqueteVencido);

            var nuevoPaquete = new PaqueteEmpresa
            {
                EmpresaId = paqueteVencido.EmpresaId,
                PlanSuscripcionId = plan.Id,
                FechaCompra = DateTime.UtcNow,
                FechaVencimiento = DateTime.UtcNow.AddDays(plan.DiasVigencia),
                VacantesIncluidas = plan.VacantesIncluidas,
                VacantesConsumidas = 0,
                Estado = EstadosPaquete.Pendiente,
                EsPruebaGratis = false,
                RenovacionAutomatica = true
            };

            await unitOfWork.PaquetesEmpresa.AgregarAsync(nuevoPaquete);
            await unitOfWork.GuardarCambiosAsync(); // para obtener el Id generado

            var pagoRenovacion = new Pago
            {
                PaqueteEmpresaId = nuevoPaquete.Id,
                Monto = plan.Precio,
                MetodoPago = "Renovación automática — pendiente de pago",
                ReferenciaTransaccion = null,
                EstadoPago = EstadosPago.Pendiente,
                FechaPago = DateTime.UtcNow
            };

            await unitOfWork.Pagos.AgregarAsync(pagoRenovacion);
            await unitOfWork.GuardarCambiosAsync();

            // 🔔 Notificar a la empresa: su ciclo terminó, ya se generó su renovación (pendiente de pago).
            if (!string.IsNullOrWhiteSpace(paqueteVencido.Empresa?.UsuarioId))
            {
                await notificacionService.CrearAsync(
                    paqueteVencido.Empresa!.UsuarioId,
                    "RenovacionPendiente",
                    $"🔄 Tu plan \"{plan.Nombre}\" se renovó automáticamente por ${plan.Precio:0.00}. Realiza el pago desde \"Comprar paquete\" para mantener tus vacantes activas.",
                    "/Empresa/ComprarPaquete");
            }

            // 🔔 Notificar a los administradores: hay un nuevo pago de renovación pendiente.
            foreach (var adminId in idsAdmins)
            {
                await notificacionService.CrearAsync(
                    adminId,
                    Notificacion.Tipos.PagoPendiente,
                    $"🔄 Renovación automática generada para {paqueteVencido.Empresa?.NombreEmpresa ?? "una empresa"} (plan {plan.Nombre}, ${plan.Precio:0.00}).",
                    "/Admin/PagosPendientes");
            }
        }

        _logger.LogInformation("Renovaciones automáticas generadas: {Cantidad}", vencidosParaRenovar.Count);
    }
}
