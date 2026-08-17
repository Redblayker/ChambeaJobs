namespace ChambeaJobs.Domain.Entities;

/// <summary>Respuesta individual del candidato a una pregunta (escala Likert 1-5).</summary>
public class RespuestaPsicometrica
{
    public int EvaluacionId { get; set; }
    public EvaluacionPsicometrica? Evaluacion { get; set; }
    public int PreguntaId { get; set; }
    public PreguntaPsicometrica? Pregunta { get; set; }
    public int Valor { get; set; } // 1 a 5
}
