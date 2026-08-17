using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// Cubre las pantallas de:
/// Dashboard Empresa, Perfil de Empresa, Comprar/Renovar Paquete,
/// Historial de Pagos, Publicar Vacante, Editar Vacante y Mis Vacantes.
/// </summary>
[Authorize(Roles = RolesSistema.Empresa)]
public class EmpresaController : Controller
{
    private readonly IEmpresaService _empresaService;
    private readonly IVacanteService _vacanteService;
    private readonly IPaqueteEmpresaService _paqueteService;
    private readonly ICatalogoService _catalogoService;
    private readonly IPostulacionService _postulacionService;
    private readonly ICandidatoService _candidatoService;
    private readonly IEvaluacionService _evaluacionService;
    private readonly UserManager<ChambeaJobs.Infrastructure.Identity.ApplicationUser> _userManager;
    private readonly IComprobantePagoService _comprobantePagoService;

    public EmpresaController(
        IEmpresaService empresaService,
        IVacanteService vacanteService,
        IPaqueteEmpresaService paqueteService,
        ICatalogoService catalogoService,
        IPostulacionService postulacionService,
        ICandidatoService candidatoService,
        IEvaluacionService evaluacionService,
        UserManager<ChambeaJobs.Infrastructure.Identity.ApplicationUser> userManager,
        IComprobantePagoService comprobantePagoService)
    {
        _empresaService = empresaService;
        _vacanteService = vacanteService;
        _paqueteService = paqueteService;
        _catalogoService = catalogoService;
        _postulacionService = postulacionService;
        _candidatoService = candidatoService;
        _evaluacionService = evaluacionService;
        _userManager = userManager;
        _comprobantePagoService = comprobantePagoService;
    }

    private string UsuarioActualId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("No se pudo determinar el usuario autenticado.");

    /// <summary>
    /// Resuelve el Id interno de Empresa a partir del usuario autenticado.
    /// Centralizado aquí porque casi todas las acciones de este controlador lo necesitan.
    /// </summary>
    private async Task<int> ObtenerEmpresaIdAsync()
    {
        var empresaId = await _empresaService.ObtenerEmpresaIdPorUsuarioAsync(UsuarioActualId);
        return empresaId ?? throw new InvalidOperationException("No se encontró el perfil de esta empresa.");
    }

    // ---------- Dashboard ----------

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var empresaId = await ObtenerEmpresaIdAsync();
        var estadoPaquete = await _paqueteService.ObtenerEstadoPaqueteAsync(empresaId);
        var vacantes = await _vacanteService.ObtenerVacantesDeEmpresaAsync(empresaId);
        var resumen = await _postulacionService.ObtenerResumenDashboardAsync(empresaId);

        ViewBag.EstadoPaquete = estadoPaquete;
        ViewBag.VacantesActivas = vacantes.Count(v => v.Estado == "Activa");
        ViewBag.TotalVacantes = vacantes.Count;
        ViewBag.Resumen = resumen;

        return View(vacantes.Take(5).ToList());
    }

    // ---------- Perfil de empresa ----------

    [HttpGet]
    public async Task<IActionResult> Perfil()
    {
        var perfil = await _empresaService.ObtenerPerfilPorUsuarioIdAsync(UsuarioActualId);
        if (perfil is null)
        {
            TempData["Error"] = "No se encontró tu perfil. Contacta a soporte.";
            return RedirectToAction("Index", "Home");
        }

        var modelo = new EditarPerfilEmpresaDto
        {
            NombreEmpresa = perfil.NombreEmpresa,
            Descripcion = perfil.Descripcion,
            SitioWeb = perfil.SitioWeb,
            UbicacionId = perfil.UbicacionId,
            LogoUrlActual = perfil.LogoUrl,
            UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync()
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Perfil(EditarPerfilEmpresaDto modelo)
    {
        if (!ModelState.IsValid)
        {
            modelo.UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync();
            return View(modelo);
        }

        try
        {
            await _empresaService.ActualizarPerfilAsync(UsuarioActualId, modelo);
            TempData["Exito"] = "Perfil de empresa actualizado.";
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            modelo.UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync();
            return View(modelo);
        }

        return RedirectToAction(nameof(Perfil));
    }

    // ---------- Perfil enriquecido (historia, cultura, beneficios, redes, colaboradores) ----------

    [HttpGet]
    public async Task<IActionResult> PerfilExtendido()
    {
        var modelo = await _empresaService.ObtenerPerfilExtendidoAsync(UsuarioActualId);
        if (modelo is null)
        {
            TempData["Error"] = "No se encontró tu perfil. Contacta a soporte.";
            return RedirectToAction("Index", "Home");
        }

        var empresaId = await ObtenerEmpresaIdAsync();
        var perfilPublico = await _empresaService.ObtenerPerfilPublicoAsync(empresaId);
        ViewBag.Galeria = perfilPublico?.Galeria ?? new List<EmpresaGaleriaDto>();
        ViewBag.LogoUrlActual = perfilPublico?.LogoUrl;
        ViewBag.EmpresaId = empresaId;

        return View(modelo);
    }

    /// <summary>
    /// Sube/reemplaza el logo de la empresa — vive como acción separada
    /// (con su propio formulario en la vista) porque el logo se administra
    /// desde "Mi Empresa" (PerfilExtendido), que es la página a la que el
    /// menú lateral realmente lleva. El formulario de Perfil básico también
    /// puede subir logo, pero casi nadie navega ahí directamente.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirLogo(IFormFile logo)
    {
        if (logo is null || logo.Length == 0)
        {
            TempData["Error"] = "Selecciona una imagen para el logo.";
            return RedirectToAction(nameof(PerfilExtendido));
        }

        try
        {
            await _empresaService.ActualizarLogoAsync(UsuarioActualId, logo);
            TempData["Exito"] = "Logo actualizado.";
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(PerfilExtendido));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PerfilExtendido(EditarPerfilExtendidoEmpresaDto modelo)
    {
        if (!ModelState.IsValid)
        {
            var empresaId = await ObtenerEmpresaIdAsync();
            ViewBag.Galeria = (await _empresaService.ObtenerPerfilPublicoAsync(empresaId))?.Galeria ?? new List<EmpresaGaleriaDto>();
            ViewBag.EmpresaId = empresaId;
            return View(modelo);
        }

        await _empresaService.ActualizarPerfilExtendidoAsync(UsuarioActualId, modelo);
        TempData["Exito"] = "Perfil de empresa actualizado. Así lo verán los candidatos.";
        return RedirectToAction(nameof(PerfilExtendido));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarFotoGaleria(IFormFile foto, string? titulo)
    {
        if (foto is null)
        {
            TempData["Error"] = "Selecciona una foto para subir.";
            return RedirectToAction(nameof(PerfilExtendido));
        }

        try
        {
            await _empresaService.AgregarFotoGaleriaAsync(UsuarioActualId, foto, titulo);
            TempData["Exito"] = "Foto agregada a la galería.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(PerfilExtendido));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarVideoGaleria(IFormFile video, string? titulo)
    {
        if (video is null || video.Length == 0)
        {
            TempData["Error"] = "Selecciona un archivo de video.";
            return RedirectToAction(nameof(PerfilExtendido));
        }

        try
        {
            await _empresaService.AgregarVideoGaleriaAsync(UsuarioActualId, video, titulo);
            TempData["Exito"] = "Video agregado a la galería.";
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(PerfilExtendido));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarGaleria(int id)
    {
        try
        {
            await _empresaService.EliminarGaleriaAsync(UsuarioActualId, id);
            TempData["Exito"] = "Elemento eliminado de la galería.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(PerfilExtendido));
    }

    // ---------- Mis vacantes ----------

    [HttpGet]
    public async Task<IActionResult> MisVacantes()
    {
        var empresaId = await ObtenerEmpresaIdAsync();
        var vacantes = await _vacanteService.ObtenerVacantesDeEmpresaAsync(empresaId);
        return View(vacantes);
    }

    /// <summary>Carreras de una Categoría, para el desplegable dependiente en Publicar/Editar Vacante (AJAX).</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerCarreras(int categoriaId)
    {
        var carreras = await _catalogoService.ObtenerCarrerasPorCategoriaAsync(categoriaId);
        return Json(carreras);
    }

    // ---------- Publicar vacante ----------

    [HttpGet]
    public async Task<IActionResult> PublicarVacante()
    {
        var empresaId = await ObtenerEmpresaIdAsync();
        var estadoPaquete = await _paqueteService.ObtenerEstadoPaqueteAsync(empresaId);

        ViewBag.EstadoPaquete = estadoPaquete;

        var modelo = new VacanteFormDto
        {
            CategoriasDisponibles = await _catalogoService.ObtenerCategoriasAsync(),
            UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync()
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublicarVacante(VacanteFormDto modelo)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        if (!ModelState.IsValid)
        {
            modelo.CategoriasDisponibles = await _catalogoService.ObtenerCategoriasAsync();
            modelo.UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync();
            ViewBag.EstadoPaquete = await _paqueteService.ObtenerEstadoPaqueteAsync(empresaId);
            return View(modelo);
        }

        try
        {
            var vacanteId = await _vacanteService.PublicarVacanteAsync(empresaId, modelo);
            TempData["Exito"] = "Vacante publicada correctamente.";
            return RedirectToAction(nameof(MisVacantes));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            modelo.CategoriasDisponibles = await _catalogoService.ObtenerCategoriasAsync();
            modelo.UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync();
            ViewBag.EstadoPaquete = await _paqueteService.ObtenerEstadoPaqueteAsync(empresaId);
            return View(modelo);
        }
    }

    // ---------- Editar vacante ----------

    [HttpGet]
    public async Task<IActionResult> EditarVacante(int id)
    {
        var empresaId = await ObtenerEmpresaIdAsync();
        var modelo = await _vacanteService.ObtenerParaEditarAsync(empresaId, id);

        if (modelo is null)
        {
            TempData["Error"] = "Esta vacante no existe o no te pertenece.";
            return RedirectToAction(nameof(MisVacantes));
        }

        modelo.CategoriasDisponibles = await _catalogoService.ObtenerCategoriasAsync();
        modelo.UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync();
        ViewBag.EstadoPaquete = await _paqueteService.ObtenerEstadoPaqueteAsync(empresaId);

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarVacante(VacanteFormDto modelo)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        if (!ModelState.IsValid)
        {
            modelo.CategoriasDisponibles = await _catalogoService.ObtenerCategoriasAsync();
            modelo.UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync();
            ViewBag.EstadoPaquete = await _paqueteService.ObtenerEstadoPaqueteAsync(empresaId);
            return View(modelo);
        }

        try
        {
            await _vacanteService.ActualizarVacanteAsync(empresaId, modelo);
            TempData["Exito"] = "Vacante actualizada.";
            return RedirectToAction(nameof(MisVacantes));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            modelo.CategoriasDisponibles = await _catalogoService.ObtenerCategoriasAsync();
            modelo.UbicacionesDisponibles = await _catalogoService.ObtenerUbicacionesAsync();
            ViewBag.EstadoPaquete = await _paqueteService.ObtenerEstadoPaqueteAsync(empresaId);
            return View(modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarVacante(int id)
    {
        var empresaId = await ObtenerEmpresaIdAsync();
        try
        {
            await _vacanteService.CerrarVacanteAsync(empresaId, id);
            TempData["Exito"] = "Vacante cerrada. Ya no aparecerá en las búsquedas.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(MisVacantes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarVacante(int id)
    {
        var empresaId = await ObtenerEmpresaIdAsync();
        try
        {
            await _vacanteService.EliminarVacanteAsync(empresaId, id);
            TempData["Exito"] = "Vacante eliminada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(MisVacantes));
    }

    // ---------- Comprar / Renovar paquete ----------

    [HttpGet]
    public async Task<IActionResult> ComprarPaquete()
    {
        var empresaId = await ObtenerEmpresaIdAsync();
        ViewBag.EstadoPaquete = await _paqueteService.ObtenerEstadoPaqueteAsync(empresaId);
        ViewBag.PlanesDisponibles = await _paqueteService.ObtenerPlanesDisponiblesAsync();
        return View(new RegistrarPagoDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ComprarPaquete(RegistrarPagoDto modelo)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        if (!ModelState.IsValid)
        {
            ViewBag.EstadoPaquete = await _paqueteService.ObtenerEstadoPaqueteAsync(empresaId);
            ViewBag.PlanesDisponibles = await _paqueteService.ObtenerPlanesDisponiblesAsync();
            return View(modelo);
        }

        await _paqueteService.RegistrarSolicitudPagoAsync(empresaId, modelo);

        TempData["Exito"] = "Tu solicitud de pago fue registrada. Un administrador la revisará y activará tu paquete en breve.";
        return RedirectToAction(nameof(Dashboard));
    }

    // ---------- Historial de pagos ----------

    [HttpGet]
    public async Task<IActionResult> HistorialPagos()
    {
        var empresaId = await ObtenerEmpresaIdAsync();
        var historial = await _paqueteService.ObtenerHistorialAsync(empresaId);
        return View(historial);
    }

    [HttpGet]
    public async Task<IActionResult> DescargarComprobante(int pagoId)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        try
        {
            var correoEmpresa = User.FindFirstValue(ClaimTypes.Email) ?? UsuarioActualId;
            var pdfBytes = await _comprobantePagoService.GenerarComprobanteAsync(pagoId, empresaId, correoEmpresa);
            return File(pdfBytes, "application/pdf", $"Comprobante_ChambeaJobs_{pagoId}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(HistorialPagos));
        }
    }

    // ---------- Candidatos postulados a una vacante ----------

    [HttpGet]
    public async Task<IActionResult> CandidatosPostulados(int vacanteId)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        try
        {
            var postulantes = await _postulacionService.ObtenerPostulantesDeVacanteAsync(empresaId, vacanteId);

            // Teléfono de cada candidato (vive en Identity, no en el dominio) —
            // se usa para armar los botones de "enviar por WhatsApp".
            foreach (var candidato in postulantes)
            {
                if (!string.IsNullOrWhiteSpace(candidato.UsuarioId))
                {
                    var usuario = await _userManager.FindByIdAsync(candidato.UsuarioId);
                    candidato.Telefono = usuario?.PhoneNumber;
                }
            }

            ViewBag.VacanteId = vacanteId;
            ViewBag.EstadosDisponibles = await _postulacionService.ObtenerEstadosDisponiblesAsync();
            ViewBag.Evaluaciones = await _evaluacionService.ObtenerResumenPorPostulacionesAsync(
                postulantes.Select(p => p.PostulacionId));
            ViewBag.EstadoPaquete = await _paqueteService.ObtenerEstadoPaqueteAsync(empresaId);
            return View(postulantes);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(MisVacantes));
        }
    }

    // ---------- Evaluaciones psicométricas ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarEvaluacion(EnviarEvaluacionDto modelo, int vacanteId)
    {
        try
        {
            await _evaluacionService.EnviarEvaluacionAsync(modelo);
            TempData["Exito"] = "Evaluación enviada al candidato.";
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(CandidatosPostulados), new { vacanteId });
    }

    [HttpGet]
    public async Task<IActionResult> ResultadosEvaluacion(int evaluacionId, int vacanteId)
    {
        var empresaId = await ObtenerEmpresaIdAsync();
        var vacante = await _vacanteService.ObtenerParaEditarAsync(empresaId, vacanteId);
        if (vacante is null)
        {
            TempData["Error"] = "Esta vacante no existe o no te pertenece.";
            return RedirectToAction(nameof(MisVacantes));
        }

        var resultado = await _evaluacionService.ObtenerResultadoAsync(evaluacionId);
        if (resultado is null)
        {
            TempData["Error"] = "El candidato todavía no ha completado esta evaluación.";
            return RedirectToAction(nameof(CandidatosPostulados), new { vacanteId });
        }

        ViewBag.VacanteId = vacanteId;
        return View(resultado);
    }

    [HttpGet]
    public async Task<IActionResult> DescargarCv(int postulacionId, int vacanteId)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        try
        {
            var rutaArchivo = await _postulacionService.MarcarCvRevisadoYObtenerUrlAsync(empresaId, postulacionId);
            if (string.IsNullOrEmpty(rutaArchivo))
            {
                TempData["Error"] = "Este candidato no tiene un CV subido.";
                return RedirectToAction(nameof(CandidatosPostulados), new { vacanteId });
            }

            return Redirect(rutaArchivo);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(CandidatosPostulados), new { vacanteId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> VideoEntrevista(int postulacionId, int vacanteId)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        try
        {
            var sala = await _postulacionService.ObtenerSalaVideollamadaEmpresaAsync(empresaId, postulacionId);
            ViewBag.SalaVideollamadaId = sala;
            ViewBag.VacanteId = vacanteId;
            return View();
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(CandidatosPostulados), new { vacanteId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> VerVideoCv(int postulacionId, int vacanteId)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        var estadoPaquete = await _paqueteService.ObtenerEstadoPaqueteAsync(empresaId);
        if (!estadoPaquete.IncluyeVideoCv)
        {
            TempData["Error"] = "Ver el Video Currículum de los candidatos solo está disponible en el Plan Empresarial. Actualiza tu plan desde \"Comprar paquete\" para usar esta función.";
            return RedirectToAction(nameof(CandidatosPostulados), new { vacanteId });
        }

        try
        {
            var detalle = await _postulacionService.ObtenerDetalleVideoCvAsync(empresaId, postulacionId);
            var comentarios = await _postulacionService.ObtenerComentariosVideoCvAsync(empresaId, postulacionId);

            ViewBag.PostulacionId = postulacionId;
            ViewBag.VacanteId = vacanteId;
            ViewBag.Detalle = detalle;
            return View(comentarios);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(CandidatosPostulados), new { vacanteId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ComentarVideoCv(int postulacionId, int vacanteId, string comentario, int? calificacion)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        var estadoPaquete = await _paqueteService.ObtenerEstadoPaqueteAsync(empresaId);
        if (!estadoPaquete.IncluyeVideoCv)
        {
            TempData["Error"] = "Esta función solo está disponible en el Plan Empresarial.";
            return RedirectToAction(nameof(CandidatosPostulados), new { vacanteId });
        }

        if (string.IsNullOrWhiteSpace(comentario))
        {
            TempData["Error"] = "Escribe un comentario antes de enviarlo.";
            return RedirectToAction(nameof(VerVideoCv), new { postulacionId, vacanteId });
        }

        try
        {
            var nombreReclutador = User.Identity?.Name ?? "Reclutador";
            await _postulacionService.ComentarVideoCvAsync(empresaId, postulacionId, UsuarioActualId, nombreReclutador, comentario, calificacion);
            TempData["Exito"] = "Comentario agregado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(VerVideoCv), new { postulacionId, vacanteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProgramarEntrevista(int postulacionId, int vacanteId, DateTime fechaEntrevista, string? nota)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        try
        {
            await _postulacionService.ProgramarEntrevistaAsync(empresaId, postulacionId, fechaEntrevista, nota);
            TempData["Exito"] = "Entrevista programada. Se notificó al candidato.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(CandidatosPostulados), new { vacanteId });
    }

    // NOTA: la acción "Comparar" (comparador de candidatos lado a lado) se
    // eliminó a pedido — la evaluación por estrellas/compatibilidad ya
    // cumple esa función de ranking, y tener ambas era redundante.

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoPostulacion(int postulacionId, int nuevoEstadoId, int vacanteId)
    {
        var empresaId = await ObtenerEmpresaIdAsync();

        try
        {
            await _postulacionService.CambiarEstadoAsync(empresaId, postulacionId, nuevoEstadoId);
            TempData["Exito"] = "Estado de la postulación actualizado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(CandidatosPostulados), new { vacanteId });
    }

    // ---------- Detalle de candidato (vista de solo lectura del perfil) ----------

    [HttpGet]
    public async Task<IActionResult> DetalleCandidato(int candidatoId)
    {
        var perfil = await _candidatoService.ObtenerPerfilPorIdAsync(candidatoId);
        if (perfil is null)
        {
            TempData["Error"] = "No se encontró este candidato.";
            return RedirectToAction(nameof(MisVacantes));
        }

        return View(perfil);
    }
}
