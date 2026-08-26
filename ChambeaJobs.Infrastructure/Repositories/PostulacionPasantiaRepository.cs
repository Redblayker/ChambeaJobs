using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Repositories;

/// <inheritdoc cref="IPostulacionPasantiaRepository"/>
public class PostulacionPasantiaRepository : GenericRepository<PostulacionPasantia>, IPostulacionPasantiaRepository
{
    public PostulacionPasantiaRepository(ApplicationDbContext contexto) : base(contexto)
    {
    }

    public async Task<bool> ExistePostulacionAsync(int candidatoId, int pasantiaId) =>
        await ConjuntoDatos.AnyAsync(p => p.CandidatoId == candidatoId && p.PasantiaId == pasantiaId);

    public async Task<IEnumerable<PostulacionPasantia>> ObtenerPorCandidatoAsync(int candidatoId) =>
        await ConjuntoDatos
            .Include(p => p.Pasantia).ThenInclude(pa => pa!.Empresa)
            .Where(p => p.CandidatoId == candidatoId)
            .ToListAsync();

    public async Task<IEnumerable<PostulacionPasantia>> ObtenerPorPasantiaAsync(int pasantiaId) =>
        await ConjuntoDatos
            .Include(p => p.Candidato).ThenInclude(c => c!.CvArchivo)
            .Where(p => p.PasantiaId == pasantiaId)
            .ToListAsync();

    public async Task<PostulacionPasantia?> ObtenerConDetalleAsync(int id) =>
        await ConjuntoDatos
            .Include(p => p.Pasantia).ThenInclude(pa => pa!.Empresa)
            .Include(p => p.Candidato)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Dictionary<int, int>> ContarPorPasantiasAsync(IEnumerable<int> pasantiaIds)
    {
        var ids = pasantiaIds.Distinct().ToList();

        var counts = await ConjuntoDatos
            .Where(p => ids.Contains(p.PasantiaId))
            .GroupBy(p => p.PasantiaId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count);

        // Asegura que todos los ids solicitados aparezcan en el diccionario (0 si no hay postulaciones)
        return ids.ToDictionary(id => id, id => counts.TryGetValue(id, out var c) ? c : 0);
    }
    public async Task<Dictionary<int, int>> ObtenerPorPasantiaAsync(IEnumerable<int> pasantiaIds)
    {
        var ids = pasantiaIds.Distinct().ToList();

        var counts = await ConjuntoDatos
            .Where(p => ids.Contains(p.PasantiaId))
            .GroupBy(p => p.PasantiaId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count);

        // Asegura que todos los ids solicitados aparezcan en el diccionario (0 si no hay postulaciones)
        return ids.ToDictionary(id => id, id => counts.TryGetValue(id, out var c) ? c : 0);
    }
}
