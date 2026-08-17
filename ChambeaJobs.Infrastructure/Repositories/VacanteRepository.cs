using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Repositories;

/// <inheritdoc cref="IVacanteRepository"/>
public class VacanteRepository : GenericRepository<Vacante>, IVacanteRepository
{
    public VacanteRepository(ApplicationDbContext contexto) : base(contexto)
    {
    }

    public async Task<Vacante?> ObtenerConDetalleAsync(int id) =>
        await ConjuntoDatos
            .Include(v => v.Empresa).ThenInclude(e => e!.LogoArchivo)
            .Include(v => v.Categoria)
            .Include(v => v.Carrera)
            .Include(v => v.Ubicacion)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<IEnumerable<Vacante>> ObtenerPorEmpresaAsync(int empresaId) =>
        await ConjuntoDatos
            .Include(v => v.Categoria)
            .Include(v => v.Carrera)
            .Where(v => v.EmpresaId == empresaId)
            .ToListAsync();

    public async Task<IEnumerable<Vacante>> ObtenerTodasConDetalleAsync() =>
        await ConjuntoDatos
            .Include(v => v.Empresa)
            .Include(v => v.Categoria)
            .Include(v => v.Carrera)
            .ToListAsync();

    public async Task<IEnumerable<Vacante>> BuscarAsync(string? palabraClave, int? categoriaId, int? ubicacionId, string? modalidad)
    {
        var query = ConjuntoDatos
            .Include(v => v.Empresa).ThenInclude(e => e!.LogoArchivo)
            .Include(v => v.Categoria)
            .Include(v => v.Carrera)
            .Include(v => v.Ubicacion)
            .Where(v => v.Estado == EstadosVacante.Activa);

        if (!string.IsNullOrWhiteSpace(palabraClave))
        {
            query = query.Where(v => v.Titulo.Contains(palabraClave) || v.Descripcion.Contains(palabraClave));
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(v => v.CategoriaId == categoriaId.Value);
        }

        if (ubicacionId.HasValue)
        {
            query = query.Where(v => v.UbicacionId == ubicacionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(modalidad))
        {
            query = query.Where(v => v.Modalidad == modalidad);
        }

        return await query
            .OrderByDescending(v => v.EsDestacada)
            .ThenByDescending(v => v.FechaPublicacion)
            .ToListAsync();
    }
}
