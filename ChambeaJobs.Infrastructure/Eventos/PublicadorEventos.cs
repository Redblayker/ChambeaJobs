using ChambeaJobs.Application.Eventos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChambeaJobs.Infrastructure.Eventos;

/// <inheritdoc cref="IPublicadorEventos"/>
public class PublicadorEventos : IPublicadorEventos
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PublicadorEventos> _logger;

    public PublicadorEventos(IServiceProvider serviceProvider, ILogger<PublicadorEventos> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublicarAsync<TEvento>(TEvento evento) where TEvento : IEventoDominio
    {
        var manejadores = _serviceProvider.GetServices<IManejadorEvento<TEvento>>();

        foreach (var manejador in manejadores)
        {
            try
            {
                await manejador.ManejarAsync(evento);
            }
            catch (Exception ex)
            {
                // Un manejador que falla (ej. el CRM) nunca debe tumbar la
                // acción principal del usuario (ej. publicar su vacante) —
                // solo se registra el error y se sigue con los demás.
                _logger.LogError(ex, "Error procesando el evento {Evento} en {Manejador}.", typeof(TEvento).Name, manejador.GetType().Name);
            }
        }
    }
}
