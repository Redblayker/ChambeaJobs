using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Repositories;

/// <inheritdoc cref="IInstitucionRepository"/>
public class InstitucionRepository : GenericRepository<Institucion>, IInstitucionRepository
{
    private readonly ApplicationDbContext _contexto;

    public InstitucionRepository(ApplicationDbContext contexto) : base(contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<Facultad>> ObtenerFacultadesAsync(int institucionId) =>
        await _contexto.Facultades
            .Where(f => f.InstitucionId == institucionId)
            .OrderBy(f => f.Nombre)
            .ToListAsync();

    public async Task<List<Carrera>> ObtenerCarrerasPorFacultadAsync(int facultadId) =>
        await _contexto.Carreras
            .Include(c => c.Categoria)
            .Where(c => c.FacultadId == facultadId)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
}
