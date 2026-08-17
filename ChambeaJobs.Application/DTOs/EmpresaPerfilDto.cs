using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ChambeaJobs.Application.DTOs;

/// <summary>DTO de solo lectura del perfil de empresa.</summary>
public class EmpresaPerfilDto
{
    public int Id { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string RUC { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Descripcion { get; set; }
    public string? SitioWeb { get; set; }
    public int UbicacionId { get; set; }
    public string UbicacionNombre { get; set; } = string.Empty;
}

/// <summary>DTO para editar el perfil de empresa.</summary>
public class EditarPerfilEmpresaDto
{
    [Required(ErrorMessage = "El nombre de la empresa es obligatorio")]
    [StringLength(150)]
    public string NombreEmpresa { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Descripcion { get; set; }

    [Url(ErrorMessage = "Ingresa una URL válida")]
    public string? SitioWeb { get; set; }

    [Required(ErrorMessage = "Selecciona una ubicación")]
    public int UbicacionId { get; set; }

    public IFormFile? Logo { get; set; }

    public string? LogoUrlActual { get; set; }
    public List<UbicacionOptionDto> UbicacionesDisponibles { get; set; } = new();
}

/// <summary>DTO para editar el perfil enriquecido de empresa (mejora solicitada: historia, cultura, beneficios, redes).</summary>
public class EditarPerfilExtendidoEmpresaDto
{
    [StringLength(4000)]
    public string? Historia { get; set; }

    [StringLength(2000)]
    public string? Mision { get; set; }

    [StringLength(2000)]
    public string? Vision { get; set; }

    [StringLength(4000)]
    public string? CulturaOrganizacional { get; set; }

    /// <summary>Uno por línea; se muestra como lista con viñetas.</summary>
    [StringLength(2000)]
    public string? Beneficios { get; set; }

    [RegularExpression("^(1-10|11-50|51-200|201-500|500\\+)?$", ErrorMessage = "Selecciona un rango válido de colaboradores.")]
    public string? NumeroColaboradores { get; set; }

    [StringLength(100)]
    public string? SectorEmpresarial { get; set; }

    [StringLength(150)]
    public string? NombreContacto { get; set; }

    [StringLength(30)]
    [RegularExpression(@"^([\d\s\+\-\(\)]{7,30})?$", ErrorMessage = "Ingresa un teléfono válido.")]
    public string? TelefonoContacto { get; set; }

    [Url(ErrorMessage = "Ingresa una URL válida")]
    public string? FacebookUrl { get; set; }

    [Url(ErrorMessage = "Ingresa una URL válida")]
    public string? InstagramUrl { get; set; }

    [Url(ErrorMessage = "Ingresa una URL válida")]
    public string? LinkedInUrl { get; set; }

    [Url(ErrorMessage = "Ingresa una URL válida")]
    public string? TiktokUrl { get; set; }
}

/// <summary>Foto o video de la galería del perfil de empresa.</summary>
public class EmpresaGaleriaDto
{
    public int Id { get; set; }
    public string TipoMedio { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty; // archivo (foto) o link externo (video)
    public string? Titulo { get; set; }
}

/// <summary>Perfil público de la empresa (mejora solicitada): historia, cultura, beneficios, galería, redes, ubicación, colaboradores.</summary>
public class EmpresaPerfilPublicoDto
{
    public int Id { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Descripcion { get; set; }
    public string? SitioWeb { get; set; }
    public string UbicacionNombre { get; set; } = string.Empty;

    public string? Historia { get; set; }
    public string? Mision { get; set; }
    public string? Vision { get; set; }
    public string? CulturaOrganizacional { get; set; }
    public List<string> Beneficios { get; set; } = new();
    public string? NumeroColaboradores { get; set; }

    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TiktokUrl { get; set; }

    public List<EmpresaGaleriaDto> Galeria { get; set; } = new();
    public int VacantesActivas { get; set; }
}

/// <summary>
/// Tarjeta ligera para el feed de "Empresas asociadas" que ve el Candidato
/// al iniciar sesión (Home/Index) — no trae toda la info del perfil
/// extendido, solo lo necesario para la tarjeta clicable.
/// </summary>
public class EmpresaResumenAsociadaDto
{
    public int Id { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string UbicacionNombre { get; set; } = string.Empty;
    public int VacantesActivas { get; set; }
}
