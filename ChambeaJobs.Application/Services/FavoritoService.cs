using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="IFavoritoService"/>
public class FavoritoService : IFavoritoService
{
    private readonly IUnitOfWork _unitOfWork;

    public FavoritoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> EsFavoritoAsync(int candidatoId, int vacanteId) =>
        await _unitOfWork.Favoritos.ExisteAsync(candidatoId, vacanteId);

    public async Task ToggleFavoritoAsync(int candidatoId, int vacanteId)
    {
        if (await _unitOfWork.Favoritos.ExisteAsync(candidatoId, vacanteId))
        {
            await _unitOfWork.Favoritos.QuitarAsync(candidatoId, vacanteId);
        }
        else
        {
            await _unitOfWork.Favoritos.AgregarAsync(new Favorito
            {
                CandidatoId = candidatoId,
                VacanteId = vacanteId,
                FechaAgregado = DateTime.UtcNow
            });
        }
    }

    public async Task<List<FavoritoDto>> ObtenerFavoritosAsync(int candidatoId)
    {
        var favoritos = await _unitOfWork.Favoritos.ObtenerPorCandidatoAsync(candidatoId);

        return favoritos
            .OrderByDescending(f => f.FechaAgregado)
            .Where(f => f.Vacante is not null)
            .Select(f => new FavoritoDto
            {
                VacanteId = f.VacanteId,
                Titulo = f.Vacante!.Titulo,
                EmpresaNombre = f.Vacante.Empresa?.NombreEmpresa ?? string.Empty,
                UbicacionNombre = f.Vacante.Ubicacion?.NombreCompleto ?? string.Empty,
                Modalidad = f.Vacante.Modalidad,
                EstadoVacante = f.Vacante.Estado
            })
            .ToList();
    }
}
