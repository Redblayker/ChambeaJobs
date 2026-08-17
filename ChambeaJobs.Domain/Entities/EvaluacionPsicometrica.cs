namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Evaluación psicométrica (Big Five) enviada por una empresa a un
/// candidato postulado a una vacante específica. Módulo nuevo, inspirado
/// en el flujo: Empresa envía → Candidato completa → Empresa ve resultados
/// y compatibilidad.
/// </summary>
public class EvaluacionPsicometrica : BaseEntity
{
    public int PostulacionId { get; set; }
    public Postulacion? Postulacion { get; set; }

    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    public DateTime FechaLimite { get; set; }
    public string? Mensaje { get; set; }
    public string Estado { get; set; } = EstadosEvaluacion.Pendiente;
    public DateTime? FechaCompletado { get; set; }

    // Puntajes por rasgo (0-100), nulos hasta que se completa
    public int? PuntajeResponsabilidad { get; set; }
    public int? PuntajeExtraversion { get; set; }
    public int? PuntajeAmabilidad { get; set; }
    public int? PuntajeApertura { get; set; }
    public int? PuntajeEstabilidadEmocional { get; set; }

    /// <summary>Compatibilidad general con la vacante (0-100), calculada al completar.</summary>
    public int? PuntajeCompatibilidad { get; set; }

    public ICollection<RespuestaPsicometrica> Respuestas { get; set; } = new List<RespuestaPsicometrica>();

    public bool VigenteParaResponder() =>
        Estado == EstadosEvaluacion.Pendiente && DateTime.UtcNow <= FechaLimite;
}

public static class EstadosEvaluacion
{
    public const string Pendiente = "Pendiente";
    public const string Completada = "Completada";
    public const string Vencida = "Vencida";
}
