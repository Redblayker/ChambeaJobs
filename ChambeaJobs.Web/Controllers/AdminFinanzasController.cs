using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// Módulo Contabilidad y Finanzas. CONFIDENCIAL: solo roles financieros
/// autorizados. Ver = SuperAdministrador/AdministradorFinanciero/Supervisor/
/// Auditor/Administrador. Editar (registrar gastos, anular, cerrar
/// períodos) = solo SuperAdministrador y AdministradorFinanciero — se
/// valida en cada acción con [Authorize] específico, nunca solo ocultando
/// botones en la vista.
/// </summary>
[Authorize(Roles = RolesSistema.FinanzasVer)]
[Route("Finanzas")]
public class AdminFinanzasController : Controller
{
    private readonly IFinanzasDashboardService _dashboardService;
    private readonly IIngresoFinancieroService _ingresoService;
    private readonly IGastoFinancieroService _gastoService;
    private readonly ICategoriaFinancieraService _categoriaService;
    private readonly IPeriodoFinancieroService _periodoService;
    private readonly IAuditoriaFinancieraService _auditoriaService;

    public AdminFinanzasController(
        IFinanzasDashboardService dashboardService,
        IIngresoFinancieroService ingresoService,
        IGastoFinancieroService gastoService,
        ICategoriaFinancieraService categoriaService,
        IPeriodoFinancieroService periodoService,
        IAuditoriaFinancieraService auditoriaService)
    {
        _dashboardService = dashboardService;
        _ingresoService = ingresoService;
        _gastoService = gastoService;
        _categoriaService = categoriaService;
        _periodoService = periodoService;
        _auditoriaService = auditoriaService;
    }

    private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    private string? Ip => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpGet("")]
    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var dashboard = await _dashboardService.ObtenerDashboardAsync();
        return View(dashboard);
    }

    [HttpPost("ExportarPdf")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportarPdf(FinanzasExportOpcionesDto opciones)
    {
        var pdf = await _dashboardService.GenerarReportePdfAsync(opciones);
        var nombreArchivo = $"reporte-finanzas-{DateTime.Now:yyyyMMdd-HHmm}.pdf";
        return File(pdf, "application/pdf", nombreArchivo);
    }

    [HttpGet("Ingresos")]
    public async Task<IActionResult> Ingresos([FromQuery] FiltroReporteFinancieroDto filtro)
    {
        var ingresos = await _ingresoService.ListarAsync(filtro);
        return View(ingresos);
    }

    [Authorize(Roles = RolesSistema.FinanzasEditar)]
    [HttpPost("Ingresos/Anular/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnularIngreso(int id, AnularMovimientoDto modelo)
    {
        try
        {
            await _ingresoService.AnularAsync(id, modelo.Motivo, UsuarioActualId, Ip);
            TempData["Exito"] = "Ingreso anulado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Ingresos));
    }

    [HttpGet("Gastos")]
    public async Task<IActionResult> Gastos([FromQuery] FiltroReporteFinancieroDto filtro)
    {
        ViewBag.Categorias = await _categoriaService.ListarAsync(TipoCategoriaFinanciera.Gasto);
        var gastos = await _gastoService.ListarAsync(filtro);
        return View(gastos);
    }

    [Authorize(Roles = RolesSistema.FinanzasEditar)]
    [HttpGet("Gastos/Nuevo")]
    public async Task<IActionResult> NuevoGasto()
    {
        ViewBag.Categorias = await _categoriaService.ListarAsync(TipoCategoriaFinanciera.Gasto);
        return View(new GastoFinancieroFormDto());
    }

    [Authorize(Roles = RolesSistema.FinanzasEditar)]
    [HttpPost("Gastos/Nuevo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NuevoGasto(GastoFinancieroFormDto modelo, IFormFile? comprobante)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _categoriaService.ListarAsync(TipoCategoriaFinanciera.Gasto);
            return View(modelo);
        }

        try
        {
            await _gastoService.RegistrarAsync(modelo, UsuarioActualId, comprobante, Ip);
            TempData["Exito"] = "Gasto registrado.";
            return RedirectToAction(nameof(Gastos));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Categorias = await _categoriaService.ListarAsync(TipoCategoriaFinanciera.Gasto);
            return View(modelo);
        }
    }

    [Authorize(Roles = RolesSistema.FinanzasEditar)]
    [HttpPost("Gastos/Anular/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnularGasto(int id, AnularMovimientoDto modelo)
    {
        try
        {
            await _gastoService.AnularAsync(id, modelo.Motivo, UsuarioActualId, Ip);
            TempData["Exito"] = "Gasto anulado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Gastos));
    }

    [HttpGet("Categorias")]
    public async Task<IActionResult> Categorias()
    {
        var categorias = await _categoriaService.ListarAsync();
        return View(categorias);
    }

    [Authorize(Roles = RolesSistema.FinanzasEditar)]
    [HttpPost("Categorias/Nueva")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NuevaCategoria(CategoriaFinancieraFormDto modelo)
    {
        try
        {
            await _categoriaService.CrearAsync(modelo);
            TempData["Exito"] = "Categoría creada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Categorias));
    }

    [Authorize(Roles = RolesSistema.FinanzasEditar)]
    [HttpPost("Categorias/AlternarActiva/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarCategoriaActiva(int id)
    {
        await _categoriaService.DesactivarAsync(id);
        return RedirectToAction(nameof(Categorias));
    }

    [HttpGet("Periodos")]
    public async Task<IActionResult> Periodos()
    {
        var periodos = await _periodoService.ListarAsync();
        return View(periodos);
    }

    [Authorize(Roles = RolesSistema.FinanzasEditar)]
    [HttpPost("Periodos/Cerrar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarPeriodo(int id)
    {
        await _periodoService.CerrarPeriodoAsync(id, UsuarioActualId, Ip);
        TempData["Exito"] = "Período cerrado. A partir de ahora, cualquier corrección ahí dentro requiere un ajuste autorizado.";
        return RedirectToAction(nameof(Periodos));
    }

    [HttpGet("EstadoResultados")]
    public async Task<IActionResult> EstadoResultados(DateTime? desde, DateTime? hasta)
    {
        var hoy = DateTime.UtcNow;
        var fechaDesde = desde ?? new DateTime(hoy.Year, hoy.Month, 1);
        var fechaHasta = hasta ?? hoy;

        var resultado = await _dashboardService.ObtenerEstadoResultadosAsync(fechaDesde, fechaHasta.Date.AddDays(1).AddSeconds(-1));
        return View(resultado);
    }

    [HttpGet("BalanceGeneral")]
    public async Task<IActionResult> BalanceGeneral()
    {
        var balance = await _dashboardService.ObtenerBalanceGeneralAsync();
        return View(balance);
    }

    [HttpGet("Auditoria")]
    public async Task<IActionResult> Auditoria()
    {
        var registros = await _auditoriaService.ObtenerRecientesAsync();
        return View(registros);
    }
}
