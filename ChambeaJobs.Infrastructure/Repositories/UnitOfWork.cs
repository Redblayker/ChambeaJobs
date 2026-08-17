using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Data;

namespace ChambeaJobs.Infrastructure.Repositories;

/// <inheritdoc cref="IUnitOfWork"/>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _contexto;

    public UnitOfWork(ApplicationDbContext contexto)
    {
        _contexto = contexto;
        Candidatos = new CandidatoRepository(_contexto);
        Experiencias = new GenericRepository<ExperienciaLaboral>(_contexto);
        Educaciones = new GenericRepository<Educacion>(_contexto);
        Habilidades = new GenericRepository<Habilidad>(_contexto);
        CandidatoHabilidades = new GenericRepository<CandidatoHabilidad>(_contexto);
        Archivos = new GenericRepository<Archivo>(_contexto);

        Empresas = new EmpresaRepository(_contexto);
        Vacantes = new VacanteRepository(_contexto);
        Pasantias = new PasantiaRepository(_contexto);
        PaquetesEmpresa = new PaqueteEmpresaRepository(_contexto);
        PlanesSuscripcion = new GenericRepository<PlanSuscripcion>(_contexto);
        Pagos = new GenericRepository<Pago>(_contexto);
        Categorias = new GenericRepository<Categoria>(_contexto);
        Carreras = new GenericRepository<Carrera>(_contexto);
        Instituciones = new InstitucionRepository(_contexto);
        Ubicaciones = new GenericRepository<Ubicacion>(_contexto);
        ConfiguracionesSistema = new GenericRepository<ConfiguracionSistema>(_contexto);

        Postulaciones = new PostulacionRepository(_contexto);
        PostulacionesPasantia = new PostulacionPasantiaRepository(_contexto);
        Favoritos = new FavoritoRepository(_contexto);
        EstadosPostulacion = new GenericRepository<EstadoPostulacion>(_contexto);

        Auditorias = new GenericRepository<Auditoria>(_contexto);

        Notificaciones = new NotificacionRepository(_contexto);
        TicketsSoporte = new GenericRepository<TicketSoporte>(_contexto);
        MensajesSoporte = new GenericRepository<MensajeSoporte>(_contexto);
        Idiomas = new GenericRepository<Idioma>(_contexto);
        CandidatoIdiomas = new GenericRepository<CandidatoIdioma>(_contexto);
        Certificados = new GenericRepository<Certificado>(_contexto);
        Cursos = new GenericRepository<Curso>(_contexto);
        ComentariosVideoCv = new GenericRepository<ComentarioVideoCv>(_contexto);
        EmpresaGalerias = new GenericRepository<EmpresaGaleria>(_contexto);

        Evaluaciones = new EvaluacionRepository(_contexto);
        PreguntasPsicometricas = new GenericRepository<PreguntaPsicometrica>(_contexto);
        RespuestasPsicometricas = new GenericRepository<RespuestaPsicometrica>(_contexto);
    }

    public ICandidatoRepository Candidatos { get; }
    public IRepository<ExperienciaLaboral> Experiencias { get; }
    public IRepository<Educacion> Educaciones { get; }
    public IRepository<Habilidad> Habilidades { get; }
    public IRepository<CandidatoHabilidad> CandidatoHabilidades { get; }
    public IRepository<Archivo> Archivos { get; }

    public IEmpresaRepository Empresas { get; }
    public IVacanteRepository Vacantes { get; }
    public IPasantiaRepository Pasantias { get; }
    public IPaqueteEmpresaRepository PaquetesEmpresa { get; }
    public IRepository<PlanSuscripcion> PlanesSuscripcion { get; }
    public IRepository<Pago> Pagos { get; }
    public IRepository<Categoria> Categorias { get; }
    public IRepository<Carrera> Carreras { get; }
    public IInstitucionRepository Instituciones { get; }
    public IRepository<Ubicacion> Ubicaciones { get; }
    public IRepository<ConfiguracionSistema> ConfiguracionesSistema { get; }

    public IPostulacionRepository Postulaciones { get; }
    public IPostulacionPasantiaRepository PostulacionesPasantia { get; }
    public IFavoritoRepository Favoritos { get; }
    public IRepository<EstadoPostulacion> EstadosPostulacion { get; }

    public IRepository<Auditoria> Auditorias { get; }

    public INotificacionRepository Notificaciones { get; }
    public IRepository<TicketSoporte> TicketsSoporte { get; }
    public IRepository<MensajeSoporte> MensajesSoporte { get; }
    public IRepository<Idioma> Idiomas { get; }
    public IRepository<CandidatoIdioma> CandidatoIdiomas { get; }
    public IRepository<Certificado> Certificados { get; }
    public IRepository<Curso> Cursos { get; }
    public IRepository<ComentarioVideoCv> ComentariosVideoCv { get; }
    public IRepository<EmpresaGaleria> EmpresaGalerias { get; }

    public IEvaluacionRepository Evaluaciones { get; }
    public IRepository<PreguntaPsicometrica> PreguntasPsicometricas { get; }
    public IRepository<RespuestaPsicometrica> RespuestasPsicometricas { get; }

    public async Task<int> GuardarCambiosAsync() => await _contexto.SaveChangesAsync();
}
