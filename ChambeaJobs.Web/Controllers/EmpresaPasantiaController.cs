using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Web.Extensions;
using ChambeaJobs.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// Módulo de Pasantías del lado Empresa, independiente de EmpresaController
/// (que maneja las Vacantes de empleo) para que ambos conceptos no se mezclen.
/// </summary>
[Authorize(Roles = RolesSistema.Empresa)]
public class EmpresaPasantiaController : Controller
{
    private readonly IPasantiaService _pasantiaService;
    private readonly ICatalogoService _catalogoService;
    private readonly IEmpresaService _empresaService;
    private readonly UserManager<ApplicationUser> _userManager;

    public EmpresaPasantiaController(
        IPasantiaService pasantiaService,
        ICatalogoService catalogoService,
        IEmpresaService empresaService,
        UserManager<ApplicationUser> userManager)
    {
        _pasantiaService = pasantiaService;
        _catalogoService = catalogoService;
        _empresaService = empresaService;
        _userManager = userManager;
    }

    private async Task<int> ObtenerEmpresaIdAsync()
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("No se pudo determinar el usuario autenticado.");
        var empresaId = await _empresaService.ObtenerEmpresaIdPorUsuarioAsync(usuarioId);
        return empresaId ?? throw new InvalidOperationException("No se encontró el perfil de esta empresa.");
    }

    // ---------- Mis pasantías ----------

    [HttpGet]
    public async Task<IActionResult> MisPasantias(int pagina = 1)
    {
        var empresaId = await ObtenerEmpresaIdAsync();
        var pasantias = await _pasantiaService.ObtenerPasantiasDeEmpresaAsync(empresaId);
        return View(this.Paginar(pasantias, pagina));
    }

    // ---------- Publicar pasantía ----------

    [HttpGet]
    public async Task<IActionResult> Publicar()
    {
        var modelo = new PasantiaFormDto
        {
            CategoriasDisponibles = await _catalogoService.ObtenerCategoriasAsync(),
            UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync()
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publicar(PasantiaFormDto modelo)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        if (!ModelState.IsValid)
        {
            modelo.CategoriasDisponibles = await _catalogoService.ObtenerCategoriasAsync();
            modelo.UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync();
            return View(modelo);
        }

        try
        {
            await _pasantiaService.PublicarPasantiaAsync(empresaId, modelo);
            TempData["Exito"] = "Pasantía publicada correctamente.";
            return RedirectToAction(nameof(MisPasantias));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            modelo.CategoriasDisponibles = await _catalogoService.ObtenerCategoriasAsync();
            modelo.UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync();
            return View(modelo);
        }
    }

    // ---------- Editar pasantía ----------

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var empresaId = await ObtenerEmpresaIdAsync();
        var modelo = await _pasantiaService.ObtenerParaEditarAsync(empresaId, id);

        if (modelo is null)
        {
            TempData["Error"] = "Esta pasantía no existe o no te pertenece.";
            return RedirectToAction(nameof(MisPasantias));
        }

        modelo.CategoriasDisponibles = await _catalogoService.ObtenerCategoriasAsync();
        modelo.UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync();
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(PasantiaFormDto modelo)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        if (!ModelState.IsValid)
        {
            modelo.CategoriasDisponibles = await _catalogoService.ObtenerCategoriasAsync();
            modelo.UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync();
            return View(modelo);
        }

        try
        {
            await _pasantiaService.ActualizarPasantiaAsync(empresaId, modelo);
            TempData["Exito"] = "Pasantía actualizada.";
            return RedirectToAction(nameof(MisPasantias));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            modelo.CategoriasDisponibles = await _catalogoService.ObtenerCategoriasAsync();
            modelo.UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync();
            return View(modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cerrar(int id)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        try
        {
            await _pasantiaService.CerrarPasantiaAsync(empresaId, id);
            TempData["Exito"] = "Pasantía despublicada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(MisPasantias));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        try
        {
            await _pasantiaService.EliminarPasantiaAsync(empresaId, id);
            TempData["Exito"] = "Pasantía eliminada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(MisPasantias));
    }

    // ---------- Postulantes de una pasantía ----------

    [HttpGet]
    public async Task<IActionResult> Postulantes(int pasantiaId, int pagina = 1)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        try
        {
            var postulantes = await _pasantiaService.ObtenerPostulantesAsync(empresaId, pasantiaId);

            // Teléfono de cada candidato (vive en Identity, no en el dominio) —
            // se usa para armar los botones de "enviar por WhatsApp", igual
            // que en CandidatosPostulados.cshtml (Vacantes).
            foreach (var candidato in postulantes)
            {
                if (!string.IsNullOrWhiteSpace(candidato.UsuarioId))
                {
                    var usuario = await _userManager.FindByIdAsync(candidato.UsuarioId);
                    candidato.Telefono = usuario?.PhoneNumber;
                }
            }

            ViewBag.PasantiaId = pasantiaId;
            return View(this.Paginar(postulantes, pagina));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(MisPasantias));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoPostulacion(int postulacionId, int pasantiaId, string nuevoEstado, string? nota)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        try
        {
            await _pasantiaService.CambiarEstadoPostulacionAsync(empresaId, postulacionId, nuevoEstado, nota);
            TempData["Exito"] = "Estado actualizado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Postulantes), new { pasantiaId });
    }
}
