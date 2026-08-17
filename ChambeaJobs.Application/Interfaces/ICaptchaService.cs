namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Verifica un token de Google reCAPTCHA v2 ("No soy un robot") contra el
/// servidor de Google antes de procesar un login o registro — mitiga bots
/// que intenten probar contraseñas o crear cuentas en masa.
///
/// Igual que Google Sign-In y PayPal: si no hay SiteKey/SecretKey
/// configurados, CaptchaDisponible es false y el sistema simplemente no
/// exige el CAPTCHA (para no bloquear el desarrollo local sin credenciales).
/// </summary>
public interface ICaptchaService
{
    bool CaptchaDisponible { get; }
    string SiteKeyPublica { get; }

    /// <summary>true si el token es válido, o si CaptchaDisponible es false (nada que verificar).</summary>
    Task<bool> VerificarAsync(string? token);
}
