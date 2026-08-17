using ChambeaJobs.Application.DTOs;

namespace ChambeaJobs.Application.Interfaces;

public interface IFavoritoService
{
    Task<bool> EsFavoritoAsync(int candidatoId, int vacanteId);
    Task ToggleFavoritoAsync(int candidatoId, int vacanteId);
    Task<List<FavoritoDto>> ObtenerFavoritosAsync(int candidatoId);
}
