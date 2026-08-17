using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Repositories;

/// <inheritdoc cref="IPostulacionRepository"/>
public class PostulacionRepository : GenericRepository<Postulacion>, IPostulacionRepository
{
    public PostulacionRepository(ApplicationDbContext contexto) : base(contexto)
    {
    }

    public async Task<bool> ExistePostulacionAsync(int candidatoId, int vacanteId) =>
        await ConjuntoDatos.AnyAsync(p => p.CandidatoId == candidatoId && p.VacanteId == vacanteId);

    public async Task<IEnumerable<Postulacion>> ObtenerPorCandidatoAsync(int candidatoId) =>
        await ConjuntoDatos
            .Include(p => p.Vacante).ThenInclude(v => v!.Empresa).ThenInclude(e => e!.LogoArchivo)
            .Include(p => p.EstadoPostulacion)
            .Where(p => p.CandidatoId == candidatoId)
            .ToListAsync();

    public async Task<IEnumerable<Postulacion>> ObtenerPorVacanteAsync(int vacanteId) =>
        await ConjuntoDatos
            .Include(p => p.Candidato).ThenInclude(c => c!.CvArchivo)
            .Include(p => p.Candidato).ThenInclude(c => c!.VideoCvArchivo)
            .Include(p => p.Candidato).ThenInclude(c => c!.Habilidades).ThenInclude(ch => ch.Habilidad)
            .Include(p => p.Candidato).ThenInclude(c => c!.Idiomas).ThenInclude(ci => ci.Idioma)
            .Include(p => p.Candidato).ThenInclude(c => c!.Experiencias)
            .Include(p => p.EstadoPostulacion)
            .Where(p => p.VacanteId == vacanteId)
            .ToListAsync();

    public async Task<Postulacion?> ObtenerConDetalleAsync(int id) =>
        await ConjuntoDatos
            .Include(p => p.Vacante).ThenInclude(v => v!.Empresa)
            .Include(p => p.Candidato).ThenInclude(c => c!.VideoCvArchivo)
            .Include(p => p.Candidato).ThenInclude(c => c!.CvArchivo)
            .Include(p => p.EstadoPostulacion)
            .Include(p => p.ComentariosVideoCv)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Postulacion>> ObtenerPorEmpresaAsync(int empresaId) =>
        await ConjuntoDatos
            .Include(p => p.Candidato)
            .Include(p => p.Vacante)
            .Include(p => p.EstadoPostulacion)
            .Where(p => p.Vacante!.EmpresaId == empresaId)
            .ToListAsync();
}
