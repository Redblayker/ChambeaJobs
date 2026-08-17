using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Repositories;

/// <inheritdoc cref="IPasantiaRepository"/>
public class PasantiaRepository : GenericRepository<Pasantia>, IPasantiaRepository
{
    public PasantiaRepository(ApplicationDbContext contexto) : base(contexto)
    {
    }

    public async Task<Pasantia?> ObtenerConDetalleAsync(int id) =>
        await ConjuntoDatos
            .Include(p => p.Empresa).ThenInclude(e => e!.LogoArchivo)
            .Include(p => p.Categoria)
            .Include(p => p.Ubicacion)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Pasantia>> ObtenerPorEmpresaAsync(int empresaId) =>
        await ConjuntoDatos
            .Include(p => p.Categoria)
            .Where(p => p.EmpresaId == empresaId)
            .ToListAsync();

    public async Task<IEnumerable<Pasantia>> ObtenerTodasConDetalleAsync() =>
        await ConjuntoDatos
            .Include(p => p.Empresa)
            .Include(p => p.Categoria)
            .ToListAsync();

    public async Task<IEnumerable<Pasantia>> BuscarAsync(string? palabraClave, int? categoriaId, int? ubicacionId, string? modalidad)
    {
        var query = ConjuntoDatos
            .Include(p => p.Empresa).ThenInclude(e => e!.LogoArchivo)
            .Include(p => p.Categoria)
            .Include(p => p.Ubicacion)
            .Where(p => p.Estado == EstadosPasantia.Activa);

        if (!string.IsNullOrWhiteSpace(palabraClave))
        {
            query = query.Where(p => p.Titulo.Contains(palabraClave) || p.Descripcion.Contains(palabraClave));
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        if (ubicacionId.HasValue)
        {
            query = query.Where(p => p.UbicacionId == ubicacionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(modalidad))
        {
            query = query.Where(p => p.Modalidad == modalidad);
        }

        return await query.OrderByDescending(p => p.FechaPublicacion).ToListAsync();
    }
}
