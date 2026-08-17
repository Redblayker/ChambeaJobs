using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChambeaJobs.Infrastructure.Services;

/// <inheritdoc cref="IPayPalPagoService"/>
public class PayPalPagoService : IPayPalPagoService
{
    private readonly HttpClient _http;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificacionService _notificacionService;
    private readonly ILogger<PayPalPagoService> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _baseUrl;

    public PayPalPagoService(
        HttpClient http,
        IConfiguration configuracion,
        IUnitOfWork unitOfWork,
        INotificacionService notificacionService,
        ILogger<PayPalPagoService> logger)
    {
        _http = http;
        _unitOfWork = unitOfWork;
        _notificacionService = notificacionService;
        _logger = logger;

        _clientId = configuracion["PayPal:ClientId"] ?? string.Empty;
        _clientSecret = configuracion["PayPal:Secret"] ?? string.Empty;

        // "Sandbox" mientras se prueba, "live" cuando ya se use con dinero real.
        var modo = configuracion["PayPal:Modo"] ?? "sandbox";
        _baseUrl = modo == "live" ? "https://api-m.paypal.com" : "https://api-m.sandbox.paypal.com";
    }

    public bool PayPalDisponible => !string.IsNullOrWhiteSpace(_clientId) && !string.IsNullOrWhiteSpace(_clientSecret);

    public string ClientIdPublico => _clientId;

    public async Task<string> CrearOrdenAsync(int empresaId, int planSuscripcionId)
    {
        if (!PayPalDisponible)
        {
            throw new InvalidOperationException("El pago con PayPal no está configurado todavía.");
        }

        var plan = await _unitOfWork.PlanesSuscripcion.ObtenerPorIdAsync(planSuscripcionId)
            ?? throw new InvalidOperationException("El plan seleccionado no es válido.");

        var token = await ObtenerTokenAccesoAsync();

        var cuerpoOrden = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    description = $"ChambeaJobs — Plan {plan.Nombre} (empresa #{empresaId})",
                    amount = new
                    {
                        currency_code = "USD",
                        value = plan.Precio.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
            }
        };

        using var solicitud = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v2/checkout/orders");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        solicitud.Content = new StringContent(JsonSerializer.Serialize(cuerpoOrden), Encoding.UTF8, "application/json");

        var respuesta = await _http.SendAsync(solicitud);
        var contenido = await respuesta.Content.ReadAsStringAsync();

        if (!respuesta.IsSuccessStatusCode)
        {
            _logger.LogError("Error creando orden en PayPal: {Contenido}", contenido);
            throw new InvalidOperationException("No se pudo crear la orden de pago en PayPal. Intenta de nuevo.");
        }

        using var documento = JsonDocument.Parse(contenido);
        return documento.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("PayPal no devolvió un ID de orden válido.");
    }

    public async Task CapturarOrdenAsync(int empresaId, string ordenPayPalId)
    {
        if (!PayPalDisponible)
        {
            throw new InvalidOperationException("El pago con PayPal no está configurado todavía.");
        }

        var token = await ObtenerTokenAccesoAsync();

        using var solicitud = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v2/checkout/orders/{ordenPayPalId}/capture");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        solicitud.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        var respuesta = await _http.SendAsync(solicitud);
        var contenido = await respuesta.Content.ReadAsStringAsync();

        if (!respuesta.IsSuccessStatusCode)
        {
            _logger.LogError("Error capturando orden de PayPal {OrdenId}: {Contenido}", ordenPayPalId, contenido);
            throw new InvalidOperationException("PayPal no pudo confirmar este pago. No se activó ningún plan.");
        }

        using var documento = JsonDocument.Parse(contenido);
        var estado = documento.RootElement.GetProperty("status").GetString();

        // Nunca se confía en lo que diga el navegador — solo se activa el
        // plan si PayPal mismo confirma que el estado es COMPLETED.
        if (estado != "COMPLETED")
        {
            throw new InvalidOperationException($"El pago no se completó (estado de PayPal: {estado}).");
        }

        var montoCapturado = documento.RootElement
            .GetProperty("purchase_units")[0]
            .GetProperty("payments")
            .GetProperty("captures")[0]
            .GetProperty("amount")
            .GetProperty("value")
            .GetString();

        await ActivarPlanTrasPagoPayPalAsync(empresaId, ordenPayPalId, decimal.Parse(montoCapturado!, System.Globalization.CultureInfo.InvariantCulture));
    }

    private async Task ActivarPlanTrasPagoPayPalAsync(int empresaId, string ordenPayPalId, decimal montoPagado)
    {
        // Se identifica el plan por el monto pagado (ya validado por PayPal),
        // ya que la orden original se creó a partir de un plan específico.
        var planes = await _unitOfWork.PlanesSuscripcion.ObtenerTodosAsync();
        var plan = planes.FirstOrDefault(p => p.Precio == montoPagado)
            ?? throw new InvalidOperationException("No se encontró un plan que coincida con el monto pagado.");

        var nuevoPaquete = new PaqueteEmpresa
        {
            EmpresaId = empresaId,
            PlanSuscripcionId = plan.Id,
            FechaCompra = DateTime.UtcNow,
            FechaVencimiento = DateTime.UtcNow.AddDays(plan.DiasVigencia),
            VacantesIncluidas = plan.VacantesIncluidas,
            VacantesConsumidas = 0,
            Estado = EstadosPaquete.Vigente, // activo de inmediato, sin esperar revisión de un Admin
            RenovacionAutomatica = true
        };

        await _unitOfWork.PaquetesEmpresa.AgregarAsync(nuevoPaquete);
        await _unitOfWork.GuardarCambiosAsync();

        var pago = new Pago
        {
            PaqueteEmpresaId = nuevoPaquete.Id,
            Monto = montoPagado,
            MetodoPago = "PayPal",
            ReferenciaTransaccion = ordenPayPalId,
            EstadoPago = EstadosPago.Aprobado,
            FechaPago = DateTime.UtcNow,
            FechaRevision = DateTime.UtcNow,
            ComentarioRevision = "Aprobado automáticamente por confirmación de PayPal.",
            IdOrdenPayPal = ordenPayPalId
        };

        await _unitOfWork.Pagos.AgregarAsync(pago);
        await _unitOfWork.GuardarCambiosAsync();

        var empresa = await _unitOfWork.Empresas.ObtenerPorIdAsync(empresaId);
        if (!string.IsNullOrWhiteSpace(empresa?.UsuarioId))
        {
            await _notificacionService.CrearAsync(
                empresa!.UsuarioId,
                Notificacion.Tipos.PagoAprobado,
                $"✅ Tu pago con PayPal fue confirmado. Tu plan {plan.Nombre} ya está activo y vence el {nuevoPaquete.FechaVencimiento:dd/MM/yyyy}.",
                "/Empresa/HistorialPagos");
        }
    }

    private async Task<string> ObtenerTokenAccesoAsync()
    {
        using var solicitud = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/oauth2/token");
        var credenciales = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Basic", credenciales);
        solicitud.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

        var respuesta = await _http.SendAsync(solicitud);
        var contenido = await respuesta.Content.ReadAsStringAsync();

        if (!respuesta.IsSuccessStatusCode)
        {
            _logger.LogError("Error obteniendo token de acceso de PayPal: {Contenido}", contenido);
            throw new InvalidOperationException("No se pudo conectar con PayPal en este momento.");
        }

        using var documento = JsonDocument.Parse(contenido);
        return documento.RootElement.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("PayPal no devolvió un token de acceso válido.");
    }
}
