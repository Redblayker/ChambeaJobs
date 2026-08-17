using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// Centro de Soporte con chat en tiempo real. A pedido, este canal está
/// disponible únicamente para Empresas (el Candidato no tiene acceso a
/// este módulo). Los tickets creados aquí aparecen en el panel de
/// Administrador → Soporte, y la conversación se ve en tiempo real de
/// ambos lados vía SoporteChatHub.
/// </summary>
[Authorize(Roles = ChambeaJobs.Domain.Enums.RolesSistema.Empresa + "," + ChambeaJobs.Domain.Enums.RolesSistema.Candidato)]
public class SoporteController : Controller
{
    private readonly ISoporteService _soporteService;

    public SoporteController(ISoporteService soporteService)
    {
        _soporteService = soporteService;
    }

    private string? UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private string? CorreoActual => User.FindFirstValue(ClaimTypes.Email);

    [HttpGet]
    public async Task<IActionResult> Contacto()
    {
        // Si ya tienes una conversación abierta, te lleva directo ahí en vez
        // de mostrarte el formulario de nuevo (evita tickets duplicados).
        var ticketAbierto = await _soporteService.ObtenerMiTicketAbiertoAsync(UsuarioActualId, CorreoActual ?? string.Empty);
        if (ticketAbierto is not null)
        {
            return RedirectToAction(nameof(Chat), new { id = ticketAbierto.Id });
        }

        var modelo = new CrearTicketSoporteDto
        {
            CorreoContacto = CorreoActual ?? string.Empty
        };
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contacto(CrearTicketSoporteDto modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var ticketId = await _soporteService.CrearTicketAsync(UsuarioActualId, modelo);
        return RedirectToAction(nameof(Chat), new { id = ticketId });
    }

    // ---------- Chat ----------

    [HttpGet]
    public async Task<IActionResult> Chat(int id)
    {
        var ticket = await _soporteService.ObtenerConMensajesAsync(id);
        if (ticket is null)
        {
            return RedirectToAction("Error404", "Home");
        }

        // Verificación de propiedad: una empresa no puede entrar al chat de otra
        // adivinando el id en la URL, salvo que sea justo la dueña del ticket.
        if (!string.Equals(ticket.CorreoContacto, CorreoActual, StringComparison.OrdinalIgnoreCase))
        {
            var ticketPropio = await _soporteService.ObtenerMiTicketAbiertoAsync(UsuarioActualId, CorreoActual ?? string.Empty);
            if (ticketPropio is null || ticketPropio.Id != id)
            {
                return Forbid();
            }
        }

        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarMensaje(int ticketId, string mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
        {
            return BadRequest();
        }

        var nombre = User.FindFirstValue(ClaimTypes.Email) ?? "Empresa";
        var enviado = await _soporteService.EnviarMensajeAsync(ticketId, UsuarioActualId, nombre, esAdmin: false, mensaje);
        return Json(enviado);
    }
}
