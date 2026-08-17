using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Interfaces;

public interface IEvaluacionRepository : IRepository<EvaluacionPsicometrica>
{
    Task<EvaluacionPsicometrica?> ObtenerPorPostulacionAsync(int postulacionId);
    Task<EvaluacionPsicometrica?> ObtenerConDetalleAsync(int evaluacionId);
    Task<bool> ExisteParaPostulacionAsync(int postulacionId);
}
