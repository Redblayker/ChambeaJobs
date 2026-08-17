namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Banco de preguntas de la evaluación psicométrica (modelo Big Five).
/// Cada pregunta mide uno de los 5 rasgos de personalidad mediante
/// una escala Likert de 1 (totalmente en desacuerdo) a 5 (totalmente
/// de acuerdo). Las preguntas marcadas como "EsInversa" se puntúan al
/// revés (6 - valor) porque están redactadas en sentido contrario al rasgo.
/// </summary>
public class PreguntaPsicometrica : BaseEntity
{
    public string Texto { get; set; } = string.Empty;
    public string Rasgo { get; set; } = string.Empty; // ver RasgosBigFive
    public bool EsInversa { get; set; }
    public int Orden { get; set; }
}

public static class RasgosBigFive
{
    public const string Responsabilidad = "Responsabilidad";
    public const string Extraversion = "Extraversión";
    public const string Amabilidad = "Amabilidad";
    public const string Apertura = "Apertura";
    public const string EstabilidadEmocional = "Estabilidad Emocional";

    public static readonly string[] Todos =
    {
        Responsabilidad, Extraversion, Amabilidad, Apertura, EstabilidadEmocional
    };
}
