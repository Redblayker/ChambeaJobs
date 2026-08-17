using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Repositories;

/// <inheritdoc cref="ICandidatoRepository"/>
public class CandidatoRepository : GenericRepository<Candidato>, ICandidatoRepository
{
    public CandidatoRepository(ApplicationDbContext contexto) : base(contexto)
    {
    }

    public async Task<Candidato?> ObtenerPorUsuarioIdAsync(string usuarioId, bool incluirDetalle = true)
    {
        if (!incluirDetalle)
        {
            return await ConjuntoDatos.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
        }

        return await ConjuntoDatos
            .Include(c => c.Experiencias)
            .Include(c => c.Educaciones).ThenInclude(e => e.Categoria)
            .Include(c => c.Educaciones).ThenInclude(e => e.Carrera)
            .Include(c => c.Habilidades).ThenInclude(ch => ch.Habilidad)
            .Include(c => c.Idiomas).ThenInclude(ci => ci.Idioma)
            .Include(c => c.Certificados).ThenInclude(cert => cert.Archivo)
            .Include(c => c.Cursos)
            .Include(c => c.CvArchivo)
            .Include(c => c.VideoCvArchivo)
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
    }

    public async Task<Candidato?> ObtenerConDetallePorIdAsync(int candidatoId) =>
        await ConjuntoDatos
            .Include(c => c.Experiencias)
            .Include(c => c.Educaciones).ThenInclude(e => e.Categoria)
            .Include(c => c.Educaciones).ThenInclude(e => e.Carrera)
            .Include(c => c.Habilidades).ThenInclude(ch => ch.Habilidad)
            .Include(c => c.Idiomas).ThenInclude(ci => ci.Idioma)
            .Include(c => c.Certificados).ThenInclude(cert => cert.Archivo)
            .Include(c => c.Cursos)
            .Include(c => c.CvArchivo)
            .Include(c => c.VideoCvArchivo)
            .FirstOrDefaultAsync(c => c.Id == candidatoId);

    public async Task<bool> ExisteParaUsuarioAsync(string usuarioId) =>
        await ConjuntoDatos.AnyAsync(c => c.UsuarioId == usuarioId);
}
