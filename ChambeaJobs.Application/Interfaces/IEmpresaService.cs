using ChambeaJobs.Application.DTOs;

namespace ChambeaJobs.Application.Interfaces;

public interface IEmpresaService
{
    Task CrearPerfilInicialAsync(string usuarioId, string nombreEmpresa, string ruc, int ubicacionId, int planSuscripcionId);
    Task<EmpresaPerfilDto?> ObtenerPerfilPorUsuarioIdAsync(string usuarioId);
    Task ActualizarPerfilAsync(string usuarioId, EditarPerfilEmpresaDto datos);

    /// <summary>
    /// Sube/reemplaza únicamente el logo de la empresa. Se separó de
    /// ActualizarPerfilAsync porque ese método exige otros campos
    /// obligatorios (nombre, ubicación) que no tienen sentido reenviar solo
    /// para cambiar el logo desde la página de "Mi Empresa" (perfil extendido).
    /// </summary>
    Task ActualizarLogoAsync(string usuarioId, Microsoft.AspNetCore.Http.IFormFile logo);
    Task<bool> ExisteRucAsync(string ruc);

    /// <summary>Id interno de Empresa (tabla Empresas) a partir del UsuarioId de Identity.</summary>
    Task<int?> ObtenerEmpresaIdPorUsuarioAsync(string usuarioId);

    // ---------- Perfil enriquecido (mejora solicitada) ----------
    Task<EditarPerfilExtendidoEmpresaDto?> ObtenerPerfilExtendidoAsync(string usuarioId);
    Task ActualizarPerfilExtendidoAsync(string usuarioId, EditarPerfilExtendidoEmpresaDto datos);

    /// <summary>Perfil público de la empresa, visible para cualquier visitante (candidatos incluidos).</summary>
    Task<EmpresaPerfilPublicoDto?> ObtenerPerfilPublicoAsync(int empresaId);

    /// <summary>Listado ligero de empresas para el feed de "Empresas asociadas" del Candidato (Home/Index).</summary>
    Task<List<EmpresaResumenAsociadaDto>> ObtenerEmpresasAsociadasAsync();

    Task AgregarFotoGaleriaAsync(string usuarioId, Microsoft.AspNetCore.Http.IFormFile foto, string? titulo);

    /// <summary>
    /// Sube un video como ARCHIVO (no por URL de YouTube/Vimeo — se cambió a
    /// propósito porque pedir una URL de "insertar/embed" resultaba
    /// confuso para las empresas). Mismo patrón que AgregarFotoGaleriaAsync.
    /// </summary>
    Task AgregarVideoGaleriaAsync(string usuarioId, Microsoft.AspNetCore.Http.IFormFile video, string? titulo);
    Task EliminarGaleriaAsync(string usuarioId, int galeriaId);
}
