using ChambeaJobs.Application.DTOs;

namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Lógica de negocio del módulo de Evaluaciones Psicométricas (Big Five):
/// envío por parte de la empresa, respuesta por parte del candidato,
/// cálculo de puntajes por rasgo y compatibilidad.
/// </summary>
public interface IEvaluacionService
{
    Task EnviarEvaluacionAsync(EnviarEvaluacionDto datos);

    /// <summary>Resumen de evaluación para mostrar en la lista de postulantes de la empresa (uno por postulación).</summary>
    Task<Dictionary<int, EvaluacionResumenDto>> ObtenerResumenPorPostulacionesAsync(IEnumerable<int> postulacionIds);

    Task<InvitacionEvaluacionDto?> ObtenerInvitacionAsync(int evaluacionId, int candidatoId);
    Task<RealizarEvaluacionDto?> ObtenerFormularioAsync(int evaluacionId, int candidatoId);
    Task GuardarRespuestasParcialesAsync(int evaluacionId, int candidatoId, Dictionary<int, int> respuestas);

    /// <summary>Guarda todas las respuestas, calcula los puntajes por rasgo y la compatibilidad, y marca la evaluación como Completada.</summary>
    Task FinalizarEvaluacionAsync(int evaluacionId, int candidatoId, Dictionary<int, int> respuestas);

    Task<ResultadoEvaluacionDto?> ObtenerResultadoAsync(int evaluacionId);

    /// <summary>Evaluaciones pendientes del candidato (para mostrar en Mis Postulaciones).</summary>
    Task<Dictionary<int, EvaluacionResumenDto>> ObtenerPendientesDeCandidatoAsync(int candidatoId, IEnumerable<int> postulacionIds);
}
