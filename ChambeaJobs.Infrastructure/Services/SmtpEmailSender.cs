using ChambeaJobs.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace ChambeaJobs.Infrastructure.Services;

/// <summary>
/// Envía correos usando SMTP configurado en appsettings.json (sección "Smtp").
///
/// Mientras el proyecto corre en local (o si "Smtp:Host" está vacío), este
/// servicio no falla: simplemente deja constancia en el log de que el
/// correo no se envió, para no bloquear el registro/login de nadie.
///
/// Cuando se despliegue a un hosting real, basta con completar los valores
/// de "Smtp" (Host, Port, EnableSsl, Username, Password) según el proveedor
/// de correo que se use (el propio hosting, SendGrid, Amazon SES, Gmail
/// SMTP, etc.) — no hace falta tocar código, cada proveedor tiene su propia
/// forma de exponer estos datos en su panel de administración.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuracion;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuracion, ILogger<SmtpEmailSender> logger)
    {
        _configuracion = configuracion;
        _logger = logger;
    }

    public async Task EnviarAsync(string correoDestino, string asunto, string cuerpoHtml)
    {
        var host = _configuracion["Smtp:Host"];
        var puerto = int.Parse(_configuracion["Smtp:Port"] ?? "25");
        var usarSsl = bool.Parse(_configuracion["Smtp:EnableSsl"] ?? "false");
        var usuario = _configuracion["Smtp:Username"];
        var password = _configuracion["Smtp:Password"];
        var remitenteCorreo = _configuracion["Smtp:RemitenteCorreo"] ?? "no-responder@chambeajobs.com";
        var remitenteNombre = _configuracion["Smtp:RemitenteNombre"] ?? "ChambeaJobs";

        if (string.IsNullOrWhiteSpace(host))
        {
            // Sin configuración de SMTP (ej. en desarrollo local): no truena la
            // app, solo deja constancia en el log de que el correo no se envió.
            _logger.LogWarning(
                "No hay servidor SMTP configurado (Smtp:Host vacío). Correo NO enviado a {Correo}. Asunto: {Asunto}",
                correoDestino, asunto);
            return;
        }

        using var mensaje = new MailMessage
        {
            From = new MailAddress(remitenteCorreo, remitenteNombre),
            Subject = asunto,
            Body = cuerpoHtml,
            IsBodyHtml = true
        };
        mensaje.To.Add(correoDestino);

        using var cliente = new SmtpClient(host, puerto)
        {
            EnableSsl = usarSsl
        };

        // Algunos proveedores de SMTP local/gratuito no piden usuario ni
        // contraseña — si los dejas vacíos en appsettings, se conecta sin
        // credenciales. Si tu proveedor sí las requiere, complétalas.
        if (!string.IsNullOrWhiteSpace(usuario))
        {
            cliente.Credentials = new NetworkCredential(usuario, password);
        }

        try
        {
            await cliente.SendMailAsync(mensaje);
        }
        catch (Exception ex)
        {
            // No dejamos que un fallo de correo tumbe el registro/login del
            // usuario — solo lo registramos para poder investigarlo después.
            _logger.LogError(ex, "Error enviando correo a {Correo}", correoDestino);
        }
    }
}
