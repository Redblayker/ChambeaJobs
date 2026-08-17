namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Envío de correos reales (confirmación de cuenta, y en el futuro
/// cualquier otra notificación por email). La implementación real vive
/// en Infrastructure porque depende de configuración de servidor SMTP.
/// </summary>
public interface IEmailSender
{
    Task EnviarAsync(string correoDestino, string asunto, string cuerpoHtml);
}
