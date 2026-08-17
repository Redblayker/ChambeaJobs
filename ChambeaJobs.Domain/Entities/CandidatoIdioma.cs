namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Tabla intermedia N:M entre Candidato e Idioma, con el nivel de dominio
/// según el Marco Común Europeo de Referencia (A1-C2). Clave primaria
/// compuesta (CandidatoId, IdiomaId) configurada en ApplicationDbContext.
/// </summary>
public class CandidatoIdioma
{
    public int CandidatoId { get; set; }
    public Candidato? Candidato { get; set; }
    public int IdiomaId { get; set; }
    public Idioma? Idioma { get; set; }

    /// <summary>Nivel MCER: A1, A2, B1, B2, C1, C2 o "Nativo".</summary>
    public string Nivel { get; set; } = string.Empty;

    public static class Niveles
    {
        public const string A1 = "A1";
        public const string A2 = "A2";
        public const string B1 = "B1";
        public const string B2 = "B2";
        public const string C1 = "C1";
        public const string C2 = "C2";
        public const string Nativo = "Nativo";

        public static readonly string[] Todos = { A1, A2, B1, B2, C1, C2, Nativo };
    }
}
