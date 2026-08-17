using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Consultas propias de Candidato que no aplican al repositorio genérico,
/// principalmente cargar el perfil completo con sus colecciones (educación,
/// experiencia, habilidades) sin forzar N+1 queries desde el controlador.
/// </summary>
public interface ICandidatoRepository : IRepository<Candidato>
{
    Task<Candidato?> ObtenerPorUsuarioIdAsync(string usuarioId, bool incluirDetalle = true);
    Task<Candidato?> ObtenerConDetallePorIdAsync(int candidatoId);
    Task<bool> ExisteParaUsuarioAsync(string usuarioId);
}
