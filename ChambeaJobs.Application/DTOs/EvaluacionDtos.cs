using System.ComponentModel.DataAnnotations;

namespace ChambeaJobs.Application.DTOs;

/// <summary>DTO para el modal "Enviar evaluación" (Empresa → Candidato).</summary>
public class EnviarEvaluacionDto
{
    [Required]
    public int PostulacionId { get; set; }

    [Required(ErrorMessage = "Selecciona la fecha límite")]
    [DataType(DataType.Date)]
    public DateTime FechaLimite { get; set; } = DateTime.UtcNow.AddDays(7);

    [StringLength(1000)]
    public string? Mensaje { get; set; }
}

/// <summary>Fila resumen de evaluación para el listado de candidatos postulados (vista Empresa).</summary>
public class EvaluacionResumenDto
{
    public int? EvaluacionId { get; set; }
    public string Estado { get; set; } = "Sin enviar"; // Sin enviar, Pendiente, Completada, Vencida
    public int? PuntajeCompatibilidad { get; set; }
    public DateTime? FechaLimite { get; set; }
}

/// <summary>Pantalla de invitación antes de comenzar la prueba (Candidato).</summary>
public class InvitacionEvaluacionDto
{
    public int EvaluacionId { get; set; }
    public string EmpresaNombre { get; set; } = string.Empty;
    public string VacanteTitulo { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
    public DateTime FechaLimite { get; set; }
    public int TotalPreguntas { get; set; }
}

/// <summary>Una pregunta con su respuesta actual (si ya la había guardado) para el formulario de la prueba.</summary>
public class PreguntaRespuestaDto
{
    public int PreguntaId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public int Orden { get; set; }
    public int? ValorSeleccionado { get; set; }
}

/// <summary>Formulario completo de la prueba (todas las preguntas en una sola pantalla).</summary>
public class RealizarEvaluacionDto
{
    public int EvaluacionId { get; set; }
    public string VacanteTitulo { get; set; } = string.Empty;
    public string EmpresaNombre { get; set; } = string.Empty;
    public List<PreguntaRespuestaDto> Preguntas { get; set; } = new();
}

/// <summary>DTO de envío del formulario de respuestas.</summary>
public class GuardarRespuestasDto
{
    public int EvaluacionId { get; set; }
    public Dictionary<int, int> Respuestas { get; set; } = new(); // PreguntaId -> Valor (1-5)
}

/// <summary>Resultados de una evaluación completada, con desglose por rasgo (Fase de resultados).</summary>
public class ResultadoEvaluacionDto
{
    public int EvaluacionId { get; set; }
    public string CandidatoNombre { get; set; } = string.Empty;
    public string VacanteTitulo { get; set; } = string.Empty;
    public string EmpresaNombre { get; set; } = string.Empty;
    public DateTime FechaCompletado { get; set; }
    public int PuntajeCompatibilidad { get; set; }
    public List<RasgoResultadoDto> Rasgos { get; set; } = new();
}

public class RasgoResultadoDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Puntaje { get; set; } // 0-100
    public string Interpretacion { get; set; } = string.Empty;
}
