using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Unit of Work: agrupa los repositorios que participan en una misma
/// transacción lógica (ej. actualizar Candidato + sus Experiencias +
/// Educaciones en una sola operación de guardado). Evita que cada
/// repositorio llame a SaveChanges por su cuenta.
/// </summary>
public interface IUnitOfWork
{
    ICandidatoRepository Candidatos { get; }
    IRepository<ExperienciaLaboral> Experiencias { get; }
    IRepository<Educacion> Educaciones { get; }
    IRepository<Habilidad> Habilidades { get; }
    IRepository<CandidatoHabilidad> CandidatoHabilidades { get; }
    IRepository<Archivo> Archivos { get; }

    IEmpresaRepository Empresas { get; }
    IVacanteRepository Vacantes { get; }
    IPasantiaRepository Pasantias { get; }
    IPaqueteEmpresaRepository PaquetesEmpresa { get; }
    IRepository<PlanSuscripcion> PlanesSuscripcion { get; }
    IRepository<Pago> Pagos { get; }
    IRepository<Categoria> Categorias { get; }
    IRepository<Carrera> Carreras { get; }
    IInstitucionRepository Instituciones { get; }
    IRepository<Ubicacion> Ubicaciones { get; }
    IRepository<ConfiguracionSistema> ConfiguracionesSistema { get; }

    IPostulacionRepository Postulaciones { get; }
    IPostulacionPasantiaRepository PostulacionesPasantia { get; }
    IFavoritoRepository Favoritos { get; }
    IRepository<EstadoPostulacion> EstadosPostulacion { get; }

    IRepository<Auditoria> Auditorias { get; }

    INotificacionRepository Notificaciones { get; }
    IRepository<TicketSoporte> TicketsSoporte { get; }
    IRepository<MensajeSoporte> MensajesSoporte { get; }
    IRepository<Idioma> Idiomas { get; }
    IRepository<CandidatoIdioma> CandidatoIdiomas { get; }
    IRepository<Certificado> Certificados { get; }
    IRepository<Curso> Cursos { get; }
    IRepository<ComentarioVideoCv> ComentariosVideoCv { get; }
    IRepository<EmpresaGaleria> EmpresaGalerias { get; }

    IEvaluacionRepository Evaluaciones { get; }
    IRepository<PreguntaPsicometrica> PreguntasPsicometricas { get; }
    IRepository<RespuestaPsicometrica> RespuestasPsicometricas { get; }

    Task<int> GuardarCambiosAsync();
}
