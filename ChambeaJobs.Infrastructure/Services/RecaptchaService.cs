using System.Text.Json;
using ChambeaJobs.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChambeaJobs.Infrastructure.Services;

/// <inheritdoc cref="ICaptchaService"/>
public class RecaptchaService : ICaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RecaptchaService> _logger;
    private readonly string _siteKey;
    private readonly string _secretKey;

    public RecaptchaService(HttpClient httpClient, IConfiguration configuracion, ILogger<RecaptchaService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _siteKey = configuracion["Recaptcha:SiteKey"] ?? string.Empty;
        _secretKey = configuracion["Recaptcha:SecretKey"] ?? string.Empty;
    }

    public bool CaptchaDisponible => !string.IsNullOrWhiteSpace(_siteKey) && !string.IsNullOrWhiteSpace(_secretKey);
    public string SiteKeyPublica => _siteKey;

    public async Task<bool> VerificarAsync(string? token)
    {
        if (!CaptchaDisponible)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var contenido = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _secretKey),
                new KeyValuePair<string, string>("response", token),
            });

            var respuesta = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", contenido);
            var json = await respuesta.Content.ReadAsStringAsync();
            using var documento = JsonDocument.Parse(json);

            return documento.RootElement.TryGetProperty("success", out var exito) && exito.GetBoolean();
        }
        catch (Exception ex)
        {
            // Si Google no responde (caída, red), no dejamos a todo el
            // sitio sin poder loguearse — se registra el error y se deja
            // pasar, priorizando disponibilidad sobre esta capa extra.
            _logger.LogWarning(ex, "No se pudo verificar el CAPTCHA con Google reCAPTCHA.");
            return true;
        }
    }
}
