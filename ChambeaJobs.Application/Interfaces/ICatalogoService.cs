using ChambeaJobs.Application.DTOs;

namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Consultas de solo lectura a los catálogos (Categorías, Ubicaciones)
/// usadas para poblar &lt;select&gt; en varios formularios (Registro Empresa,
/// Publicar/Editar Vacante, Buscar Vacantes).
/// </summary>
public interface ICatalogoService
{
    Task<List<UbicacionOptionDto>> ObtenerUbicacionesAsync();
    Task<List<CategoriaOptionDto>> ObtenerCategoriasAsync();

    /// <summary>Carreras (subcategorías) dentro de una Categoría específica — para el desplegable dependiente en "Educación" del perfil de Candidato.</summary>
    Task<List<CarreraOptionDto>> ObtenerCarrerasPorCategoriaAsync(int categoriaId);

    /// <summary>Universidades e INATEC, para el paso 1 del catálogo en "Educación".</summary>
    Task<List<InstitucionOptionDto>> ObtenerInstitucionesAsync();

    /// <summary>Facultades/áreas de una institución — paso 2 del catálogo (Institución → Facultad → Carrera).</summary>
    Task<List<FacultadOptionDto>> ObtenerFacultadesPorInstitucionAsync(int institucionId);

    /// <summary>Carreras de una facultad específica — paso 3 del catálogo, ya filtradas a lo que esa institución realmente ofrece.</summary>
    Task<List<CarreraDeInstitucionDto>> ObtenerCarrerasPorFacultadAsync(int facultadId);

    /// <summary>Todas las carreras del catálogo, agrupadas por categoría — para cuando el candidato elige "Otra institución" (no hay institución que filtre).</summary>
    Task<List<CarreraDeInstitucionDto>> ObtenerTodasLasCarrerasAgrupadasAsync();
}
