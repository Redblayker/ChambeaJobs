using System.ComponentModel.DataAnnotations;

namespace ChambeaJobs.Application.DTOs;

public class CategoriaOptionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Subcategoría de carrera dentro de una Categoría, para el desplegable dependiente en Educación.</summary>
public class CarreraOptionDto
{
    public int Id { get; set; }
    public int CategoriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Universidad o instituto técnico (INATEC), para el catálogo de "Institución" en Educación.</summary>
public class InstitucionOptionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}

/// <summary>Facultad/área de conocimiento de una Institución — paso 2 del catálogo de 3 niveles.</summary>
public class FacultadOptionDto
{
    public int Id { get; set; }
    public int InstitucionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Carrera ofrecida por una Facultad específica — incluye la categoría, para agrupar el desplegable visualmente.</summary>
public class CarreraDeInstitucionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
}

/// <summary>DTO para publicar/editar una vacante.</summary>
public class VacanteFormDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona una categoría")]
    public int CategoriaId { get; set; }

    /// <summary>Carrera específica dentro de la categoría (opcional, ej. "Ingeniería en Sistemas" dentro de "Tecnología").</summary>
    public int? CarreraId { get; set; }

    [Required(ErrorMessage = "Selecciona una ubicación")]
    public int UbicacionId { get; set; }

    [Required(ErrorMessage = "Selecciona la modalidad")]
    public string Modalidad { get; set; } = string.Empty; // Presencial, Remoto, Híbrido

    [Required(ErrorMessage = "Selecciona la experiencia requerida")]
    public string ExperienciaRequerida { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria")]
    public string Descripcion { get; set; } = string.Empty;

    public string? Requisitos { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El salario debe ser un valor positivo")]
    public decimal? SalarioMin { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El salario debe ser un valor positivo")]
    public decimal? SalarioMax { get; set; }

    [Required(ErrorMessage = "La fecha de cierre es obligatoria")]
    [DataType(DataType.Date)]
    public DateTime FechaCierre { get; set; } = DateTime.UtcNow.AddDays(30);

    /// <summary>Solo se guarda en true si el plan de la empresa lo permite (validado también en el servidor).</summary>
    public bool EsDestacada { get; set; }

    public List<CategoriaOptionDto> CategoriasDisponibles { get; set; } = new();
    public List<UbicacionOptionDto> UbicacionesDisponibles { get; set; } = new();
}

/// <summary>DTO resumen para el listado "Mis Vacantes".</summary>
public class VacanteListItemDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int NumeroPostulantes { get; set; }
    public bool EsDestacada { get; set; }
}

/// <summary>DTO de detalle público de una vacante.</summary>
public class VacanteDetalleDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
    public string EmpresaNombre { get; set; } = string.Empty;
    public string? EmpresaLogoUrl { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public string UbicacionNombre { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public string ExperienciaRequerida { get; set; } = string.Empty;
    public decimal? SalarioMin { get; set; }
    public decimal? SalarioMax { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Requisitos { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public DateTime FechaCierre { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool EsDestacada { get; set; }
}
