using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// Cubre las pantallas de Perfil Candidato (ver) y
/// Editar Perfil Candidato (datos básicos, foto, CV, experiencias,
/// educaciones y habilidades).
/// </summary>
[Authorize(Roles = RolesSistema.Candidato)]
public class CandidatoController : Controller
{
    private readonly ICandidatoService _candidatoService;
    private readonly IPostulacionService _postulacionService;
    private readonly IEvaluacionService _evaluacionService;
    private readonly ICatalogoService _catalogoService;
    private readonly ICvGeneradorService _cvGeneradorService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CandidatoController(
        ICandidatoService candidatoService,
        IPostulacionService postulacionService,
        IEvaluacionService evaluacionService,
        ICatalogoService catalogoService,
        ICvGeneradorService cvGeneradorService,
        UserManager<ApplicationUser> userManager)
    {
        _candidatoService = candidatoService;
        _postulacionService = postulacionService;
        _evaluacionService = evaluacionService;
        _catalogoService = catalogoService;
        _cvGeneradorService = cvGeneradorService;
        _userManager = userManager;
    }

    private string UsuarioActualId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("No se pudo determinar el usuario autenticado.");

    // ---------- Ver perfil ----------

    [HttpGet]
    public async Task<IActionResult> Perfil()
    {
        var perfil = await _candidatoService.ObtenerPerfilPorUsuarioIdAsync(UsuarioActualId);
        if (perfil is null)
        {
            // No debería ocurrir en flujo normal (se crea al registrarse),
            // pero se maneja explícitamente en vez de dejar una excepción sin control.
            TempData["Error"] = "No se encontró tu perfil. Contacta a soporte.";
            return RedirectToAction("Index", "Home");
        }

        return View(perfil);
    }

    [HttpGet]
    public async Task<IActionResult> GenerarCv()
    {
        var usuario = await _userManager.FindByIdAsync(UsuarioActualId);

        try
        {
            var pdfBytes = await _cvGeneradorService.GenerarPdfAsync(UsuarioActualId, usuario?.Email, usuario?.PhoneNumber);
            return File(pdfBytes, "application/pdf", "CV_ChambeaJobs.pdf");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Perfil));
        }
    }

    // ---------- Editar datos básicos, foto y CV ----------

    [HttpGet]
    public async Task<IActionResult> EditarPerfil()
    {
        var perfil = await _candidatoService.ObtenerPerfilPorUsuarioIdAsync(UsuarioActualId);
        if (perfil is null)
        {
            TempData["Error"] = "No se encontró tu perfil. Contacta a soporte.";
            return RedirectToAction("Index", "Home");
        }

        var modelo = new EditarPerfilCandidatoDto
        {
            Nombres = perfil.Nombres,
            Apellidos = perfil.Apellidos,
            Direccion = perfil.Direccion,
            FotoUrlActual = perfil.FotoUrl,
            CvNombreActual = perfil.CvNombreOriginal,
            VideoCvUrlActual = perfil.VideoCvUrl
        };

        ViewBag.Experiencias = perfil.Experiencias;
        ViewBag.Educaciones = perfil.Educaciones;
        ViewBag.Habilidades = perfil.Habilidades;
        ViewBag.Idiomas = perfil.Idiomas;
        ViewBag.Certificados = perfil.Certificados;
        ViewBag.IdiomasDisponibles = await _candidatoService.ObtenerIdiomasDisponiblesAsync();
            ViewBag.Categorias = await _catalogoService.ObtenerCategoriasAsync();
            ViewBag.Instituciones = await _catalogoService.ObtenerInstitucionesAsync();

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPerfil(EditarPerfilCandidatoDto modelo)
    {
        if (!ModelState.IsValid)
        {
            var perfilActual = await _candidatoService.ObtenerPerfilPorUsuarioIdAsync(UsuarioActualId);
            ViewBag.Experiencias = perfilActual?.Experiencias ?? new List<ExperienciaDto>();
            ViewBag.Educaciones = perfilActual?.Educaciones ?? new List<EducacionDto>();
            ViewBag.Habilidades = perfilActual?.Habilidades ?? new List<HabilidadCandidatoDto>();
            ViewBag.Idiomas = perfilActual?.Idiomas ?? new List<IdiomaCandidatoDto>();
            ViewBag.Certificados = perfilActual?.Certificados ?? new List<CertificadoDto>();
            ViewBag.IdiomasDisponibles = await _candidatoService.ObtenerIdiomasDisponiblesAsync();
            ViewBag.Categorias = await _catalogoService.ObtenerCategoriasAsync();
            ViewBag.Instituciones = await _catalogoService.ObtenerInstitucionesAsync();
            return View(modelo);
        }

        try
        {
            await _candidatoService.ActualizarDatosBasicosAsync(UsuarioActualId, modelo);
            TempData["Exito"] = "Perfil actualizado correctamente.";
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var perfilActual = await _candidatoService.ObtenerPerfilPorUsuarioIdAsync(UsuarioActualId);
            ViewBag.Experiencias = perfilActual?.Experiencias ?? new List<ExperienciaDto>();
            ViewBag.Educaciones = perfilActual?.Educaciones ?? new List<EducacionDto>();
            ViewBag.Habilidades = perfilActual?.Habilidades ?? new List<HabilidadCandidatoDto>();
            ViewBag.Idiomas = perfilActual?.Idiomas ?? new List<IdiomaCandidatoDto>();
            ViewBag.Certificados = perfilActual?.Certificados ?? new List<CertificadoDto>();
            ViewBag.IdiomasDisponibles = await _candidatoService.ObtenerIdiomasDisponiblesAsync();
            ViewBag.Categorias = await _catalogoService.ObtenerCategoriasAsync();
            ViewBag.Instituciones = await _catalogoService.ObtenerInstitucionesAsync();
            return View(modelo);
        }

        return RedirectToAction(nameof(EditarPerfil));
    }

    // ---------- Experiencias ----------

    /// <summary>Carreras de una Categoría, para el desplegable dependiente de "Educación" (AJAX, sin recargar la página).</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerCarreras(int categoriaId)
    {
        var carreras = await _catalogoService.ObtenerCarrerasPorCategoriaAsync(categoriaId);
        return Json(carreras);
    }

    /// <summary>Facultades/áreas de una Institución específica — paso 2 del catálogo (AJAX).</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerFacultadesPorInstitucion(int institucionId)
    {
        var facultades = await _catalogoService.ObtenerFacultadesPorInstitucionAsync(institucionId);
        return Json(facultades);
    }

    /// <summary>Carreras que ofrece una Facultad específica — paso 3 del catálogo, ya filtradas a lo que esa institución realmente ofrece (AJAX).</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerCarrerasPorFacultad(int facultadId)
    {
        var carreras = await _catalogoService.ObtenerCarrerasPorFacultadAsync(facultadId);
        return Json(carreras);
    }

    /// <summary>Todas las carreras agrupadas por categoría — para cuando el candidato elige "Otra institución" (AJAX).</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodasLasCarrerasAgrupadas()
    {
        var carreras = await _catalogoService.ObtenerTodasLasCarrerasAgrupadasAsync();
        return Json(carreras);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarExperiencia(ExperienciaDto experiencia)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Revisa los datos de la experiencia laboral.";
            return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-experiencia");
        }

        try
        {
            var nuevoId = await _candidatoService.AgregarExperienciaAsync(UsuarioActualId, experiencia);
            TempData["Exito"] = "Experiencia agregada.";
            return Redirect(Url.Action(nameof(EditarPerfil)) + $"?nuevoId={nuevoId}&tipo=experiencia#tab-experiencia");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-experiencia");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarExperiencia(int id)
    {
        try
        {
            await _candidatoService.EliminarExperienciaAsync(UsuarioActualId, id);
            TempData["Exito"] = "Experiencia eliminada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-experiencia");
    }

    // ---------- Educación ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarEducacion(EducacionDto educacion)
    {
        if (!ModelState.IsValid)
        {
            var detalles = string.Join(" ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m)));
            TempData["Error"] = string.IsNullOrWhiteSpace(detalles)
                ? "Revisa los datos de la educación."
                : $"Revisa los datos de la educación: {detalles}";
            return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-educacion");
        }

        try
        {
            var nuevoId = await _candidatoService.AgregarEducacionAsync(UsuarioActualId, educacion);
            TempData["Exito"] = "Educación agregada.";
            return Redirect(Url.Action(nameof(EditarPerfil)) + $"?nuevoId={nuevoId}&tipo=educacion#tab-educacion");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-educacion");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarEducacion(int id)
    {
        try
        {
            await _candidatoService.EliminarEducacionAsync(UsuarioActualId, id);
            TempData["Exito"] = "Educación eliminada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-educacion");
    }

    // ---------- Habilidades ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarHabilidad(string nombreHabilidad, string? nivelDominio)
    {
        if (string.IsNullOrWhiteSpace(nombreHabilidad))
        {
            TempData["Error"] = "Escribe el nombre de la habilidad.";
            return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-habilidades");
        }

        try
        {
            await _candidatoService.AgregarHabilidadAsync(UsuarioActualId, nombreHabilidad, nivelDominio);
            TempData["Exito"] = "Habilidad agregada.";
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-habilidades");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarHabilidad(int id)
    {
        try
        {
            await _candidatoService.EliminarHabilidadAsync(UsuarioActualId, id);
            TempData["Exito"] = "Habilidad eliminada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-habilidades");
    }

    // ---------- Idiomas ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarIdioma(int idiomaId, string nivel)
    {
        try
        {
            await _candidatoService.AgregarIdiomaAsync(UsuarioActualId, idiomaId, nivel);
            TempData["Exito"] = "Idioma agregado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-idiomas");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarIdioma(int idiomaId)
    {
        try
        {
            await _candidatoService.EliminarIdiomaAsync(UsuarioActualId, idiomaId);
            TempData["Exito"] = "Idioma eliminado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-idiomas");
    }

    // ---------- Certificados (PDF) ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarCertificado(string nombre, string? institucionEmisora, DateTime? fechaObtencion, IFormFile archivoPdf, string tipoDocumento)
    {
        if (string.IsNullOrWhiteSpace(nombre) || archivoPdf is null)
        {
            TempData["Error"] = "El nombre del documento y el archivo PDF son obligatorios.";
            return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-certificados");
        }

        try
        {
            await _candidatoService.AgregarCertificadoAsync(UsuarioActualId, nombre, institucionEmisora, fechaObtencion, archivoPdf, tipoDocumento);
            TempData["Exito"] = "Documento agregado.";
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-certificados");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCertificado(int id)
    {
        try
        {
            await _candidatoService.EliminarCertificadoAsync(UsuarioActualId, id);
            TempData["Exito"] = "Certificado eliminado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect(Url.Action(nameof(EditarPerfil)) + "#tab-certificados");
    }

    // NOTA: el módulo de Cursos se eliminó por completo a pedido (pestaña,
    // acciones AgregarCurso/EliminarCurso, y su carga en EditarPerfil/Perfil).

    // ---------- Mis postulaciones ----------

    [HttpGet]
    public async Task<IActionResult> MisPostulaciones()
    {
        var perfil = await _candidatoService.ObtenerPerfilPorUsuarioIdAsync(UsuarioActualId);
        if (perfil is null)
        {
            TempData["Error"] = "No se encontró tu perfil. Contacta a soporte.";
            return RedirectToAction("Index", "Home");
        }

        var postulaciones = await _postulacionService.ObtenerMisPostulacionesAsync(perfil.Id);
        ViewBag.Evaluaciones = await _evaluacionService.ObtenerPendientesDeCandidatoAsync(
            perfil.Id, postulaciones.Select(p => p.Id));
        return View(postulaciones);
    }

    [HttpGet]
    public async Task<IActionResult> VideoEntrevista(int postulacionId)
    {
        var perfil = await _candidatoService.ObtenerPerfilPorUsuarioIdAsync(UsuarioActualId);
        if (perfil is null)
        {
            TempData["Error"] = "No se encontró tu perfil. Contacta a soporte.";
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var sala = await _postulacionService.ObtenerSalaVideollamadaCandidatoAsync(perfil.Id, postulacionId);
            ViewBag.SalaVideollamadaId = sala;
            return View();
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(MisPostulaciones));
        }
    }

    // NOTA: la acción "ResponderPrueba" (simulada, con puntaje aleatorio) se
    // eliminó — ahora el motor real vive en EvaluacionController (Invitacion/
    // Realizar/Resultados), y EvaluacionService sincroniza automáticamente el
    // puntaje real con Postulacion.PruebaPsicometricaPuntaje al finalizar.
    // Dejar el endpoint simulado activo habría permitido que un candidato se
    // autoaprobara con un puntaje aleatorio sin responder la prueba real.

    // NOTA: el módulo de Favoritos se eliminó por completo a pedido
    // (nav, controlador, vista y botón "Guardar en favoritos").
}
