using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// CRM interno de ChambeaJobs: pipeline comercial de empresas (prospectos y
/// clientes), ficha por empresa y registro de actividades de seguimiento.
/// Exclusivo para el equipo interno (rol Administrador).
/// </summary>
[Authorize(Roles = RolesSistema.CualquierAdmin)]
[Route("Admin/Crm")]
public class AdminCrmController : Controller
{
    private readonly ICrmService _crmService;
    private readonly IFileStorageService _fileStorage;

    public AdminCrmController(ICrmService crmService, IFileStorageService fileStorage)
    {
        _crmService = crmService;
        _fileStorage = fileStorage;
    }

    private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    private string UsuarioActualNombre => User.Identity?.Name ?? "Admin";

    [HttpGet("")]
    [HttpGet("FasesProcesoComercial")]
    public async Task<IActionResult> FasesProcesoComercial()
    {
        var columnas = await _crmService.ObtenerPipelineAsync();
        return View(columnas);
    }

    [HttpGet("Lista")]
    public async Task<IActionResult> Lista(string? busqueda, EtapaPipelineCrm? etapa, int pagina = 1)
    {
        ViewBag.Busqueda = busqueda;
        ViewBag.EtapaSeleccionada = etapa;
        var empresas = await _crmService.ListarAsync(busqueda, etapa);
        return View(this.Paginar(empresas, pagina));
    }

    [HttpGet("Nueva")]
    public IActionResult Nueva() => View(new CrmEmpresaFormDto());

    [HttpPost("Nueva")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nueva(CrmEmpresaFormDto modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var id = await _crmService.CrearAsync(modelo, UsuarioActualId);
        TempData["Exito"] = "Empresa agregada al CRM.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpGet("Detalle/{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var detalle = await _crmService.ObtenerDetalleAsync(id);
        if (detalle is null) return NotFound();

        if (detalle.EmpresaId.HasValue)
        {
            ViewBag.Estadisticas = await _crmService.ObtenerEstadisticasAsync(detalle.EmpresaId.Value);
        }

        return View(detalle);
    }

    [HttpGet("Editar/{id:int}")]
    public async Task<IActionResult> Editar(int id)
    {
        var detalle = await _crmService.ObtenerDetalleAsync(id);
        if (detalle is null) return NotFound();

        var modelo = new CrmEmpresaFormDto
        {
            EmpresaId = detalle.EmpresaId,
            NombreEmpresa = detalle.NombreEmpresa,
            Etapa = detalle.Etapa,
            ContactoPrincipal = detalle.ContactoPrincipal,
            Telefono = detalle.Telefono,
            Correo = detalle.Correo,
            RUC = detalle.RUC,
            Direccion = detalle.Direccion,
            SectorEmpresarial = detalle.SectorEmpresarial,
            TamanoEmpresa = detalle.TamanoEmpresa,
            SitioWeb = detalle.SitioWeb,
            RedesSociales = detalle.RedesSociales,
            Observaciones = detalle.Observaciones
        };
        ViewBag.Id = id;
        return View(modelo);
    }

    [HttpPost("Editar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, CrmEmpresaFormDto modelo)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Id = id;
            return View(modelo);
        }

        await _crmService.ActualizarAsync(id, modelo);
        TempData["Exito"] = "Cambios guardados.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost("SubirArchivo/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirArchivo(int id, IFormFile archivo, string? descripcion)
    {
        if (archivo is null || archivo.Length == 0)
        {
            TempData["Error"] = "Selecciona un archivo.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        var ruta = await _fileStorage.GuardarArchivoAsync(archivo, "crm");
        await _crmService.AgregarArchivoAdjuntoAsync(id, archivo.FileName, ruta, descripcion, UsuarioActualId);
        TempData["Exito"] = "Archivo adjuntado.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost("EliminarArchivo/{id:int}/{archivoId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarArchivo(int id, int archivoId)
    {
        await _crmService.EliminarArchivoAdjuntoAsync(archivoId);
        TempData["Exito"] = "Archivo eliminado.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpGet("Agenda")]
    public async Task<IActionResult> Agenda(DateTime? desde, DateTime? hasta, int pagina = 1)
    {
        var fechaDesde = desde ?? DateTime.Today;
        var fechaHasta = hasta ?? DateTime.Today.AddDays(30);
        ViewBag.Desde = fechaDesde;
        ViewBag.Hasta = fechaHasta;

        var items = await _crmService.ObtenerAgendaAsync(fechaDesde, fechaHasta.AddDays(1).AddSeconds(-1));
        return View(this.Paginar(items, pagina));
    }

    [HttpGet("Reportes")]
    public async Task<IActionResult> Reportes()
    {
        var reporte = await _crmService.ObtenerReporteAsync();
        return View(reporte);
    }

    [HttpPost("ExportarReporte")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportarReporte(CrmReporteExportOpcionesDto opciones)
    {
        var pdf = await _crmService.GenerarReportePdfAsync(opciones);
        var nombreArchivo = $"reporte-crm-{DateTime.Now:yyyyMMdd-HHmm}.pdf";
        return File(pdf, "application/pdf", nombreArchivo);
    }

    [HttpPost("MoverEtapa")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoverEtapa([FromBody] CrmMoverEtapaDto dto)
    {
        try
        {
            await _crmService.MoverEtapaAsync(dto);
            return Json(new { exito = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("Eliminar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _crmService.EliminarAsync(id);
        TempData["Exito"] = "Registro eliminado.";
        return RedirectToAction(nameof(Lista));
    }

    [HttpPost("RegistrarActividad/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarActividad(int id, CrmActividadFormDto modelo)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Revisa los datos de la actividad.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        await _crmService.RegistrarActividadAsync(id, modelo, UsuarioActualId, UsuarioActualNombre);
        TempData["Exito"] = "Actividad registrada.";
        return RedirectToAction(nameof(Detalle), new { id });
    }
}
