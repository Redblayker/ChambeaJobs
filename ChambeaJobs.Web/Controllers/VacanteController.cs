using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// Cubre las pantallas de Buscar Vacantes y Detalle
/// Vacante (públicas), más la acción de Aplicar que requiere sesión de Candidato.
/// </summary>
public class VacanteController : Controller
{
    private readonly IVacanteService _vacanteService;
    private readonly IPostulacionService _postulacionService;
    private readonly ICatalogoService _catalogoService;
    private readonly ICandidatoService _candidatoService;
    private readonly IEmpresaService _empresaService;

    public VacanteController(
        IVacanteService vacanteService,
        IPostulacionService postulacionService,
        ICatalogoService catalogoService,
        ICandidatoService candidatoService,
        IEmpresaService empresaService)
    {
        _vacanteService = vacanteService;
        _postulacionService = postulacionService;
        _catalogoService = catalogoService;
        _candidatoService = candidatoService;
        _empresaService = empresaService;
    }

    private string? UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // ---------- Buscar vacantes (público) ----------

    [HttpGet]
    public async Task<IActionResult> Buscar(string? q, int? categoriaId, int? ubicacionId, string? modalidad, int pagina = 1)
    {
        var resultados = await _vacanteService.BuscarAsync(q, categoriaId, ubicacionId, modalidad);

        ViewBag.Categorias = await _catalogoService.ObtenerCategoriasAsync();
        ViewBag.Ubicaciones = await _catalogoService.ObtenerUbicacionesAsync();
        ViewBag.PalabraClave = q;
        ViewBag.CategoriaId = categoriaId;
        ViewBag.UbicacionId = ubicacionId;
        ViewBag.Modalidad = modalidad;

        return View(this.Paginar(resultados, pagina));
    }

    // ---------- Detalle de vacante (público) ----------

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var vacante = await _vacanteService.ObtenerDetalleAsync(id);
        if (vacante is null)
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
                ViewBag.YaPostulo = await _postulacionService.YaPostuloAsync(candidatoId.Value, id);
            }
        }

        return View(vacante);
    }

    // ---------- Perfil público de empresa ----------

    [HttpGet]
    public async Task<IActionResult> PerfilEmpresa(int id, int pagina = 1)
    {
        var perfil = await _empresaService.ObtenerPerfilPublicoAsync(id);
        if (perfil is null)
        {
            return RedirectToAction("Error404", "Home");
        }

        var vacantesDeLaEmpresa = await _vacanteService.ObtenerVacantesDeEmpresaAsync(id);
        ViewBag.VacantesDeLaEmpresa = this.Paginar(
            vacantesDeLaEmpresa.Where(v => v.Estado == "Activa"),
            pagina);

        return View(perfil);
    }

    // ---------- Aplicar (solo Candidato) ----------

    [HttpPost]
    [Authorize(Roles = RolesSistema.Candidato)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aplicar(int vacanteId)
    {
        var candidatoId = await ObtenerCandidatoIdAsync()
            ?? throw new InvalidOperationException("No se encontró tu perfil de candidato.");

        try
        {
            await _postulacionService.PostularAsync(candidatoId, vacanteId);
            TempData["Exito"] = "¡Postulación enviada!";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Detalle), new { id = vacanteId });
    }

    private async Task<int?> ObtenerCandidatoIdAsync()
    {
        if (UsuarioActualId is null) return null;
        var perfil = await _candidatoService.ObtenerPerfilPorUsuarioIdAsync(UsuarioActualId);
        return perfil?.Id;
    }
}
