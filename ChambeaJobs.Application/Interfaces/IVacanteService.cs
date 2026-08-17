using ChambeaJobs.Application.DTOs;

namespace ChambeaJobs.Application.Interfaces;

public interface IVacanteService
{
    /// <summary>
    /// Publica una vacante. Lanza InvalidOperationException si la empresa
    /// no tiene un paquete vigente con cupo disponible.
    /// </summary>
    Task<int> PublicarVacanteAsync(int empresaId, VacanteFormDto datos);

    Task ActualizarVacanteAsync(int empresaId, VacanteFormDto datos);
    Task CerrarVacanteAsync(int empresaId, int vacanteId);
    Task EliminarVacanteAsync(int empresaId, int vacanteId);

    Task<List<VacanteListItemDto>> ObtenerVacantesDeEmpresaAsync(int empresaId);
    Task<VacanteFormDto?> ObtenerParaEditarAsync(int empresaId, int vacanteId);
    Task<VacanteDetalleDto?> ObtenerDetalleAsync(int vacanteId);

    Task<List<VacanteDetalleDto>> BuscarAsync(string? palabraClave, int? categoriaId, int? ubicacionId, string? modalidad);
}
