namespace ChambeaJobs.Domain.Entities;

/// <summary>Postulación de un Candidato a una Pasantía (separada de Postulacion, que es solo para Vacantes de empleo).</summary>
public class PostulacionPasantia : BaseEntity
{
    public int CandidatoId { get; set; }
    public Candidato? Candidato { get; set; }
    public int PasantiaId { get; set; }
    public Pasantia? Pasantia { get; set; }

    public DateTime FechaPostulacion { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = EstadosPostulacionPasantia.Postulado;
    public string? NotaEmpresa { get; set; }
}

public static class EstadosPostulacionPasantia
{
    public const string Postulado = "Postulado";
    public const string EnRevision = "En revisión";
    public const string Aceptado = "Aceptado";
    public const string Rechazado = "Rechazado";
}
