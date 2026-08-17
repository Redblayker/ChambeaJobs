using System.ComponentModel.DataAnnotations;

namespace ChambeaJobs.Application.DTOs;

/// <summary>
/// Datos recibidos del formulario de Registro Candidato.
/// Se valida tanto aquí (servidor) como en el cliente (JS).
/// </summary>
public class RegistroCandidatoDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [RegularExpression(@"^\d{4}-?\d{4}$", ErrorMessage = "Ingresa un número de teléfono válido de Nicaragua (8 dígitos, ej. 8888-8888)")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes confirmar la contraseña")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmarPassword { get; set; } = string.Empty;

    /// <summary>
    /// Validado manualmente en el controlador (no con Data Annotations),
    /// porque [Range(typeof(bool),..)] en checkboxes genera validación
    /// cliente inconsistente con jQuery Validate y puede bloquear el envío
    /// del formulario sin mostrar ningún error visible.
    /// </summary>
    public bool AceptaTerminos { get; set; }
}
