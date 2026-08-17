namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Documento adjuntado por el candidato en formato PDF: certificaciones,
/// títulos académicos, diplomas, licencias profesionales, portafolios,
/// cartas de recomendación o constancias laborales. Se apoya en la
/// entidad genérica <see cref="Archivo"/> para el almacenamiento físico.
/// </summary>
public class Certificado : BaseEntity
{
    public int CandidatoId { get; set; }
    public Candidato? Candidato { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? InstitucionEmisora { get; set; }
    public DateTime? FechaObtencion { get; set; }
    public string TipoDocumento { get; set; } = TiposDocumento.Certificado;

    public int ArchivoId { get; set; }
    public Archivo? Archivo { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

public static class TiposDocumento
{
    public const string Certificado = "Certificado";
    public const string TituloAcademico = "Título académico";
    public const string Diploma = "Diploma";
    public const string LicenciaProfesional = "Licencia profesional";
    public const string Portafolio = "Portafolio";
    public const string CartaRecomendacion = "Carta de recomendación";
    public const string ConstanciaLaboral = "Constancia laboral";
    public const string RecordPolicial = "Récord Policial";

    public static readonly string[] Todos =
    {
        Certificado, TituloAcademico, Diploma, LicenciaProfesional, Portafolio, CartaRecomendacion, ConstanciaLaboral, RecordPolicial
    };
}
