using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// Panel de Administrador: Dashboard, Gestión de Usuarios/Empresas/Vacantes,
/// Categorías, Ubicaciones, Configuración del Sistema, Reportes, Auditoría
/// y la cola de aprobación de pagos.
/// </summary>
[Authorize(Roles = RolesSistema.CualquierAdmin)]
public class AdminController : Controller
{
    private readonly IPaqueteEmpresaService _paqueteService;
    private readonly IAdminUsuarioService _usuarioService;
    private readonly IAdminEmpresaService _empresaService;
    private readonly IAdminVacanteService _vacanteService;
    private readonly IPasantiaService _pasantiaService;
    private readonly IAdminCatalogoService _catalogoService;
    private readonly IConfiguracionSistemaService _configuracionService;
    private readonly IReporteService _reporteService;
    private readonly IAuditoriaService _auditoriaService;
    private readonly ISoporteService _soporteService;
    private readonly IComprobantePagoService _comprobantePagoService;

    public AdminController(
        IPaqueteEmpresaService paqueteService,
        IAdminUsuarioService usuarioService,
        IAdminEmpresaService empresaService,
        IAdminVacanteService vacanteService,
        IPasantiaService pasantiaService,
        IAdminCatalogoService catalogoService,
        IConfiguracionSistemaService configuracionService,
        IReporteService reporteService,
        IAuditoriaService auditoriaService,
        ISoporteService soporteService,
        IComprobantePagoService comprobantePagoService)
    {
        _paqueteService = paqueteService;
        _usuarioService = usuarioService;
        _empresaService = empresaService;
        _vacanteService = vacanteService;
        _pasantiaService = pasantiaService;
        _catalogoService = catalogoService;
        _configuracionService = configuracionService;
        _reporteService = reporteService;
        _auditoriaService = auditoriaService;
        _soporteService = soporteService;
        _comprobantePagoService = comprobantePagoService;
    }

    private string AdminActualId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("No se pudo determinar el usuario autenticado.");

    // ---------- Dashboard ----------

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var reporte = await _reporteService.ObtenerDashboardAsync();
        ViewBag.PagosPendientesCount = (await _paqueteService.ObtenerPagosPendientesAsync()).Count;
        return View(reporte);
    }

    // ---------- Pagos pendientes ----------

    [HttpGet]
    public async Task<IActionResult> PagosPendientes()
    {
        var pendientes = await _paqueteService.ObtenerPagosPendientesAsync();
        return View(pendientes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AprobarPago(int pagoId, string? comentario)
    {
        try
        {
            await _paqueteService.AprobarPagoAsync(pagoId, comentario);
            TempData["Exito"] = "Pago aprobado. El paquete de la empresa ya está vigente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(PagosPendientes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RechazarPago(int pagoId, string? comentario)
    {
        try
        {
            await _paqueteService.RechazarPagoAsync(pagoId, comentario);
            TempData["Exito"] = "Pago rechazado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(PagosPendientes));
    }

    // ---------- Historial completo de pagos (todas las empresas) ----------

    [HttpGet]
    public async Task<IActionResult> HistorialPagos(string? nombreEmpresa, string? estadoPago, DateTime? desde, DateTime? hasta)
    {
        var historial = await _paqueteService.ObtenerHistorialCompletoAsync(nombreEmpresa, estadoPago, desde, hasta);

        ViewBag.NombreEmpresa = nombreEmpresa;
        ViewBag.EstadoPago = estadoPago;
        ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
        ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
        ViewBag.TotalMostrado = historial.Where(p => p.EstadoPago == "Aprobado").Sum(p => p.Monto);

        return View(historial);
    }

    [HttpGet]
    public async Task<IActionResult> DescargarComprobante(int pagoId)
    {
        try
        {
            var pdfBytes = await _comprobantePagoService.GenerarComprobanteAdminAsync(pagoId);
            return File(pdfBytes, "application/pdf", $"Comprobante_ChambeaJobs_{pagoId}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(HistorialPagos));
        }
    }

    // ---------- Gestión de Usuarios ----------

    [HttpGet]
    public async Task<IActionResult> GestionUsuarios()
    {
        var usuarios = await _usuarioService.ObtenerCandidatosYAdminsAsync();
        return View(usuarios);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspenderUsuario(string usuarioId)
    {
        try
        {
            await _usuarioService.SuspenderAsync(usuarioId, AdminActualId);
            TempData["Exito"] = "Usuario suspendido.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(GestionUsuarios));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivarUsuario(string usuarioId)
    {
        try
        {
            await _usuarioService.ActivarAsync(usuarioId, AdminActualId);
            TempData["Exito"] = "Usuario activado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(GestionUsuarios));
    }

    // ---------- Gestión de Empresas ----------

    [HttpGet]
    public async Task<IActionResult> GestionEmpresas()
    {
        var empresas = await _empresaService.ObtenerTodasAsync();
        return View(empresas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspenderEmpresa(string usuarioId)
    {
        try
        {
            await _empresaService.SuspenderAsync(usuarioId, AdminActualId);
            TempData["Exito"] = "Empresa suspendida.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(GestionEmpresas));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivarEmpresa(string usuarioId)
    {
        try
        {
            await _empresaService.ActivarAsync(usuarioId, AdminActualId);
            TempData["Exito"] = "Empresa activada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(GestionEmpresas));
    }

    // ---------- Gestión de Vacantes ----------

    [HttpGet]
    public async Task<IActionResult> GestionVacantes()
    {
        var vacantes = await _vacanteService.ObtenerTodasAsync();
        return View(vacantes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DespublicarVacante(int id)
    {
        try
        {
            await _vacanteService.DespublicarAsync(id, AdminActualId);
            TempData["Exito"] = "Vacante despublicada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(GestionVacantes));
    }

    // ---------- Gestión de Pasantías (módulo independiente de Vacantes) ----------

    [HttpGet]
    public async Task<IActionResult> GestionPasantias()
    {
        var pasantias = await _pasantiaService.ObtenerTodasAsync();
        return View(pasantias);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DespublicarPasantia(int id)
    {
        try
        {
            await _pasantiaService.DespublicarAdminAsync(id, AdminActualId);
            TempData["Exito"] = "Pasantía despublicada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(GestionPasantias));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarPasantiaAdmin(int id)
    {
        try
        {
            await _pasantiaService.EliminarAdminAsync(id, AdminActualId);
            TempData["Exito"] = "Pasantía eliminada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(GestionPasantias));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarVacanteAdmin(int id)
    {
        try
        {
            await _vacanteService.EliminarAsync(id, AdminActualId);
            TempData["Exito"] = "Vacante eliminada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(GestionVacantes));
    }

    // ---------- Categorías ----------

    [HttpGet]
    public async Task<IActionResult> Categorias()
    {
        var categorias = await _catalogoService.ObtenerCategoriasAsync();
        return View(categorias);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCategoria(string nombre, string? descripcion)
    {
        try
        {
            await _catalogoService.CrearCategoriaAsync(nombre, descripcion);
            TempData["Exito"] = "Categoría creada.";
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Categorias));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCategoria(int id, string nombre, string? descripcion)
    {
        try
        {
            await _catalogoService.EditarCategoriaAsync(id, nombre, descripcion);
            TempData["Exito"] = "Categoría actualizada.";
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Categorias));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCategoria(int id)
    {
        try
        {
            await _catalogoService.EliminarCategoriaAsync(id);
            TempData["Exito"] = "Categoría eliminada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Categorias));
    }

    // ---------- Ubicaciones ----------

    [HttpGet]
    public async Task<IActionResult> Ubicaciones()
    {
        var ubicaciones = await _catalogoService.ObtenerUbicacionesAsync();
        return View(ubicaciones);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearUbicacion(string departamento, string ciudad)
    {
        try
        {
            await _catalogoService.CrearUbicacionAsync(departamento, ciudad);
            TempData["Exito"] = "Ubicación creada.";
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Ubicaciones));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarUbicacion(int id, string departamento, string ciudad)
    {
        try
        {
            await _catalogoService.EditarUbicacionAsync(id, departamento, ciudad);
            TempData["Exito"] = "Ubicación actualizada.";
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Ubicaciones));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarUbicacion(int id)
    {
        try
        {
            await _catalogoService.EliminarUbicacionAsync(id);
            TempData["Exito"] = "Ubicación eliminada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Ubicaciones));
    }

    // ---------- Configuración del sistema ----------

    [HttpGet]
    public async Task<IActionResult> Configuracion()
    {
        var planes = await _paqueteService.ObtenerTodosLosPlanesAsync();
        return View(planes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPlan(EditarPlanSuscripcionDto modelo)
    {
        try
        {
            await _paqueteService.ActualizarPlanAsync(modelo, AdminActualId);
            TempData["Exito"] = $"Plan \"{modelo.Nombre}\" actualizado.";
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Configuracion));
    }

    // ---------- Reportes ----------

    [HttpGet]
    public async Task<IActionResult> Reportes()
    {
        var reporte = await _reporteService.ObtenerDashboardAsync();
        return View("Dashboard", reporte);
    }

    // ---------- Auditoría ----------

    [HttpGet]
    public async Task<IActionResult> Auditoria()
    {
        var registros = await _auditoriaService.ObtenerRecientesAsync();
        return View(registros);
    }

    // ---------- Soporte ----------

    [HttpGet]
    public async Task<IActionResult> Soporte()
    {
        var tickets = await _soporteService.ObtenerTodosAsync();
        return View(tickets);
    }

    [HttpGet]
    public async Task<IActionResult> ChatSoporte(int id)
    {
        var ticket = await _soporteService.ObtenerConMensajesAsync(id);
        if (ticket is null)
        {
            return RedirectToAction("Error404", "Home");
        }

        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarMensajeSoporte(int ticketId, string mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
        {
            return BadRequest();
        }

        var enviado = await _soporteService.EnviarMensajeAsync(ticketId, AdminActualId, "Soporte ChambeaJobs", esAdmin: true, mensaje);
        return Json(enviado);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarTicketSoporte(int ticketId)
    {
        try
        {
            await _soporteService.CerrarTicketAsync(ticketId);
            TempData["Exito"] = "Ticket marcado como resuelto.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(ChatSoporte), new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResponderTicket(int ticketId, string respuesta)
    {
        try
        {
            await _soporteService.ResponderAsync(ticketId, respuesta, AdminActualId);
            TempData["Exito"] = "Respuesta enviada. El ticket quedó marcado como resuelto.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Soporte));
    }
}
