using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// Widget de chatbot disponible en cualquier página, para candidatos,
/// empresas y visitantes sin cuenta. Sin [Authorize] a propósito.
/// </summary>
public class ChatbotController : Controller
{
    private readonly IChatService _chatService;

    public ChatbotController(IChatService chatService)
    {
        _chatService = chatService;
    }

    private string? UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private string RolActual
    {
        get
        {
            if (User.IsInRole("Empresa")) return "Empresa";
            if (User.IsInRole("Candidato")) return "Candidato";
            return "Visitante";
        }
    }

    [HttpPost]
    public async Task<IActionResult> Iniciar(int? conversacionId)
    {
        var conversacion = await _chatService.ObtenerOCrearConversacionAsync(conversacionId, UsuarioActualId, RolActual);
        return Json(conversacion);
    }

    [HttpPost]
    public async Task<IActionResult> EnviarMensaje([FromBody] ChatEnviarMensajeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Texto)) return BadRequest();

        var nombreContacto = User.Identity?.Name ?? "Visitante del chat";
        var correoContacto = User.FindFirstValue(ClaimTypes.Email) ?? "sin-correo@chambeajobs.com";

        var conversacion = await _chatService.EnviarMensajeAsync(dto, UsuarioActualId, RolActual, nombreContacto, correoContacto);
        return Json(conversacion);
    }

    [HttpPost]
    public async Task<IActionResult> Escalar(int conversacionId, string? nombre, string? correo)
    {
        var nombreContacto = string.IsNullOrWhiteSpace(nombre) ? (User.Identity?.Name ?? "Visitante del chat") : nombre;
        var correoContacto = string.IsNullOrWhiteSpace(correo) ? (User.FindFirstValue(ClaimTypes.Email) ?? "sin-correo@chambeajobs.com") : correo;

        try
        {
            var ticketId = await _chatService.EscalarASoporteAsync(conversacionId, nombreContacto, correoContacto);
            return Json(new { exito = true, ticketId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
