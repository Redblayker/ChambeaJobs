namespace ChambeaJobs.Domain.Entities;

/// <summary>Oferta de pasantía publicada por una Empresa (independiente de las Vacantes de empleo).</summary>
public class Pasantia : BaseEntity
{
    public int EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public int UbicacionId { get; set; }
    public Ubicacion? Ubicacion { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? Requisitos { get; set; }
    public string Modalidad { get; set; } = string.Empty; // Presencial, Remoto, Híbrido
    public int DuracionMeses { get; set; }
    public bool EsRemunerada { get; set; }
    public decimal? MontoRemuneracion { get; set; }
    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaCierre { get; set; }
    public string Estado { get; set; } = EstadosPasantia.Activa;

    public ICollection<PostulacionPasantia> Postulaciones { get; set; } = new List<PostulacionPasantia>();
}

public static class EstadosPasantia
{
    public const string Activa = "Activa";
    public const string Cerrada = "Cerrada";
}
