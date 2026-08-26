using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Repositories;

/// <inheritdoc cref="IEvaluacionRepository"/>
public class EvaluacionRepository : GenericRepository<EvaluacionPsicometrica>, IEvaluacionRepository
{
    public EvaluacionRepository(ApplicationDbContext contexto) : base(contexto)
    {
    }

    public async Task<EvaluacionPsicometrica?> ObtenerPorPostulacionAsync(int postulacionId) =>
        await ConjuntoDatos
            .Include(e => e.Postulacion).ThenInclude(p => p!.Vacante).ThenInclude(v => v!.Empresa)
            .Include(e => e.Postulacion).ThenInclude(p => p!.Candidato)
            .FirstOrDefaultAsync(e => e.PostulacionId == postulacionId);

    public async Task<EvaluacionPsicometrica?> ObtenerConDetalleAsync(int evaluacionId) =>
        await ConjuntoDatos
            .Include(e => e.Postulacion).ThenInclude(p => p!.Vacante).ThenInclude(v => v!.Empresa)
            .Include(e => e.Postulacion).ThenInclude(p => p!.Candidato)
            .Include(e => e.Respuestas)
            .FirstOrDefaultAsync(e => e.Id == evaluacionId);

    public async Task<bool> ExisteParaPostulacionAsync(int postulacionId) =>
        await ConjuntoDatos.AnyAsync(e => e.PostulacionId == postulacionId);


}
