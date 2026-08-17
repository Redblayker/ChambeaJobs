using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="ICatalogoService"/>
public class CatalogoService : ICatalogoService
{
    private readonly IUnitOfWork _unitOfWork;

    public CatalogoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<UbicacionOptionDto>> ObtenerUbicacionesAsync()
    {
        var ubicaciones = await _unitOfWork.Ubicaciones.ObtenerTodosAsync();
        return ubicaciones
            .Select(u => new UbicacionOptionDto { Id = u.Id, NombreCompleto = u.NombreCompleto })
            .OrderBy(u => u.NombreCompleto)
            .ToList();
    }

    public async Task<List<CategoriaOptionDto>> ObtenerCategoriasAsync()
    {
        var categorias = await _unitOfWork.Categorias.ObtenerTodosAsync();
        return categorias
            .Select(c => new CategoriaOptionDto { Id = c.Id, Nombre = c.Nombre })
            .OrderBy(c => c.Nombre)
            .ToList();
    }

    public async Task<List<CarreraOptionDto>> ObtenerCarrerasPorCategoriaAsync(int categoriaId)
    {
        var carreras = await _unitOfWork.Carreras.ObtenerTodosAsync();

        // Con el catálogo de 3 niveles (Institución -> Facultad -> Carrera),
        // el mismo nombre de carrera (ej. "Ingeniería en Sistemas") existe
        // como una fila DISTINTA por cada universidad que la ofrece — eso es
        // correcto para el flujo del Candidato (Institución → Facultad →
        // Carrera, donde sí importa de cuál universidad es). Pero aquí, del
        // lado de la Empresa, solo se pide "qué carrera busca" en general,
        // sin filtrar por universidad — así que hay que deduplicar por
        // nombre, o se ven filas repetidas como en la captura del bug.
        return carreras
            .Where(c => c.CategoriaId == categoriaId)
            .GroupBy(c => c.Nombre.Trim())
            .Select(g => new CarreraOptionDto { Id = g.Min(c => c.Id), CategoriaId = categoriaId, Nombre = g.Key })
            .OrderBy(c => c.Nombre)
            .ToList();
    }

    public async Task<List<InstitucionOptionDto>> ObtenerInstitucionesAsync()
    {
        var instituciones = await _unitOfWork.Instituciones.ObtenerTodosAsync();
        return instituciones
            .Select(i => new InstitucionOptionDto { Id = i.Id, Nombre = i.Nombre, Tipo = i.Tipo })
            // Universidades primero, INATEC (Técnico) al final, y alfabético dentro de cada grupo.
            .OrderBy(i => i.Tipo == "Universidad" ? 0 : 1)
            .ThenBy(i => i.Nombre)
            .ToList();
    }

    public async Task<List<FacultadOptionDto>> ObtenerFacultadesPorInstitucionAsync(int institucionId)
    {
        var facultades = await _unitOfWork.Instituciones.ObtenerFacultadesAsync(institucionId);
        return facultades
            .Select(f => new FacultadOptionDto { Id = f.Id, InstitucionId = f.InstitucionId, Nombre = f.Nombre })
            .ToList();
    }

    public async Task<List<CarreraDeInstitucionDto>> ObtenerCarrerasPorFacultadAsync(int facultadId)
    {
        var carreras = await _unitOfWork.Instituciones.ObtenerCarrerasPorFacultadAsync(facultadId);
        return carreras
            .Select(c => new CarreraDeInstitucionDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                CategoriaId = c.CategoriaId,
                CategoriaNombre = c.Categoria?.Nombre ?? string.Empty
            })
            .ToList();
    }

    public async Task<List<CarreraDeInstitucionDto>> ObtenerTodasLasCarrerasAgrupadasAsync()
    {
        var carreras = await _unitOfWork.Carreras.ObtenerTodosAsync();
        var categorias = (await _unitOfWork.Categorias.ObtenerTodosAsync()).ToDictionary(c => c.Id, c => c.Nombre);

        // Mismo motivo que ObtenerCarrerasPorCategoriaAsync: si el candidato
        // eligió "Otra institución" (no hay universidad que filtre), no
        // tiene sentido mostrarle "Ingeniería en Sistemas" repetida una vez
        // por cada universidad que la ofrece — se deduplica por nombre.
        return carreras
            .GroupBy(c => c.Nombre.Trim())
            .Select(g =>
            {
                var primero = g.First();
                return new CarreraDeInstitucionDto
                {
                    Id = g.Min(c => c.Id),
                    Nombre = g.Key,
                    CategoriaId = primero.CategoriaId,
                    CategoriaNombre = categorias.TryGetValue(primero.CategoriaId, out var nombre) ? nombre : string.Empty
                };
            })
            .OrderBy(c => c.CategoriaNombre)
            .ThenBy(c => c.Nombre)
            .ToList();
    }
}
