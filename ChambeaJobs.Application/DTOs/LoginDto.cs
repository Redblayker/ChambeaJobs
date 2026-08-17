using System.ComponentModel.DataAnnotations;

namespace ChambeaJobs.Application.DTOs;

/// <summary>
/// Datos recibidos del formulario de Login.
/// </summary>
public class LoginDto
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool Recordarme { get; set; }

    /// <summary>
    /// URL a la que volver tras un login exitoso, si el usuario llegó
    /// a Login por un [Authorize] fallido en otra pantalla.
    /// </summary>
    public string? ReturnUrl { get; set; }
}

/// <summary>
/// Datos para la pantalla de configuración de autenticación en dos pasos
/// (2FA) — la clave y la URL "otpauth://" solo se generan cuando el usuario
/// TODAVÍA no la tiene activada; si ya la activó, solo importa YaActivado.
/// </summary>
public class ConfigurarDobleFactorDto
{
    public bool YaActivado { get; set; }
    public string? ClaveManual { get; set; }
    public string? UrlOtpAuth { get; set; }
}
