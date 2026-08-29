using ChambeaJobs.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChambeaJobs.Web.Extensions;
using System.Security.Claims;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// Centro de notificaciones 🔔, disponible para cualquier usuario autenticado
/// (Candidato, Empresa o Administrador).
/// </summary>
[Authorize]
public class NotificacionesController : Controller
{
    private readonly INotificacionService _notificacionService;

    public NotificacionesController(INotificacionService notificacionService)
    {
        _notificacionService = notificacionService;
    }

    private string UsuarioActualId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("No se pudo determinar el usuario autenticado.");

    [HttpGet]
    public async Task<IActionResult> Index(int pagina = 1)
    {
        var notificaciones = await _notificacionService.ObtenerRecientesAsync(UsuarioActualId, 100);
        return View(this.Paginar(notificaciones, pagina));
    }

    /// <summary>Marca la notificación como leída y redirige a su destino (o al listado si no tiene uno).</summary>
    [HttpGet]
    public async Task<IActionResult> Ir(int id)
    {
        var notificaciones = await _notificacionService.ObtenerRecientesAsync(UsuarioActualId, 100);
        var notificacion = notificaciones.FirstOrDefault(n => n.Id == id);

        await _notificacionService.MarcarComoLeidaAsync(id, UsuarioActualId);

        if (notificacion?.UrlDestino is not null)
        {
            return LocalRedirect(notificacion.UrlDestino);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarTodasLeidas(string? returnUrl)
    {
        await _notificacionService.MarcarTodasComoLeidasAsync(UsuarioActualId);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _notificacionService.EliminarAsync(id, UsuarioActualId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarTodas()
    {
        await _notificacionService.EliminarTodasAsync(UsuarioActualId);
        return RedirectToAction(nameof(Index));
    }
}
