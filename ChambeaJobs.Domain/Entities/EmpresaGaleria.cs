namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Elemento multimedia (foto o video) del perfil público de una empresa.
/// Las fotos se guardan como Archivo físico; los videos se referencian por
/// URL externa (YouTube/Vimeo) para no cargar el servidor con streaming.
/// </summary>
public class EmpresaGaleria : BaseEntity
{
    public int EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public string TipoMedio { get; set; } = TiposMedio.Foto;

    /// <summary>Para fotos: archivo subido. Null si TipoMedio = Video.</summary>
    public int? ArchivoId { get; set; }
    public Archivo? Archivo { get; set; }

    /// <summary>Para videos: URL externa embebible (YouTube/Vimeo). Null si TipoMedio = Foto.</summary>
    public string? UrlVideo { get; set; }

    public string? Titulo { get; set; }
    public int Orden { get; set; } = 0;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public static class TiposMedio
    {
        public const string Foto = "Foto";
        public const string Video = "Video";
    }
}
