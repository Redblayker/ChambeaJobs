using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Web.BackgroundJobs;

/// <summary>
/// Automatizaciones del CRM ligadas al ciclo de vida del plan de una empresa:
/// aviso 15 días antes de vencer, 7 días antes, el mismo día del vencimiento,
/// y mover la ficha CRM a "Cliente inactivo" cuando el plan ya venció y no
/// se renovó. Se ejecuta una vez al día.
/// </summary>
public class CrmAutomatizacionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CrmAutomatizacionBackgroundService> _logger;
    private static readonly TimeSpan IntervaloEjecucion = TimeSpan.FromHours(24);

    public CrmAutomatizacionBackgroundService(IServiceProvider serviceProvider, ILogger<CrmAutomatizacionBackgroundService> logger)
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
                await EjecutarAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando automatizaciones del CRM.");
            }

            await Task.Delay(IntervaloEjecucion, stoppingToken);
        }
    }

    private async Task EjecutarAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notificacionService = scope.ServiceProvider.GetRequiredService<INotificacionService>();
        var adminUsuarioService = scope.ServiceProvider.GetRequiredService<IAdminUsuarioService>();

        var idsAdmins = await adminUsuarioService.ObtenerIdsAdministradoresAsync();
        var hoy = DateTime.UtcNow.Date;

        var paquetesVigentes = await db.PaquetesEmpresa
            .Include(p => p.Empresa)
            .Include(p => p.PlanSuscripcion)
            .Where(p => p.Estado == EstadosPaquete.Vigente)
            .ToListAsync(stoppingToken);

        foreach (var paquete in paquetesVigentes)
        {
            stoppingToken.ThrowIfCancellationRequested();
            if (paquete.Empresa is null) continue;

            var diasRestantes = (paquete.FechaVencimiento.Date - hoy).Days;

            if (diasRestantes == 15 || diasRestantes == 7 || diasRestantes == 3 || diasRestantes == 1 || diasRestantes == 0 || diasRestantes == -1)
            {
                var tipoAviso = $"CrmVencimiento{diasRestantes}d";
                var yaSeAviso = await db.Notificaciones.AnyAsync(n =>
                    n.UsuarioId == paquete.Empresa.UsuarioId &&
                    n.Tipo == tipoAviso &&
                    n.FechaCreacion.Date == hoy, stoppingToken);

                if (!yaSeAviso)
                {
                    var mensaje = diasRestantes switch
                    {
                        0 => $"⏰ Tu plan \"{paquete.PlanSuscripcion?.Nombre}\" vence HOY.",
                        < 0 => $"⚠️ Tu plan \"{paquete.PlanSuscripcion?.Nombre}\" ya venció. Renuévalo para seguir publicando vacantes.",
                        _ => $"⏰ Tu plan \"{paquete.PlanSuscripcion?.Nombre}\" vence en {diasRestantes} días."
                    };

                    await notificacionService.CrearAsync(paquete.Empresa.UsuarioId, tipoAviso, mensaje, "/Empresa/ComprarPaquete");
                    await RegistrarActividadCrmAsync(db, paquete.EmpresaId, mensaje, stoppingToken);

                    foreach (var adminId in idsAdmins)
                    {
                        await notificacionService.CrearAsync(adminId, tipoAviso,
                            $"⏰ {paquete.Empresa.NombreEmpresa}: plan vence en {diasRestantes} día(s).", "/Admin/Crm/Pipeline");
                    }
                }
            }
        }

        // Empresas cuyo plan ya venció y no tienen ningún paquete vigente
        // vuelven a "Cliente inactivo" — vía evento, no tocando el CRM
        // directo, para mantener el mismo desacople que el resto del sistema.
        var empresaIdsConPlanVigente = paquetesVigentes.Select(p => p.EmpresaId).ToHashSet();

        var crmActivos = await db.CrmEmpresas
            .Where(c => c.EmpresaId != null
                && c.Etapa == EtapaPipelineCrm.ClienteActivo
                && !empresaIdsConPlanVigente.Contains(c.EmpresaId!.Value))
            .ToListAsync(stoppingToken);

        if (crmActivos.Count > 0)
        {
            var publicador = scope.ServiceProvider.GetRequiredService<ChambeaJobs.Application.Eventos.IPublicadorEventos>();

            foreach (var crm in crmActivos)
            {
                var ultimoPaqueteId = await db.PaquetesEmpresa
                    .Where(p => p.EmpresaId == crm.EmpresaId!.Value)
                    .OrderByDescending(p => p.FechaVencimiento)
                    .Select(p => p.Id)
                    .FirstOrDefaultAsync(stoppingToken);

                await publicador.PublicarAsync(new ChambeaJobs.Application.Eventos.PlanVencidoEvento(crm.EmpresaId!.Value, ultimoPaqueteId));
            }

            _logger.LogInformation("CRM: {Cantidad} empresa(s) movidas a Cliente inactivo por vencimiento.", crmActivos.Count);
        }
    }

    private static async Task RegistrarActividadCrmAsync(ApplicationDbContext db, int empresaId, string mensaje, CancellationToken ct)
    {
        var crm = await db.CrmEmpresas.FirstOrDefaultAsync(c => c.EmpresaId == empresaId, ct);
        if (crm is null) return;

        db.CrmActividades.Add(new CrmActividad
        {
            CrmEmpresaId = crm.Id,
            Tipo = TipoActividadCrm.Recordatorio,
            Descripcion = mensaje,
            FechaActividad = DateTime.UtcNow,
            UsuarioId = "sistema",
            UsuarioNombre = "Automatización CRM",
            FechaCreacion = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }
}
