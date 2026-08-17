namespace ChambeaJobs.Application.Eventos;

/// <summary>Cualquier módulo que necesite reaccionar a un evento de dominio
/// implementa esta interfaz para ese tipo de evento específico. Se pueden
/// registrar varios manejadores para el mismo evento (ej. el CRM y, más
/// adelante, el chatbot o un módulo de analítica) sin que se conozcan entre
/// sí — así es como el sistema queda desacoplado.</summary>
public interface IManejadorEvento<in TEvento> where TEvento : IEventoDominio
{
    Task ManejarAsync(TEvento evento);
}

/// <summary>Punto único por donde el resto del sistema dispara eventos de
/// dominio, sin saber ni importarle quién los procesa después.</summary>
public interface IPublicadorEventos
{
    Task PublicarAsync<TEvento>(TEvento evento) where TEvento : IEventoDominio;
}
