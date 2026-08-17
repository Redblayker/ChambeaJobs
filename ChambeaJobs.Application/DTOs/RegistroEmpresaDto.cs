using System.ComponentModel.DataAnnotations;

namespace ChambeaJobs.Application.DTOs;

/// <summary>
/// Datos recibidos del formulario de Registro Empresa.
/// </summary>
public class RegistroEmpresaDto
{
    [Required(ErrorMessage = "El nombre de la empresa es obligatorio")]
    [StringLength(150)]
    public string NombreEmpresa { get; set; } = string.Empty;

    [Required(ErrorMessage = "El RUC/identificación fiscal es obligatorio")]
    [StringLength(30)]
    public string RUC { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo corporativo es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [RegularExpression(@"^\d{4}-?\d{4}$", ErrorMessage = "Ingresa un número de teléfono válido de Nicaragua (8 dígitos, ej. 8888-8888)")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona la ubicación de la empresa")]
    public int UbicacionId { get; set; }

    [Required(ErrorMessage = "Selecciona un plan")]
    public int PlanSuscripcionId { get; set; }

    /// <summary>Planes disponibles para poblar las tarjetas de selección en la vista. No se envía desde el formulario.</summary>
    public List<PlanSuscripcionOptionDto> PlanesDisponibles { get; set; } = new();

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes confirmar la contraseña")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmarPassword { get; set; } = string.Empty;

    /// <summary>Validado manualmente en el controlador (ver nota en RegistroCandidatoDto).</summary>
    public bool AceptaTerminos { get; set; }

    /// <summary>
    /// Lista de ubicaciones para poblar el &lt;select&gt; en la vista.
    /// No se envía desde el formulario; el controlador la carga antes de renderizar.
    /// </summary>
    public List<UbicacionOptionDto> UbicacionesDisponibles { get; set; } = new();
}

public class UbicacionOptionDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty; // "Managua, Managua"
}
