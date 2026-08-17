using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Repositories;

/// <inheritdoc cref="IFavoritoRepository"/>
public class FavoritoRepository : IFavoritoRepository
{
    private readonly ApplicationDbContext _contexto;

    public FavoritoRepository(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<bool> ExisteAsync(int candidatoId, int vacanteId) =>
        await _contexto.Favoritos.AnyAsync(f => f.CandidatoId == candidatoId && f.VacanteId == vacanteId);

    public async Task<IEnumerable<Favorito>> ObtenerPorCandidatoAsync(int candidatoId) =>
        await _contexto.Favoritos
            .Include(f => f.Vacante).ThenInclude(v => v!.Empresa)
            .Include(f => f.Vacante).ThenInclude(v => v!.Ubicacion)
            .Where(f => f.CandidatoId == candidatoId)
            .ToListAsync();

    public async Task AgregarAsync(Favorito favorito) => await _contexto.Favoritos.AddAsync(favorito);

    public async Task QuitarAsync(int candidatoId, int vacanteId)
    {
        var favorito = await _contexto.Favoritos
            .FirstOrDefaultAsync(f => f.CandidatoId == candidatoId && f.VacanteId == vacanteId);

        if (favorito is not null)
        {
            _contexto.Favoritos.Remove(favorito);
        }
    }

    public async Task<int> GuardarCambiosAsync() => await _contexto.SaveChangesAsync();
}
