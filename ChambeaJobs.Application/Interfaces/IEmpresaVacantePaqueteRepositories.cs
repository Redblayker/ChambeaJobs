using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Interfaces;

public interface IEmpresaRepository : IRepository<Empresa>
{
    Task<Empresa?> ObtenerPorUsuarioIdAsync(string usuarioId, bool incluirDetalle = true);
    Task<bool> ExisteParaUsuarioAsync(string usuarioId);
    Task<bool> ExisteRucAsync(string ruc);

    /// <summary>Empresa con Ubicación, Logo, Galería y Vacantes cargados, para el perfil público.</summary>
    Task<Empresa?> ObtenerConGaleriaAsync(int empresaId);

    /// <summary>Todas las empresas con Ubicación/Logo/Vacantes cargados — usado para el feed de "Empresas asociadas" del Candidato.</summary>
    Task<List<Empresa>> ObtenerTodasConDetalleAsync();
}

/// <summary>
/// El repositorio genérico no trae relaciones M:N cargadas (no hace
/// Include) — este método específico sí trae Carreras, para el
/// desplegable dependiente "Carrera según la Institución elegida".
/// </summary>
public interface IInstitucionRepository : IRepository<Institucion>
{
    /// <summary>Facultades/áreas de una institución — paso 2 del catálogo (Institución → Facultad → Carrera).</summary>
    Task<List<Facultad>> ObtenerFacultadesAsync(int institucionId);

    /// <summary>Carreras de una facultad específica — paso 3 del catálogo.</summary>
    Task<List<Carrera>> ObtenerCarrerasPorFacultadAsync(int facultadId);
}

public interface IVacanteRepository : IRepository<Vacante>
{
    Task<Vacante?> ObtenerConDetalleAsync(int id);
    Task<IEnumerable<Vacante>> ObtenerPorEmpresaAsync(int empresaId);
    Task<IEnumerable<Vacante>> ObtenerTodasConDetalleAsync();
    Task<IEnumerable<Vacante>> BuscarAsync(string? palabraClave, int? categoriaId, int? ubicacionId, string? modalidad);
}

public interface IPaqueteEmpresaRepository : IRepository<PaqueteEmpresa>
{
    Task<PaqueteEmpresa?> ObtenerVigentePorEmpresaAsync(int empresaId);
    Task<IEnumerable<PaqueteEmpresa>> ObtenerPorEmpresaAsync(int empresaId);
    Task<IEnumerable<PaqueteEmpresa>> ObtenerPendientesDeAprobacionAsync();

    /// <summary>Todos los paquetes que ya tienen un Pago asociado, con Empresa incluida (para el historial de Admin).</summary>
    Task<IEnumerable<PaqueteEmpresa>> ObtenerTodosConDetalleAsync();
}
