using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Interfaces;

public interface IPostulacionRepository : IRepository<Postulacion>
{
    Task<bool> ExistePostulacionAsync(int candidatoId, int vacanteId);
    Task<IEnumerable<Postulacion>> ObtenerPorCandidatoAsync(int candidatoId);
    Task<IEnumerable<Postulacion>> ObtenerPorVacanteAsync(int vacanteId);
    Task<Postulacion?> ObtenerConDetalleAsync(int id);

    /// <summary>Todas las postulaciones de todas las vacantes de una empresa (para KPIs del dashboard).</summary>
    Task<IEnumerable<Postulacion>> ObtenerPorEmpresaAsync(int empresaId);

    //Agregado por allan<
    Task<Dictionary<int, int>>ContarPorVacanteAsync(IEnumerable<int> vacanteIds);
}

public interface IFavoritoRepository
{
    Task<bool> ExisteAsync(int candidatoId, int vacanteId);
    Task<IEnumerable<Favorito>> ObtenerPorCandidatoAsync(int candidatoId);
    Task AgregarAsync(Favorito favorito);
    Task QuitarAsync(int candidatoId, int vacanteId);
    Task<int> GuardarCambiosAsync();
}
