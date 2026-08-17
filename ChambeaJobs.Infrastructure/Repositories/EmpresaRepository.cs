using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Repositories;

/// <inheritdoc cref="IEmpresaRepository"/>
public class EmpresaRepository : GenericRepository<Empresa>, IEmpresaRepository
{
    public EmpresaRepository(ApplicationDbContext contexto) : base(contexto)
    {
    }

    public async Task<Empresa?> ObtenerPorUsuarioIdAsync(string usuarioId, bool incluirDetalle = true)
    {
        if (!incluirDetalle)
        {
            return await ConjuntoDatos.FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);
        }

        return await ConjuntoDatos
            .Include(e => e.Ubicacion)
            .Include(e => e.LogoArchivo)
            .Include(e => e.Paquetes).ThenInclude(p => p.Pago)
            .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);
    }

    public async Task<bool> ExisteParaUsuarioAsync(string usuarioId) =>
        await ConjuntoDatos.AnyAsync(e => e.UsuarioId == usuarioId);

    public async Task<bool> ExisteRucAsync(string ruc) =>
        await ConjuntoDatos.AnyAsync(e => e.RUC == ruc);

    public async Task<Empresa?> ObtenerConGaleriaAsync(int empresaId) =>
        await ConjuntoDatos
            .Include(e => e.Ubicacion)
            .Include(e => e.LogoArchivo)
            .Include(e => e.Galeria).ThenInclude(g => g.Archivo)
            .Include(e => e.Vacantes)
            .FirstOrDefaultAsync(e => e.Id == empresaId);

    /// <summary>Todas las empresas con Ubicación/Logo/Vacantes cargados — usado para el feed de "Empresas asociadas" del Candidato.</summary>
    public async Task<List<Empresa>> ObtenerTodasConDetalleAsync() =>
        await ConjuntoDatos
            .Include(e => e.Ubicacion)
            .Include(e => e.LogoArchivo)
            .Include(e => e.Vacantes)
            .OrderBy(e => e.NombreEmpresa)
            .ToListAsync();
}
