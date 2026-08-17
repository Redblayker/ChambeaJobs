using System.ComponentModel.DataAnnotations;

namespace ChambeaJobs.Application.DTOs;

/// <summary>DTO para publicar/editar una pasantía.</summary>
public class PasantiaFormDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona una categoría")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "Selecciona una ubicación")]
    public int UbicacionId { get; set; }

    [Required(ErrorMessage = "Selecciona la modalidad")]
    public string Modalidad { get; set; } = string.Empty; // Presencial, Remoto, Híbrido

    [Required(ErrorMessage = "La descripción es obligatoria")]
    public string Descripcion { get; set; } = string.Empty;

    public string? Requisitos { get; set; }

    [Required(ErrorMessage = "Indica la duración en meses")]
    [Range(1, 24, ErrorMessage = "La duración debe ser entre 1 y 24 meses")]
    public int DuracionMeses { get; set; } = 3;

    public bool EsRemunerada { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto debe ser un valor positivo")]
    public decimal? MontoRemuneracion { get; set; }

    [Required(ErrorMessage = "La fecha de cierre es obligatoria")]
    [DataType(DataType.Date)]
    public DateTime FechaCierre { get; set; } = DateTime.UtcNow.AddDays(30);

    public List<CategoriaOptionDto> CategoriasDisponibles { get; set; } = new();
    public List<UbicacionOptionDto> UbicacionesDisponibles { get; set; } = new();
}

/// <summary>DTO resumen para el listado "Mis Pasantías" (lado Empresa).</summary>
public class PasantiaListItemDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int NumeroPostulantes { get; set; }
}

/// <summary>DTO de detalle público de una pasantía.</summary>
public class PasantiaDetalleDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
    public string EmpresaNombre { get; set; } = string.Empty;
    public string? EmpresaLogoUrl { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public string UbicacionNombre { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public int DuracionMeses { get; set; }
    public bool EsRemunerada { get; set; }
    public decimal? MontoRemuneracion { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Requisitos { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public DateTime FechaCierre { get; set; }
    public string Estado { get; set; } = string.Empty;
}

/// <summary>Fila de "Postulantes de la Pasantía" (lado Empresa).</summary>
public class PostulantePasantiaDto
{
    public int PostulacionId { get; set; }
    public int CandidatoId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public string? CvUrl { get; set; }
    public string? Telefono { get; set; }
    public DateTime FechaPostulacion { get; set; }
    public string Estado { get; set; } = string.Empty;
}

/// <summary>Fila de "Gestión de Pasantías" (lado Admin).</summary>
public class PasantiaAdminDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string EmpresaNombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; }
    public int NumeroPostulantes { get; set; }
}

/// <summary>Fila de "Mis Postulaciones a Pasantías" (lado Candidato).</summary>
public class PostulacionPasantiaCandidatoDto
{
    public int Id { get; set; }
    public int PasantiaId { get; set; }
    public string PasantiaTitulo { get; set; } = string.Empty;
    public string EmpresaNombre { get; set; } = string.Empty;
    public DateTime FechaPostulacion { get; set; }
    public string Estado { get; set; } = string.Empty;
}
