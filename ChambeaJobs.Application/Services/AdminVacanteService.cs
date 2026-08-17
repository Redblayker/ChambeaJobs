using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="IAdminVacanteService"/>
public class AdminVacanteService : IAdminVacanteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoriaService;

    public AdminVacanteService(IUnitOfWork unitOfWork, IAuditoriaService auditoriaService)
    {
        _unitOfWork = unitOfWork;
        _auditoriaService = auditoriaService;
    }

    public async Task<List<VacanteAdminDto>> ObtenerTodasAsync()
    {
        var vacantes = await _unitOfWork.Vacantes.ObtenerTodasConDetalleAsync();

        return vacantes
            .OrderByDescending(v => v.FechaPublicacion)
            .Select(v => new VacanteAdminDto
            {
                Id = v.Id,
                Titulo = v.Titulo,
                EmpresaNombre = v.Empresa?.NombreEmpresa ?? string.Empty,
                CategoriaNombre = v.Categoria?.Nombre ?? string.Empty,
                Estado = v.Estado,
                FechaPublicacion = v.FechaPublicacion
            })
            .ToList();
    }

    public async Task DespublicarAsync(int vacanteId, string adminId)
    {
        var vacante = await _unitOfWork.Vacantes.ObtenerPorIdAsync(vacanteId)
            ?? throw new InvalidOperationException("Esta vacante no existe.");

        vacante.Estado = EstadosVacante.Cerrada;
        _unitOfWork.Vacantes.Actualizar(vacante);
        await _unitOfWork.GuardarCambiosAsync();

        await _auditoriaService.RegistrarAsync(adminId, "Despublicar", "Vacante", vacanteId.ToString());
    }

    public async Task EliminarAsync(int vacanteId, string adminId)
    {
        var vacante = await _unitOfWork.Vacantes.ObtenerPorIdAsync(vacanteId)
            ?? throw new InvalidOperationException("Esta vacante no existe.");

        _unitOfWork.Vacantes.Eliminar(vacante);
        await _unitOfWork.GuardarCambiosAsync();

        await _auditoriaService.RegistrarAsync(adminId, "Eliminar", "Vacante", vacanteId.ToString());
    }
}
