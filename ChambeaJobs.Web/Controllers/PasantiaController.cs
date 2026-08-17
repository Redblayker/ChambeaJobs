using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// Módulo de Pasantías, independiente del módulo de Vacantes de empleo.
/// Cubre Buscar y Detalle (públicas), Aplicar y Mis Postulaciones (Candidato).
/// </summary>
public class PasantiaController : Controller
{
    private readonly IPasantiaService _pasantiaService;
    private readonly ICatalogoService _catalogoService;
    private readonly ICandidatoService _candidatoService;

    public PasantiaController(
        IPasantiaService pasantiaService,
        ICatalogoService catalogoService,
        ICandidatoService candidatoService)
    {
        _pasantiaService = pasantiaService;
        _catalogoService = catalogoService;
        _candidatoService = candidatoService;
    }

    private string? UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // ---------- Buscar pasantías (público) ----------

    [HttpGet]
    public async Task<IActionResult> Buscar(string? q, int? categoriaId, int? ubicacionId, string? modalidad)
    {
        var resultados = await _pasantiaService.BuscarAsync(q, categoriaId, ubicacionId, modalidad);

        ViewBag.Categorias = await _catalogoService.ObtenerCategoriasAsync();
        ViewBag.Ubicaciones = await _catalogoService.ObtenerUbicacionesAsync();
        ViewBag.PalabraClave = q;
        ViewBag.CategoriaId = categoriaId;
        ViewBag.UbicacionId = ubicacionId;
        ViewBag.Modalidad = modalidad;

        return View(resultados);
    }

    // ---------- Detalle de pasantía (público) ----------

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var pasantia = await _pasantiaService.ObtenerDetalleAsync(id);
        if (pasantia is null)
        {
            return RedirectToAction("Error404", "Home");
        }

        var esCandidatoAutenticado = User.IsInRole(RolesSistema.Candidato);
        ViewBag.EsCandidato = esCandidatoAutenticado;

        if (esCandidatoAutenticado && UsuarioActualId is not null)
        {
            var candidatoId = await ObtenerCandidatoIdAsync();
            if (candidatoId.HasValue)
            {
                ViewBag.YaPostulo = await _pasantiaService.YaPostuloAsync(candidatoId.Value, id);
            }
        }

        return View(pasantia);
    }

    // ---------- Aplicar (solo Candidato) ----------

    [HttpPost]
    [Authorize(Roles = RolesSistema.Candidato)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aplicar(int pasantiaId)
    {
        var candidatoId = await ObtenerCandidatoIdAsync()
            ?? throw new InvalidOperationException("No se encontró tu perfil de candidato.");

        try
        {
            await _pasantiaService.PostularAsync(candidatoId, pasantiaId);
            TempData["Exito"] = "¡Postulación a la pasantía enviada!";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Detalle), new { id = pasantiaId });
    }

    // ---------- Mis postulaciones a pasantías (solo Candidato) ----------

    [HttpGet]
    [Authorize(Roles = RolesSistema.Candidato)]
    public async Task<IActionResult> MisPostulaciones()
    {
        var candidatoId = await ObtenerCandidatoIdAsync()
            ?? throw new InvalidOperationException("No se encontró tu perfil de candidato.");

        var postulaciones = await _pasantiaService.ObtenerMisPostulacionesAsync(candidatoId);
        return View(postulaciones);
    }

    private async Task<int?> ObtenerCandidatoIdAsync()
    {
        if (UsuarioActualId is null) return null;
        var perfil = await _candidatoService.ObtenerPerfilPorUsuarioIdAsync(UsuarioActualId);
        return perfil?.Id;
    }
}
