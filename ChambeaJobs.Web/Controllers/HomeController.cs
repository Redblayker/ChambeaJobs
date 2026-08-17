using ChambeaJobs.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChambeaJobs.Web.Controllers;

/// <summary>
/// Controlador público: Landing y páginas de error.
/// El resto de pantallas públicas (Contacto, Buscar Vacantes, Detalle Vacante)
/// se agregan en su módulo correspondiente.
/// </summary>
public class HomeController : Controller
{
    private readonly ICatalogoService _catalogoService;
    private readonly IEmpresaService _empresaService;

    public HomeController(ICatalogoService catalogoService, IEmpresaService empresaService)
    {
        _catalogoService = catalogoService;
        _empresaService = empresaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Categorias = await _catalogoService.ObtenerCategoriasAsync();
        ViewBag.Ubicaciones = await _catalogoService.ObtenerUbicacionesAsync();

        // Antes, un Candidato con sesión iniciada veía una pantalla en blanco
        // con solo el logo al entrar a Inicio. Ahora ve el feed de empresas
        // asociadas — el buscador y demás ya viven en la barra lateral, así
        // que Inicio pasa a ser este feed en vez de quedar vacío.
        if (User.Identity?.IsAuthenticated == true && User.IsInRole(ChambeaJobs.Domain.Enums.RolesSistema.Candidato))
        {
            ViewBag.EmpresasAsociadas = await _empresaService.ObtenerEmpresasAsociadasAsync();
        }

        return View();
    }

    [HttpGet]
    [Route("/Home/Error404")]
    public IActionResult Error404() => View();

    [HttpGet]
    [Route("/Home/Error500")]
    public IActionResult Error500() => View();

    [HttpGet]
    public IActionResult Terminos() => View();

    [HttpGet]
    public IActionResult Privacidad() => View();
}
