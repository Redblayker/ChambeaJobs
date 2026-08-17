using System.ComponentModel.DataAnnotations;

namespace ChambeaJobs.Application.DTOs;

/// <summary>DTO del formulario "Recuperar contraseña".</summary>
public class RecuperarPasswordDto
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    public string Correo { get; set; } = string.Empty;
}

/// <summary>DTO del formulario "Restablecer contraseña".</summary>
public class RestablecerPasswordDto
{
    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes confirmar la contraseña")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}
