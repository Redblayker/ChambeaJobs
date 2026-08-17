namespace ChambeaJobs.Domain.Entities;

/// <summary>Oferta de empleo publicada por una Empresa.</summary>
public class Vacante : BaseEntity
{
    public int EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    /// <summary>Carrera específica dentro de la Categoría (opcional, ej. "Ingeniería en Sistemas" dentro de "Tecnología").</summary>
    public int? CarreraId { get; set; }
    public Carrera? Carrera { get; set; }
    public int UbicacionId { get; set; }
    public Ubicacion? Ubicacion { get; set; }
    public int PaqueteEmpresaId { get; set; }
    public PaqueteEmpresa? PaqueteEmpresa { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? Requisitos { get; set; }
    public string Modalidad { get; set; } = string.Empty; // Presencial, Remoto, Híbrido
    public decimal? SalarioMin { get; set; }
    public decimal? SalarioMax { get; set; }

    /// <summary>Experiencia laboral que pide la vacante (ej. "1 año", "De 1 a 3 años"). Ver ExperienciaRequeridaOpciones para los valores válidos.</summary>
    public string ExperienciaRequerida { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaCierre { get; set; }
    public string Estado { get; set; } = EstadosVacante.Activa;

    /// <summary>Solo puede activarse si la Empresa tiene un plan que lo permita (Plan Empresarial). Las vacantes destacadas aparecen primero en las búsquedas, con una estrella.</summary>
    public bool EsDestacada { get; set; }
}

public static class EstadosVacante
{
    public const string Activa = "Activa";
    public const string Cerrada = "Cerrada";
}

/// <summary>
/// Opciones fijas para el campo ExperienciaRequerida — un desplegable, no
/// texto libre, para que las búsquedas/filtros futuros puedan compararlas
/// de forma exacta en vez de tener que interpretar texto escrito a mano
/// por cada empresa (ej. "2 años" vs "dos años" vs "2 años de experiencia").
/// </summary>
public static class ExperienciaRequeridaOpciones
{
    public static readonly string[] Todas =
    {
        "Sin experiencia",
        "Menos de 1 año",
        "1 año",
        "2 años",
        "3 años",
        "De 1 a 3 años",
        "De 3 a 5 años",
        "Más de 5 años",
    };
}
