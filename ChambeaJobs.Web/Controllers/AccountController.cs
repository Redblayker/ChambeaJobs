using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// Controlador de autenticación y registro. Cubre las pantallas
/// (Login, Registro selector, Registro Candidato,
/// Registro Empresa, Recuperar contraseña, Restablecer contraseña).
/// </summary>
[EnableRateLimiting("autenticacion")]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ICandidatoService _candidatoService;
    private readonly IEmpresaService _empresaService;
    private readonly ICatalogoService _catalogoService;
    private readonly IEmailSender _emailSender;
    private readonly IPaqueteEmpresaService _paqueteEmpresaService;
    private readonly IConfiguration _configuracion;
    private readonly ICaptchaService _captchaService;
    private readonly IAuditoriaService _auditoriaService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ICandidatoService candidatoService,
        IEmpresaService empresaService,
        ICatalogoService catalogoService,
        IEmailSender emailSender,
        IPaqueteEmpresaService paqueteEmpresaService,
        IConfiguration configuracion,
        ICaptchaService captchaService,
        IAuditoriaService auditoriaService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _candidatoService = candidatoService;
        _empresaService = empresaService;
        _catalogoService = catalogoService;
        _emailSender = emailSender;
        _paqueteEmpresaService = paqueteEmpresaService;
        _configuracion = configuracion;
        _captchaService = captchaService;
        _auditoriaService = auditoriaService;
    }

    /// <summary>true solo si se configuraron credenciales de Google — así el botón "Continuar con Google" no se muestra si no está listo para usarse.</summary>
    private bool GoogleDisponible =>
        !string.IsNullOrWhiteSpace(_configuracion["Authentication:Google:ClientId"]);

    // ---------- Envío del correo de confirmación (compartido por ambos registros) ----------

    private async Task EnviarCorreoConfirmacionAsync(ApplicationUser usuario, string nombreParaSaludo)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);

        var enlaceConfirmacion = Url.Action(
            nameof(ConfirmarCorreo), "Account",
            new { usuarioId = usuario.Id, token },
            protocol: Request.Scheme);

        var cuerpoHtml = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;"">
                <h2 style=""color:#14497A;"">¡Bienvenido a ChambeaJobs, {nombreParaSaludo}!</h2>
                <p>Gracias por crear tu cuenta. Antes de poder iniciar sesión, confirma tu correo electrónico haciendo clic en el siguiente botón:</p>
                <p style=""text-align:center; margin: 30px 0;"">
                    <a href=""{enlaceConfirmacion}"" style=""background:#F0661E; color:white; padding:14px 28px; text-decoration:none; border-radius:8px; font-weight:bold;"">
                        Confirmar mi correo
                    </a>
                </p>
                <p style=""color:#616B7A; font-size:13px;"">Si el botón no funciona, copia y pega este enlace en tu navegador:<br/>{enlaceConfirmacion}</p>
                <p style=""color:#616B7A; font-size:13px;"">Si tú no creaste esta cuenta, puedes ignorar este correo.</p>
            </div>";

        await _emailSender.EnviarAsync(usuario.Email!, "Confirma tu correo — ChambeaJobs", cuerpoHtml);
    }

    // ---------- Selector de rol ----------

    [HttpGet]
    // Ya no existe una "página" de selección de tipo de cuenta — ahora es un
    // modal (ver _Layout.cshtml), igual que el login. Esta acción se
    // conserva solo por si algún enlace viejo o marcador apunta aquí
    // directamente; simplemente redirige al Home con la bandera que abre el modal.
    public IActionResult RegistroSelector() => Redirect("/?abrirRegistro=true");

    // ---------- Registro Candidato ----------

    // Ya no existe una "página" de registro — ahora vive como modal en
    // _Layout.cshtml (igual que Login). Esta acción se conserva solo como
    // respaldo para enlaces viejos.
    [HttpGet]
    public IActionResult RegistroCandidato() => Redirect("/?abrirRegistroCandidato=true");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistroCandidato(RegistroCandidatoDto modelo)
    {
        if (!modelo.AceptaTerminos)
        {
            ModelState.AddModelError(nameof(modelo.AceptaTerminos), "Debes aceptar los términos y condiciones para continuar.");
        }

        if (!ModelState.IsValid)
        {
            return RespuestaErrorRegistro();
        }

        if (!await CaptchaValidoAsync())
        {
            ModelState.AddModelError(string.Empty, "No se pudo verificar que no eres un robot. Vuelve a marcar la casilla del CAPTCHA.");
            return RespuestaErrorRegistro();
        }

        var correoExistente = await _userManager.FindByEmailAsync(modelo.Correo);
        if (correoExistente is not null)
        {
            ModelState.AddModelError(nameof(modelo.Correo), "Ya existe una cuenta registrada con este correo.");
            return RespuestaErrorRegistro();
        }

        var telefonoExistente = await _userManager.Users.AnyAsync(u => u.PhoneNumber == modelo.Telefono);
        if (telefonoExistente)
        {
            ModelState.AddModelError(nameof(modelo.Telefono), "Ya existe una cuenta registrada con este número de teléfono.");
            return RespuestaErrorRegistro();
        }

        var usuario = new ApplicationUser
        {
            UserName = modelo.Correo,
            Email = modelo.Correo,
            PhoneNumber = modelo.Telefono,
            EstadoCuenta = true,
            FechaRegistro = DateTime.UtcNow
        };

        var resultado = await _userManager.CreateAsync(usuario, modelo.Password);

        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return RespuestaErrorRegistro();
        }

        await _userManager.AddToRoleAsync(usuario, RolesSistema.Candidato);

        await _candidatoService.CrearPerfilInicialAsync(usuario.Id, modelo.Nombres, modelo.Apellidos);

        // ⚠️ DESACTIVADO TEMPORALMENTE (igual que el bloqueo en Login): mientras
        // se corre en local sin SMTP real, no tiene sentido exigir un correo
        // que nunca va a llegar. Se marca EmailConfirmed=true automáticamente
        // y se inicia sesión directo. Para reactivar el flujo de confirmación,
        // descomenta el envío de correo + redirect de abajo y quita estas 2 líneas.
        usuario.EmailConfirmed = true;
        await _userManager.UpdateAsync(usuario);
        // await EnviarCorreoConfirmacionAsync(usuario, modelo.Nombres);
        // return RedirectToAction(nameof(RevisaTuCorreo), new { correo = modelo.Correo });

        await _signInManager.SignInAsync(usuario, isPersistent: false);

        var urlDestino = Url.Action("Perfil", "Candidato") ?? "/";
        if (EsPeticionAjax())
        {
            return Ok(new { exito = true, redirectUrl = urlDestino });
        }
        return Redirect(urlDestino);
    }

    /// <summary>
    /// Junta todos los errores de ModelState en una respuesta JSON para el
    /// modal (que ya no tiene una View propia donde volcarlos campo por
    /// campo con asp-validation-for — se muestran juntos en un resumen).
    /// </summary>
    private IActionResult RespuestaErrorRegistro()
    {
        var mensajes = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        if (mensajes.Count == 0)
        {
            mensajes.Add("Revisa los datos ingresados.");
        }
        return BadRequest(new { exito = false, mensajes });
    }

    // ---------- Registro Empresa ----------

    [HttpGet]
    public IActionResult RegistroEmpresa() => Redirect("/?abrirRegistroEmpresa=true");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistroEmpresa(RegistroEmpresaDto modelo)
    {
        if (!modelo.AceptaTerminos)
        {
            ModelState.AddModelError(nameof(modelo.AceptaTerminos), "Debes aceptar los términos y condiciones para continuar.");
        }

        if (!ModelState.IsValid)
        {
            return RespuestaErrorRegistro();
        }

        if (!await CaptchaValidoAsync())
        {
            ModelState.AddModelError(string.Empty, "No se pudo verificar que no eres un robot. Vuelve a marcar la casilla del CAPTCHA.");
            return RespuestaErrorRegistro();
        }

        var correoExistente = await _userManager.FindByEmailAsync(modelo.Correo);
        if (correoExistente is not null)
        {
            ModelState.AddModelError(nameof(modelo.Correo), "Ya existe una cuenta registrada con este correo.");
            return RespuestaErrorRegistro();
        }

        if (await _empresaService.ExisteRucAsync(modelo.RUC))
        {
            ModelState.AddModelError(nameof(modelo.RUC), "Ya existe una empresa registrada con este RUC.");
            return RespuestaErrorRegistro();
        }

        var telefonoExistente = await _userManager.Users.AnyAsync(u => u.PhoneNumber == modelo.Telefono);
        if (telefonoExistente)
        {
            ModelState.AddModelError(nameof(modelo.Telefono), "Ya existe una cuenta registrada con este número de teléfono.");
            return RespuestaErrorRegistro();
        }

        var usuario = new ApplicationUser
        {
            UserName = modelo.Correo,
            Email = modelo.Correo,
            PhoneNumber = modelo.Telefono,
            EstadoCuenta = true,
            FechaRegistro = DateTime.UtcNow
        };

        var resultado = await _userManager.CreateAsync(usuario, modelo.Password);

        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return RespuestaErrorRegistro();
        }

        await _userManager.AddToRoleAsync(usuario, RolesSistema.Empresa);

        await _empresaService.CrearPerfilInicialAsync(usuario.Id, modelo.NombreEmpresa, modelo.RUC, modelo.UbicacionId, modelo.PlanSuscripcionId);

        // ⚠️ DESACTIVADO TEMPORALMENTE — ver la misma nota en RegistroCandidato.
        usuario.EmailConfirmed = true;
        await _userManager.UpdateAsync(usuario);
        // await EnviarCorreoConfirmacionAsync(usuario, modelo.NombreEmpresa);
        // return RedirectToAction(nameof(RevisaTuCorreo), new { correo = modelo.Correo });

        await _signInManager.SignInAsync(usuario, isPersistent: false);

        var urlDestino = Url.Action("Dashboard", "Empresa") ?? "/";
        if (EsPeticionAjax())
        {
            return Ok(new { exito = true, redirectUrl = urlDestino });
        }
        return Redirect(urlDestino);
    }

    // ---------- Login ----------

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        // Ya no existe una "página" de Login independiente — TODO el login
        // pasa por el modal (ver _Layout.cshtml). Esta acción solo existe
        // porque ASP.NET Core Identity necesita una URL configurada como
        // "LoginPath" a la que redirigir automáticamente cuando alguien sin
        // sesión intenta entrar a una página protegida (ej. Centro de
        // Soporte) — en vez de renderizar un formulario aquí, se manda al
        // Home con una bandera que la propia página detecta y abre el modal.
        if (_signInManager.IsSignedIn(User))
        {
            return await RedirigirSegunRolAsync();
        }

        var destino = "/?abrirLogin=true";
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            destino += "&returnUrl=" + Uri.EscapeDataString(returnUrl);
        }
        return Redirect(destino);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>true si la petición viene del fetch() del modal de login (no una navegación normal de página completa).</summary>
    private bool EsPeticionAjax() =>
        string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Verifica el CAPTCHA contra Google antes de procesar Login/Registro.
    /// Si no está configurado (SiteKey/SecretKey vacíos), no bloquea nada —
    /// mismo criterio de "si no está listo, no se exige" que Google Sign-In.
    /// </summary>
    private async Task<bool> CaptchaValidoAsync() =>
        await _captchaService.VerificarAsync(Request.Form["g-recaptcha-response"]);

    /// <summary>
    /// Calcula a dónde debe ir un usuario ya autenticado según su rol —
    /// devuelto como string (no como IActionResult) para poder reutilizarlo
    /// tanto en la navegación normal (Redirect) como en la respuesta JSON
    /// del modal de login (redirectUrl).
    /// </summary>
    private async Task<string> ObtenerUrlDestinoAsync(ApplicationUser usuario)
    {
        if (await _userManager.IsInRoleAsync(usuario, RolesSistema.Administrador) || await _userManager.IsInRoleAsync(usuario, RolesSistema.SuperAdministrador))
        {
            return Url.Action("Dashboard", "Admin") ?? "/";
        }
        if (await _userManager.IsInRoleAsync(usuario, RolesSistema.Empresa))
        {
            return Url.Action("Dashboard", "Empresa") ?? "/";
        }
        if (await _userManager.IsInRoleAsync(usuario, RolesSistema.Candidato))
        {
            return Url.Action("Perfil", "Candidato") ?? "/";
        }
        return Url.Action("Index", "Home") ?? "/";
    }

    public async Task<IActionResult> Login(LoginDto modelo)
    {
        ViewBag.GoogleDisponible = GoogleDisponible;

        if (!ModelState.IsValid)
        {
            if (EsPeticionAjax())
            {
                return BadRequest(new { exito = false, mensaje = "Revisa los datos ingresados." });
            }
            TempData["ErrorLogin"] = "Revisa los datos ingresados.";
            return Redirect("/?abrirLogin=true");
        }

        if (!await CaptchaValidoAsync())
        {
            if (EsPeticionAjax())
            {
                return BadRequest(new { exito = false, mensaje = "No se pudo verificar que no eres un robot. Vuelve a marcar la casilla del CAPTCHA." });
            }
            TempData["ErrorLogin"] = "No se pudo verificar que no eres un robot. Vuelve a marcar la casilla del CAPTCHA.";
            return Redirect("/?abrirLogin=true");
        }

        var usuario = await _userManager.FindByEmailAsync(modelo.Correo);

        if (usuario is null)
        {
            if (EsPeticionAjax())
            {
                return Unauthorized(new { exito = false, mensaje = "Correo o contraseña incorrectos." });
            }
            TempData["ErrorLogin"] = "Correo o contraseña incorrectos.";
            return Redirect("/?abrirLogin=true");
        }

        if (!usuario.EstadoCuenta)
        {
            if (EsPeticionAjax())
            {
                return Unauthorized(new { exito = false, mensaje = "Esta cuenta se encuentra suspendida. Contacta a soporte." });
            }
            TempData["ErrorLogin"] = "Esta cuenta se encuentra suspendida. Contacta a soporte.";
            return Redirect("/?abrirLogin=true");
        }

        // ⚠️ DESACTIVADO TEMPORALMENTE: mientras se corre 100% en local sin un
        // servidor SMTP configurado, exigir el correo confirmado bloquearía a
        // todos los usuarios (el correo de confirmación nunca llega). Vuelve a
        // poner esta condición en "if (!usuario.EmailConfirmed)" en cuanto haya
        // un SMTP real configurado en producción.
        if (false && !usuario.EmailConfirmed)
        {
            if (EsPeticionAjax())
            {
                return Unauthorized(new { exito = false, mensaje = "Todavía no has confirmado tu correo electrónico." });
            }
            TempData["ErrorLogin"] = "Todavía no has confirmado tu correo electrónico. Revisa tu bandeja de entrada, o solicita que te lo reenviemos.";
            return Redirect("/?abrirLogin=true");
        }

        var resultado = await _signInManager.PasswordSignInAsync(
            usuario, modelo.Password, modelo.Recordarme, lockoutOnFailure: true);

        // La contraseña es correcta, pero esta cuenta tiene activada la
        // verificación en dos pasos (hoy, solo el rol Administrador la usa)
        // — Identity NO completó el inicio de sesión todavía; le pide al
        // modal que muestre el segundo paso (código de 6 dígitos) en vez de
        // redirigir de una vez.
        if (resultado.RequiresTwoFactor)
        {
            if (EsPeticionAjax())
            {
                return Ok(new { exito = false, requiere2FA = true, mensaje = "Ingresa el código de tu aplicación de autenticación." });
            }
            return Redirect("/?abrirLogin=true&requiere2FA=true");
        }

        if (resultado.IsLockedOut)
        {
            if (await _userManager.IsInRoleAsync(usuario, RolesSistema.Administrador) || await _userManager.IsInRoleAsync(usuario, RolesSistema.SuperAdministrador))
            {
                await _auditoriaService.RegistrarAsync(usuario.Id, "CuentaBloqueada", "AspNetUsers", usuario.Id,
                    "Cuenta bloqueada temporalmente por múltiples intentos de inicio de sesión fallidos.");
            }

            if (EsPeticionAjax())
            {
                return StatusCode(423, new { exito = false, mensaje = "Cuenta bloqueada temporalmente por múltiples intentos fallidos. Intenta de nuevo en unos minutos." });
            }
            TempData["ErrorLogin"] = "Cuenta bloqueada temporalmente por múltiples intentos fallidos. Intenta de nuevo en unos minutos.";
            return Redirect("/?abrirLogin=true");
        }

        if (!resultado.Succeeded)
        {
            if (EsPeticionAjax())
            {
                return Unauthorized(new { exito = false, mensaje = "Correo o contraseña incorrectos." });
            }
            TempData["ErrorLogin"] = "Correo o contraseña incorrectos.";
            return Redirect("/?abrirLogin=true");
        }

        var urlDestino = !string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl)
            ? modelo.ReturnUrl
            : await ObtenerUrlDestinoAsync(usuario);

        if (await _userManager.IsInRoleAsync(usuario, RolesSistema.Administrador) || await _userManager.IsInRoleAsync(usuario, RolesSistema.SuperAdministrador))
        {
            await _auditoriaService.RegistrarAsync(usuario.Id, "InicioSesion", "AspNetUsers", usuario.Id, $"Inicio de sesión exitoso ({usuario.Email}).");
        }

        if (EsPeticionAjax())
        {
            return Ok(new { exito = true, redirectUrl = urlDestino });
        }

        return Redirect(urlDestino);
    }

    /// <summary>
    /// Segundo paso del login cuando la cuenta tiene 2FA activado — recibe
    /// el código de 6 dígitos de la app autenticadora (Google Authenticator,
    /// Authy, etc.) y completa el inicio de sesión que PasswordSignInAsync
    /// dejó pendiente en Login(). Debe llamarse dentro de la misma sesión de
    /// navegador donde ocurrió el primer paso (Identity guarda el estado
    /// intermedio en una cookie temporal propia).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerificarCodigo2FA(string codigo, bool recordarme = false)
    {
        var codigoLimpio = (codigo ?? string.Empty).Replace(" ", string.Empty).Trim();

        var resultado = await _signInManager.TwoFactorAuthenticatorSignInAsync(codigoLimpio, recordarme, rememberClient: false);

        if (resultado.IsLockedOut)
        {
            return StatusCode(423, new { exito = false, mensaje = "Cuenta bloqueada temporalmente por múltiples intentos fallidos." });
        }

        if (!resultado.Succeeded)
        {
            return Unauthorized(new { exito = false, mensaje = "Código incorrecto. Revisa la hora de tu teléfono e intenta de nuevo." });
        }

        var usuario = await _signInManager.UserManager.GetUserAsync(User)
            ?? throw new InvalidOperationException("No se pudo identificar al usuario tras verificar el código de dos factores.");

        return Ok(new { exito = true, redirectUrl = await ObtenerUrlDestinoAsync(usuario) });
    }

    // ---------- Autenticación en dos pasos (2FA) — configuración ----------
    // Hoy solo se ofrece/usa para el rol Administrador, dado que es la
    // cuenta con más poder sobre el sistema (aprobar pagos, gestionar
    // usuarios y empresas, ver auditoría). Nada impide que un Candidato o
    // Empresa la active también si en el futuro se decide ofrecerla a todos.

    /// <summary>
    /// Revoca todas las sesiones activas del usuario (esta y cualquier otra
    /// que tenga abierta en otro dispositivo/navegador) cambiando su
    /// "security stamp" — la cookie de las demás sesiones deja de ser
    /// válida la próxima vez que Identity la valide. Útil si sospechas que
    /// alguien más tiene acceso a tu cuenta.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarTodasLasSesiones()
    {
        var usuario = await _userManager.GetUserAsync(User)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        await _userManager.UpdateSecurityStampAsync(usuario);

        if (await _userManager.IsInRoleAsync(usuario, RolesSistema.Administrador) || await _userManager.IsInRoleAsync(usuario, RolesSistema.SuperAdministrador))
        {
            await _auditoriaService.RegistrarAsync(usuario.Id, "CerrarTodasLasSesiones", "AspNetUsers", usuario.Id,
                "El usuario revocó todas sus sesiones activas.");
        }

        // Esta misma sesión también queda invalidada — se cierra aquí mismo
        // y se manda de vuelta al login, en vez de dejarla "viva" hasta la
        // próxima validación automática del security stamp.
        await _signInManager.SignOutAsync();
        TempData["Exito"] = "Se cerraron todas tus sesiones activas. Inicia sesión de nuevo.";
        return Redirect("/?abrirLogin=true");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ConfigurarDobleFactor()
    {
        var usuario = await _userManager.GetUserAsync(User)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (await _userManager.GetTwoFactorEnabledAsync(usuario))
        {
            return View(new ConfigurarDobleFactorDto { YaActivado = true });
        }

        var claveExistente = await _userManager.GetAuthenticatorKeyAsync(usuario);
        if (string.IsNullOrEmpty(claveExistente))
        {
            await _userManager.ResetAuthenticatorKeyAsync(usuario);
            claveExistente = await _userManager.GetAuthenticatorKeyAsync(usuario);
        }

        return View(new ConfigurarDobleFactorDto
        {
            YaActivado = false,
            ClaveManual = FormatearClaveParaLectura(claveExistente!),
            UrlOtpAuth = GenerarUrlOtpAuth(usuario.Email!, claveExistente!),
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarDobleFactor(string codigo)
    {
        var usuario = await _userManager.GetUserAsync(User)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        var codigoLimpio = (codigo ?? string.Empty).Replace(" ", string.Empty).Trim();
        var esValido = await _userManager.VerifyTwoFactorTokenAsync(
            usuario, _userManager.Options.Tokens.AuthenticatorTokenProvider, codigoLimpio);

        if (!esValido)
        {
            TempData["Error"] = "El código no coincide. Verifica que tu teléfono tenga la hora correcta e intenta de nuevo.";
            return RedirectToAction(nameof(ConfigurarDobleFactor));
        }

        await _userManager.SetTwoFactorEnabledAsync(usuario, true);
        TempData["Exito"] = "Verificación en dos pasos activada. La próxima vez que inicies sesión, te pedirá el código de tu aplicación autenticadora.";
        return RedirectToAction(nameof(ConfigurarDobleFactor));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesactivarDobleFactor()
    {
        var usuario = await _userManager.GetUserAsync(User)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        await _userManager.SetTwoFactorEnabledAsync(usuario, false);
        await _userManager.ResetAuthenticatorKeyAsync(usuario);
        TempData["Exito"] = "Verificación en dos pasos desactivada.";
        return RedirectToAction(nameof(ConfigurarDobleFactor));
    }

    /// <summary>Inserta espacios cada 4 caracteres — mismo formato que muestran Google/Microsoft Authenticator al pedir la clave manual.</summary>
    private static string FormatearClaveParaLectura(string clave)
    {
        var trozos = new List<string>();
        for (var i = 0; i < clave.Length; i += 4)
        {
            trozos.Add(clave.Substring(i, Math.Min(4, clave.Length - i)));
        }
        return string.Join(" ", trozos);
    }

    /// <summary>URI estándar "otpauth://" que cualquier app autenticadora (Google/Microsoft Authenticator, Authy) sabe leer desde un código QR.</summary>
    private static string GenerarUrlOtpAuth(string correo, string clave)
    {
        const string emisor = "ChambeaJobs";
        return $"otpauth://totp/{Uri.EscapeDataString(emisor)}:{Uri.EscapeDataString(correo)}" +
               $"?secret={clave}&issuer={Uri.EscapeDataString(emisor)}&digits=6";
    }

    /// <summary>
    /// Redirige a un usuario ya autenticado a su panel correspondiente según
    /// su rol. Se usa tanto tras un login exitoso como cuando un usuario ya
    /// logueado llega de nuevo a la pantalla de Login (ej. con el botón
    /// "atrás" del navegador).
    /// </summary>
    private async Task<IActionResult> RedirigirSegunRolAsync()
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario is null)
        {
            return RedirectToAction("Index", "Home");
        }

        return Redirect(await ObtenerUrlDestinoAsync(usuario));
    }

    // ---------- Logout ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ---------- Acceso denegado ----------

    [HttpGet]
    public IActionResult AccessoDenegado() => View();

    // ---------- Continuar con Google (solo Candidatos) ----------

    [HttpGet]
    public IActionResult LoginGoogle(string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "Account", new { returnUrl });
        var propiedades = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        return Challenge(propiedades, "Google");
    }

    [HttpGet]
    public async Task<IActionResult> GoogleCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError is not null)
        {
            TempData["Error"] = "Ocurrió un problema al continuar con Google. Intenta de nuevo.";
            return RedirectToAction(nameof(Login));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            TempData["Error"] = "No se pudo completar el inicio de sesión con Google.";
            return RedirectToAction(nameof(Login));
        }

        // Caso 1: esta cuenta de Google ya estaba vinculada antes — entra directo.
        var resultado = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (resultado.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return await RedirigirSegunRolAsync();
        }

        if (resultado.IsLockedOut)
        {
            TempData["Error"] = "Esta cuenta está bloqueada temporalmente por múltiples intentos fallidos.";
            return RedirectToAction(nameof(Login));
        }

        var correo = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(correo))
        {
            TempData["Error"] = "Tu cuenta de Google no tiene un correo disponible para continuar.";
            return RedirectToAction(nameof(Login));
        }

        var usuarioExistente = await _userManager.FindByEmailAsync(correo);

        if (usuarioExistente is not null)
        {
            // "Continuar con Google" es exclusivo de Candidatos — si el correo
            // ya pertenece a una cuenta de Empresa/Admin, no se vincula así.
            if (!await _userManager.IsInRoleAsync(usuarioExistente, RolesSistema.Candidato))
            {
                TempData["Error"] = "Ya existe una cuenta con este correo que no es de Candidato. Inicia sesión con tu contraseña.";
                return RedirectToAction(nameof(Login));
            }

            if (!usuarioExistente.EstadoCuenta)
            {
                TempData["Error"] = "Esta cuenta se encuentra suspendida. Contacta a soporte.";
                return RedirectToAction(nameof(Login));
            }

            // Primera vez que usa Google con una cuenta que ya tenía contraseña: se vincula para la próxima vez.
            var resultadoVinculo = await _userManager.AddLoginAsync(usuarioExistente, info);
            if (!resultadoVinculo.Succeeded)
            {
                TempData["Error"] = "No se pudo vincular tu cuenta de Google a tu cuenta existente.";
                return RedirectToAction(nameof(Login));
            }

            // Google ya verificó este correo — si no estaba confirmado, se confirma automáticamente.
            if (!usuarioExistente.EmailConfirmed)
            {
                usuarioExistente.EmailConfirmed = true;
                await _userManager.UpdateAsync(usuarioExistente);
            }

            await _signInManager.SignInAsync(usuarioExistente, isPersistent: false);
            return await RedirigirSegunRolAsync();
        }

        // Caso 3: cuenta totalmente nueva — se crea automáticamente como Candidato.
        var nombreCompleto = info.Principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var nombres = info.Principal.FindFirstValue(ClaimTypes.GivenName);
        var apellidos = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nombres))
        {
            // Algunas cuentas de Google no exponen nombre/apellido por separado; se usa el nombre completo.
            nombres = nombreCompleto;
        }

        var nuevoUsuario = new ApplicationUser
        {
            UserName = correo,
            Email = correo,
            EstadoCuenta = true,
            EmailConfirmed = true, // Google ya verificó este correo, no hace falta nuestro propio correo de confirmación
            FechaRegistro = DateTime.UtcNow
        };

        var resultadoCreacion = await _userManager.CreateAsync(nuevoUsuario);
        if (!resultadoCreacion.Succeeded)
        {
            TempData["Error"] = "No se pudo crear tu cuenta: " + string.Join(" ", resultadoCreacion.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Login));
        }

        await _userManager.AddToRoleAsync(nuevoUsuario, RolesSistema.Candidato);
        await _userManager.AddLoginAsync(nuevoUsuario, info);
        await _candidatoService.CrearPerfilInicialAsync(nuevoUsuario.Id, nombres, apellidos);

        await _signInManager.SignInAsync(nuevoUsuario, isPersistent: false);

        TempData["Exito"] = "¡Bienvenido a ChambeaJobs! Completa tu número de teléfono y el resto de tu perfil para empezar a postularte.";
        return RedirectToAction("EditarPerfil", "Candidato");
    }

    // ---------- Confirmación de correo ----------

    [HttpGet]
    public IActionResult RevisaTuCorreo(string correo)
    {
        ViewBag.Correo = correo;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmarCorreo(string usuarioId, string token)
    {
        if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(token))
        {
            TempData["Error"] = "El enlace de confirmación no es válido.";
            return RedirectToAction(nameof(Login));
        }

        var usuario = await _userManager.FindByIdAsync(usuarioId);
        if (usuario is null)
        {
            TempData["Error"] = "El enlace de confirmación no es válido.";
            return RedirectToAction(nameof(Login));
        }

        if (usuario.EmailConfirmed)
        {
            // Ya estaba confirmado (ej. el usuario le dio clic al enlace dos veces) — no es un error.
            return View("ConfirmarCorreoExitoso");
        }

        var resultado = await _userManager.ConfirmEmailAsync(usuario, token);

        if (!resultado.Succeeded)
        {
            TempData["Error"] = "El enlace de confirmación no es válido o ya expiró. Solicita que te reenviemos uno nuevo.";
            return RedirectToAction(nameof(Login));
        }

        return View("ConfirmarCorreoExitoso");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReenviarConfirmacion(string correo)
    {
        var usuario = await _userManager.FindByEmailAsync(correo);

        // Por seguridad, siempre mostramos el mismo mensaje exista o no la
        // cuenta — así no revelamos si un correo está registrado.
        if (usuario is not null && !usuario.EmailConfirmed)
        {
            var nombreParaSaludo = await _userManager.IsInRoleAsync(usuario, RolesSistema.Empresa)
                ? (await _empresaService.ObtenerPerfilPorUsuarioIdAsync(usuario.Id))?.NombreEmpresa ?? "ChambeaJobs"
                : (await _candidatoService.ObtenerPerfilPorUsuarioIdAsync(usuario.Id))?.Nombres ?? "ChambeaJobs";

            await EnviarCorreoConfirmacionAsync(usuario, nombreParaSaludo);
        }

        return RedirectToAction(nameof(RevisaTuCorreo), new { correo });
    }

    // ---------- Recuperar contraseña ----------

    [HttpGet]
    public IActionResult RecuperarPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecuperarPassword(RecuperarPasswordDto modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var usuario = await _userManager.FindByEmailAsync(modelo.Correo);

        // Por seguridad, no revelamos si el correo existe o no en el sistema:
        // siempre mostramos la misma pantalla de confirmación.
        if (usuario is null)
        {
            return RedirectToAction(nameof(RecuperarPasswordConfirmacion));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

        var enlaceRestablecer = Url.Action(
            nameof(RestablecerPassword), "Account",
            new { usuarioId = usuario.Id, token },
            protocol: Request.Scheme);

        // NOTA IMPORTANTE: este proyecto no tiene un servidor de correo (SMTP)
        // configurado. En un entorno de producción real, aquí se enviaría
        // 'enlaceRestablecer' por correo electrónico al usuario (por ejemplo,
        // con SendGrid o un servidor SMTP institucional) en vez de mostrarlo
        // en pantalla. Se muestra aquí únicamente para fines de desarrollo/demo.
        ViewBag.EnlaceRestablecer = enlaceRestablecer;

        return View(nameof(RecuperarPasswordConfirmacion));
    }

    [HttpGet]
    public IActionResult RecuperarPasswordConfirmacion() => View();

    // ---------- Restablecer contraseña ----------

    [HttpGet]
    public IActionResult RestablecerPassword(string usuarioId, string token)
    {
        if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(token))
        {
            TempData["Error"] = "El enlace de recuperación no es válido.";
            return RedirectToAction(nameof(Login));
        }

        return View(new RestablecerPasswordDto { UsuarioId = usuarioId, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestablecerPassword(RestablecerPasswordDto modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var usuario = await _userManager.FindByIdAsync(modelo.UsuarioId);
        if (usuario is null)
        {
            // Mensaje genérico, sin confirmar si el usuario existe (seguridad).
            return RedirectToAction(nameof(RestablecerPasswordConfirmacion));
        }

        var resultado = await _userManager.ResetPasswordAsync(usuario, modelo.Token, modelo.Password);

        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(modelo);
        }

        return RedirectToAction(nameof(RestablecerPasswordConfirmacion));
    }

    [HttpGet]
    public IActionResult RestablecerPasswordConfirmacion() => View();
}
