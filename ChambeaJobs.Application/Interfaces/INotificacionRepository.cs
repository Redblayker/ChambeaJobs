using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Interfaces;

public interface INotificacionRepository : IRepository<Notificacion>
{
    Task<List<Notificacion>> ObtenerPorUsuarioAsync(string usuarioId, int cantidad = 30);
    Task<int> ContarNoLeidasAsync(string usuarioId);
    Task MarcarTodasComoLeidasAsync(string usuarioId);
    Task EliminarTodasAsync(string usuarioId);
}
