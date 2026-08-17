using ChambeaJobs.Application.Interfaces;

namespace ChambeaJobs.Web.BackgroundJobs;

/// <summary>Se asegura de que exista el período financiero del mes/año
/// actual (sin duplicar — ObtenerOCrearPeriodoMensualActualAsync ya es
/// idempotente). Corre una vez al día; no hace nada más porque el cierre de
/// un período es una decisión humana (botón "Cerrar período" en el panel),
/// no algo que deba automatizarse solo.</summary>
public class FinanzasAutomatizacionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FinanzasAutomatizacionBackgroundService> _logger;
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(24);

    public FinanzasAutomatizacionBackgroundService(IServiceProvider serviceProvider, ILogger<FinanzasAutomatizacionBackgroundService> logger)
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
                using var scope = _serviceProvider.CreateScope();
                var periodoService = scope.ServiceProvider.GetRequiredService<IPeriodoFinancieroService>();
                await periodoService.ObtenerOCrearPeriodoMensualActualAsync();
                await periodoService.ObtenerOCrearPeriodoAnualActualAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error asegurando el período financiero actual.");
            }

            await Task.Delay(Intervalo, stoppingToken);
        }
    }
}
