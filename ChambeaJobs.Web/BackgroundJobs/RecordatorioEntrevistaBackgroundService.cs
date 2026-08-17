using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Web.BackgroundJobs;

/// <summary>
/// Job diario que revisa las postulaciones con entrevista programada para
/// el día siguiente y envía la notificación 🔔 "Entrevista mañana" tanto al
/// candidato como al reclutador. Se ejecuta una vez al iniciar la app y
/// luego cada 24 horas (suficiente para un sistema de este tamaño; si se
/// necesita mayor precisión horaria, se puede migrar a Hangfire/Quartz).
/// </summary>
public class RecordatorioEntrevistaBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecordatorioEntrevistaBackgroundService> _logger;
    private static readonly TimeSpan IntervaloEjecucion = TimeSpan.FromHours(24);

    public RecordatorioEntrevistaBackgroundService(IServiceProvider serviceProvider, ILogger<RecordatorioEntrevistaBackgroundService> logger)
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
                await EnviarRecordatoriosAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // No debe tumbar la app si falla un ciclo del job; se reintenta en la próxima ejecución.
                _logger.LogError(ex, "Error enviando recordatorios de entrevista.");
            }

            await Task.Delay(IntervaloEjecucion, stoppingToken);
        }
    }

    private async Task EnviarRecordatoriosAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var notificacionService = scope.ServiceProvider.GetRequiredService<INotificacionService>();

        var manana = DateTime.UtcNow.Date.AddDays(1);

        var todasLasPostulaciones = await unitOfWork.Postulaciones.ObtenerTodosAsync();
        var pendientes = todasLasPostulaciones
            .Where(p => p.EntrevistaProgramada
                && !p.RecordatorioEntrevistaEnviado
                && p.FechaEntrevista.HasValue
                && p.FechaEntrevista.Value.Date == manana)
            .ToList();

        if (pendientes.Count == 0) return;

        foreach (var postulacion in pendientes)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var detalle = await unitOfWork.Postulaciones.ObtenerConDetalleAsync(postulacion.Id);
            if (detalle is null) continue;

            var vacanteTitulo = detalle.Vacante?.Titulo ?? "tu vacante";
            var nombreCandidato = detalle.Candidato is not null
                ? $"{detalle.Candidato.Nombres} {detalle.Candidato.Apellidos}".Trim()
                : "El candidato";

            if (!string.IsNullOrWhiteSpace(detalle.Candidato?.UsuarioId))
            {
                await notificacionService.CrearAsync(
                    detalle.Candidato!.UsuarioId,
                    Notificacion.Tipos.EntrevistaProxima,
                    $"🔔 Entrevista mañana: tienes una entrevista para \"{vacanteTitulo}\" a las {detalle.FechaEntrevista:HH:mm}.",
                    "/Candidato/MisPostulaciones");
            }

            if (!string.IsNullOrWhiteSpace(detalle.Vacante?.Empresa?.UsuarioId))
            {
                await notificacionService.CrearAsync(
                    detalle.Vacante!.Empresa!.UsuarioId,
                    Notificacion.Tipos.EntrevistaProxima,
                    $"🔔 Entrevista mañana con {nombreCandidato} para \"{vacanteTitulo}\" a las {detalle.FechaEntrevista:HH:mm}.",
                    $"/Empresa/CandidatosPostulados?vacanteId={detalle.VacanteId}");
            }

            detalle.RecordatorioEntrevistaEnviado = true;
            unitOfWork.Postulaciones.Actualizar(detalle);
        }

        await unitOfWork.GuardarCambiosAsync();
        _logger.LogInformation("Recordatorios de entrevista enviados: {Cantidad}", pendientes.Count);
    }
}
