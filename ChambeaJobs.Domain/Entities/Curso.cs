namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Curso o capacitación completada por el candidato. Independiente de
/// <see cref="Certificado"/>: un curso puede o no tener un PDF adjunto
/// (ArchivoId es opcional, a diferencia del certificado que siempre lo exige).
/// </summary>
public class Curso : BaseEntity
{
    public int CandidatoId { get; set; }
    public Candidato? Candidato { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? Institucion { get; set; }
    public int? HorasDuracion { get; set; }
    public DateTime? FechaFinalizacion { get; set; }

    public int? ArchivoId { get; set; }
    public Archivo? Archivo { get; set; }
}
