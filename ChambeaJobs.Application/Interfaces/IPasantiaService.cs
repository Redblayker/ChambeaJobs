using ChambeaJobs.Application.DTOs;

namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Módulo de Pasantías, completamente independiente del módulo de Vacantes
/// de empleo (entidades, reglas y pantallas propias en los 3 roles).
/// </summary>
public interface IPasantiaService
{
    Task<int> PublicarPasantiaAsync(int empresaId, PasantiaFormDto datos);
    Task ActualizarPasantiaAsync(int empresaId, PasantiaFormDto datos);
    Task CerrarPasantiaAsync(int empresaId, int pasantiaId);
    Task EliminarPasantiaAsync(int empresaId, int pasantiaId);

    Task<List<PasantiaListItemDto>> ObtenerPasantiasDeEmpresaAsync(int empresaId);
    Task<PasantiaFormDto?> ObtenerParaEditarAsync(int empresaId, int pasantiaId);
    Task<PasantiaDetalleDto?> ObtenerDetalleAsync(int pasantiaId);

    Task<List<PasantiaDetalleDto>> BuscarAsync(string? palabraClave, int? categoriaId, int? ubicacionId, string? modalidad);

    // ---------- Administración ----------
    Task<List<PasantiaAdminDto>> ObtenerTodasAsync();
    Task DespublicarAdminAsync(int pasantiaId, string adminId);
    Task EliminarAdminAsync(int pasantiaId, string adminId);

    // ---------- Postulaciones a pasantías ----------
    Task PostularAsync(int candidatoId, int pasantiaId);
    Task<bool> YaPostuloAsync(int candidatoId, int pasantiaId);
    Task<List<PostulantePasantiaDto>> ObtenerPostulantesAsync(int empresaId, int pasantiaId);
    Task<List<PostulacionPasantiaCandidatoDto>> ObtenerMisPostulacionesAsync(int candidatoId);
    Task CambiarEstadoPostulacionAsync(int empresaId, int postulacionId, string nuevoEstado, string? nota);
}
