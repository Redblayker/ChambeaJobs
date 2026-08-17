using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Repositories;

/// <inheritdoc cref="IPaqueteEmpresaRepository"/>
public class PaqueteEmpresaRepository : GenericRepository<PaqueteEmpresa>, IPaqueteEmpresaRepository
{
    public PaqueteEmpresaRepository(ApplicationDbContext contexto) : base(contexto)
    {
    }

    public async Task<PaqueteEmpresa?> ObtenerVigentePorEmpresaAsync(int empresaId) =>
        await ConjuntoDatos
            .Include(p => p.Pago)
            .Include(p => p.PlanSuscripcion)
            .Where(p => p.EmpresaId == empresaId && p.Estado == EstadosPaquete.Vigente)
            .OrderByDescending(p => p.FechaCompra)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<PaqueteEmpresa>> ObtenerPorEmpresaAsync(int empresaId) =>
        await ConjuntoDatos
            .Include(p => p.Pago)
            .Where(p => p.EmpresaId == empresaId)
            .ToListAsync();

    public async Task<IEnumerable<PaqueteEmpresa>> ObtenerPendientesDeAprobacionAsync() =>
        await ConjuntoDatos
            .Include(p => p.Pago)
            .Include(p => p.Empresa)
            .Where(p => p.Estado == EstadosPaquete.Pendiente)
            .ToListAsync();

    public async Task<IEnumerable<PaqueteEmpresa>> ObtenerTodosConDetalleAsync() =>
        await ConjuntoDatos
            .Include(p => p.Pago)
            .Include(p => p.Empresa)
            .Where(p => p.Pago != null)
            .ToListAsync();
}
