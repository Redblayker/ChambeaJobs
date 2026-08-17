using System.ComponentModel.DataAnnotations;

namespace ChambeaJobs.Application.DTOs;

/// <summary>DTO de solo lectura para la pantalla "Perfil Candidato (ver)".</summary>
public class CandidatoPerfilDto
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public string? Direccion { get; set; }
    public string? Disponibilidad { get; set; }
    public string? CvNombreOriginal { get; set; }
    public string? CvUrl { get; set; }
    public string? VideoCvUrl { get; set; }
    public int PorcentajeCompletitud { get; set; }
    public List<ExperienciaDto> Experiencias { get; set; } = new();
    public List<EducacionDto> Educaciones { get; set; } = new();
    public List<HabilidadCandidatoDto> Habilidades { get; set; } = new();
    public List<IdiomaCandidatoDto> Idiomas { get; set; } = new();
    public List<CertificadoDto> Certificados { get; set; } = new();
    public List<CursoDto> Cursos { get; set; } = new();
}

/// <summary>Idioma del catálogo, para poblar el &lt;select&gt; al agregar un idioma al perfil.</summary>
public class IdiomaOptionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Idioma que domina el candidato, con su nivel MCER.</summary>
public class IdiomaCandidatoDto
{
    public int IdiomaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
}

/// <summary>Certificado (PDF) adjuntado por el candidato.</summary>
public class CertificadoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? InstitucionEmisora { get; set; }
    public DateTime? FechaObtencion { get; set; }
    public string ArchivoUrl { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
}

/// <summary>Curso o capacitación completada por el candidato.</summary>
public class CursoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Institucion { get; set; }
    public int? HorasDuracion { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public string? ArchivoUrl { get; set; }
}

public class ExperienciaDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la empresa es obligatorio")]
    [StringLength(150)]
    public string NombreEmpresa { get; set; } = string.Empty;

    [Required(ErrorMessage = "El puesto es obligatorio")]
    [StringLength(150)]
    public string Puesto { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaFin { get; set; }

    [StringLength(1000)]
    public string? Descripcion { get; set; }
}

public class EducacionDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La institución es obligatoria")]
    [StringLength(150)]
    public string Institucion { get; set; } = string.Empty;

    /// <summary>Institución elegida del catálogo (universidad/INATEC). Nulo si se escribió "Otra institución" a mano.</summary>
    public int? InstitucionId { get; set; }

    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(150)]
    public string TituloObtenido { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona el nivel educativo")]
    public string NivelEducativo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaFin { get; set; }

    public int? CategoriaId { get; set; }

    /// <summary>Solo lectura: nombre de la categoría, para mostrarla en el perfil.</summary>
    public string? CategoriaNombre { get; set; }

    /// <summary>Carrera específica dentro de la categoría (opcional, ej. "Ingeniería en Sistemas" dentro de "Tecnología").</summary>
    public int? CarreraId { get; set; }

    /// <summary>Solo lectura: nombre de la carrera, para mostrarla en el perfil.</summary>
    public string? CarreraNombre { get; set; }
}

public class HabilidadCandidatoDto
{
    public int HabilidadId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? NivelDominio { get; set; }
}

/// <summary>DTO para el formulario "Editar Perfil Candidato".</summary>
public class EditarPerfilCandidatoDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Direccion { get; set; }

    public string? Disponibilidad { get; set; }

    /// <summary>Nueva foto de perfil (opcional; si es null se conserva la actual).</summary>
    public Microsoft.AspNetCore.Http.IFormFile? Foto { get; set; }

    /// <summary>Nuevo video currículum (archivo de video, grabado o subido); si es null se conserva el actual.</summary>
    public Microsoft.AspNetCore.Http.IFormFile? VideoCv { get; set; }

    /// <summary>Solo lectura: ruta del video CV actualmente guardado, para mostrarlo/reproducirlo en la vista.</summary>
    public string? VideoCvUrlActual { get; set; }

    /// <summary>Nuevo CV en PDF (opcional; si es null se conserva el actual).</summary>
    public Microsoft.AspNetCore.Http.IFormFile? Cv { get; set; }

    public string? FotoUrlActual { get; set; }
    public string? CvNombreActual { get; set; }
}
