using ChambeaJobs.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChambeaJobs.Web.ViewComponents;

/// <summary>
/// Campanita 🔔 del layout: muestra el conteo de notificaciones no leídas
/// y las últimas notificaciones del usuario autenticado, sin importar su
/// rol (Candidato, Empresa o Administrador).
/// </summary>
public class NotificacionesBellViewComponent : ViewComponent
{
    private readonly INotificacionService _notificacionService;

    public NotificacionesBellViewComponent(INotificacionService notificacionService)
    {
        _notificacionService = notificacionService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var usuarioId = HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            return Content(string.Empty); // usuario no autenticado: no se muestra la campanita
        }

        var recientes = await _notificacionService.ObtenerRecientesAsync(usuarioId, 8);
        var noLeidas = await _notificacionService.ContarNoLeidasAsync(usuarioId);

        ViewBag.NoLeidas = noLeidas;
        return View(recientes);
    }
}
