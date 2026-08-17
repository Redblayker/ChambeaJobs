using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Repositories;

/// <inheritdoc cref="INotificacionRepository"/>
public class NotificacionRepository : GenericRepository<Notificacion>, INotificacionRepository
{
    public NotificacionRepository(ApplicationDbContext contexto) : base(contexto)
    {
    }

    public async Task<List<Notificacion>> ObtenerPorUsuarioAsync(string usuarioId, int cantidad = 30) =>
        await ConjuntoDatos
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.FechaCreacion)
            .Take(cantidad)
            .ToListAsync();

    public async Task<int> ContarNoLeidasAsync(string usuarioId) =>
        await ConjuntoDatos.CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);

    public async Task MarcarTodasComoLeidasAsync(string usuarioId)
    {
        var pendientes = await ConjuntoDatos
            .Where(n => n.UsuarioId == usuarioId && !n.Leida)
            .ToListAsync();

        foreach (var n in pendientes)
        {
            n.Leida = true;
        }

        await Contexto.SaveChangesAsync();
    }

    public async Task EliminarTodasAsync(string usuarioId)
    {
        var propias = await ConjuntoDatos.Where(n => n.UsuarioId == usuarioId).ToListAsync();
        ConjuntoDatos.RemoveRange(propias);
        await Contexto.SaveChangesAsync();
    }
}
