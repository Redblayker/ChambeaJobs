using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// Flujo del Candidato para la evaluación psicométrica Big Five enviada
/// por una empresa: pantalla de invitación, la prueba en sí (Likert 1-5)
/// y sus resultados. Inspirado en el prototipo HTML/CSS/JS de referencia,
/// adaptado a Razor + Service Layer (sin SPA aparte).
/// </summary>
[Authorize(Roles = RolesSistema.Candidato)]
public class EvaluacionController : Controller
{
    private readonly IEvaluacionService _evaluacionService;
    private readonly ICandidatoService _candidatoService;

    public EvaluacionController(IEvaluacionService evaluacionService, ICandidatoService candidatoService)
    {
        _evaluacionService = evaluacionService;
        _candidatoService = candidatoService;
    }

    private string UsuarioActualId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("No se pudo determinar el usuario autenticado.");

    private async Task<int> ObtenerCandidatoIdAsync()
    {
        var perfil = await _candidatoService.ObtenerPerfilPorUsuarioIdAsync(UsuarioActualId);
        return perfil?.Id ?? throw new InvalidOperationException("No se encontró tu perfil de candidato.");
    }

    // ---------- Invitación ----------

    [HttpGet]
    public async Task<IActionResult> Invitacion(int id)
    {
        var candidatoId = await ObtenerCandidatoIdAsync();
        var invitacion = await _evaluacionService.ObtenerInvitacionAsync(id, candidatoId);

        if (invitacion is null)
        {
            TempData["Error"] = "Esta evaluación no existe o no te pertenece.";
            return RedirectToAction("MisPostulaciones", "Candidato");
        }

        return View(invitacion);
    }

    // ---------- Realizar la prueba ----------

    [HttpGet]
    public async Task<IActionResult> Realizar(int id)
    {
        var candidatoId = await ObtenerCandidatoIdAsync();
        var formulario = await _evaluacionService.ObtenerFormularioAsync(id, candidatoId);

        if (formulario is null)
        {
            TempData["Error"] = "Esta evaluación no existe o no te pertenece.";
            return RedirectToAction("MisPostulaciones", "Candidato");
        }

        return View(formulario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarProgreso(int evaluacionId, [FromForm] Dictionary<int, int> respuestas)
    {
        var candidatoId = await ObtenerCandidatoIdAsync();

        try
        {
            await _evaluacionService.GuardarRespuestasParcialesAsync(evaluacionId, candidatoId, respuestas);
            return Json(new { exito = true });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar(int evaluacionId, [FromForm] Dictionary<int, int> respuestas)
    {
        var candidatoId = await ObtenerCandidatoIdAsync();

        try
        {
            await _evaluacionService.FinalizarEvaluacionAsync(evaluacionId, candidatoId, respuestas);
            TempData["Exito"] = "¡Evaluación completada! Aquí tienes tu resultado.";
            return RedirectToAction(nameof(Resultados), new { id = evaluacionId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Realizar), new { id = evaluacionId });
        }
    }

    // ---------- Resultados ----------

    [HttpGet]
    public async Task<IActionResult> Resultados(int id)
    {
        var candidatoId = await ObtenerCandidatoIdAsync();

        // Verificamos pertenencia antes de mostrar resultados (reutiliza la validación del formulario).
        var formulario = await _evaluacionService.ObtenerFormularioAsync(id, candidatoId);
        if (formulario is null)
        {
            TempData["Error"] = "Esta evaluación no existe o no te pertenece.";
            return RedirectToAction("MisPostulaciones", "Candidato");
        }

        var resultado = await _evaluacionService.ObtenerResultadoAsync(id);
        if (resultado is null)
        {
            TempData["Error"] = "Todavía no has completado esta evaluación.";
            return RedirectToAction(nameof(Realizar), new { id });
        }

        return View(resultado);
    }
}
