using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos del sistema. Hereda de IdentityDbContext
/// para que EF Core genere y gestione automáticamente AspNetUsers,
/// AspNetRoles, AspNetUserRoles y demás tablas de Identity, en lugar de recrearlas manualmente.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ---------- Módulo Candidato ----------
    public DbSet<Archivo> Archivos => Set<Archivo>();
    public DbSet<Candidato> Candidatos => Set<Candidato>();
    public DbSet<ExperienciaLaboral> ExperienciasLaborales => Set<ExperienciaLaboral>();
    public DbSet<Educacion> Educaciones => Set<Educacion>();
    public DbSet<Habilidad> Habilidades => Set<Habilidad>();
    public DbSet<CandidatoHabilidad> CandidatoHabilidades => Set<CandidatoHabilidad>();
    public DbSet<Idioma> Idiomas => Set<Idioma>();
    public DbSet<CandidatoIdioma> CandidatoIdiomas => Set<CandidatoIdioma>();
    public DbSet<Certificado> Certificados => Set<Certificado>();
    public DbSet<Curso> Cursos => Set<Curso>();

    // ---------- Módulo Empresa / Vacantes / Paquetes ----------
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Carrera> Carreras => Set<Carrera>();
    public DbSet<Institucion> Instituciones => Set<Institucion>();
    public DbSet<Facultad> Facultades => Set<Facultad>();
    public DbSet<Ubicacion> Ubicaciones => Set<Ubicacion>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<EmpresaGaleria> EmpresaGalerias => Set<EmpresaGaleria>();
    public DbSet<Vacante> Vacantes => Set<Vacante>();
    public DbSet<Pasantia> Pasantias => Set<Pasantia>();
    public DbSet<PaqueteEmpresa> PaquetesEmpresa => Set<PaqueteEmpresa>();
    public DbSet<PlanSuscripcion> PlanesSuscripcion => Set<PlanSuscripcion>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<ConfiguracionSistema> ConfiguracionesSistema => Set<ConfiguracionSistema>();

    // ---------- Módulo Postulaciones / Favoritos ----------
    public DbSet<EstadoPostulacion> EstadosPostulacion => Set<EstadoPostulacion>();
    public DbSet<Postulacion> Postulaciones => Set<Postulacion>();
    public DbSet<PostulacionPasantia> PostulacionesPasantia => Set<PostulacionPasantia>();
    public DbSet<Favorito> Favoritos => Set<Favorito>();
    public DbSet<ComentarioVideoCv> ComentariosVideoCv => Set<ComentarioVideoCv>();

    // ---------- Módulo Administrador ----------
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    // ---------- Módulo Notificaciones ----------
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    // ---------- Módulo Soporte ----------
    public DbSet<TicketSoporte> TicketsSoporte => Set<TicketSoporte>();
    public DbSet<MensajeSoporte> MensajesSoporte => Set<MensajeSoporte>();

    // ---------- Módulo Evaluaciones Psicométricas ----------
    public DbSet<PreguntaPsicometrica> PreguntasPsicometricas => Set<PreguntaPsicometrica>();
    public DbSet<EvaluacionPsicometrica> EvaluacionesPsicometricas => Set<EvaluacionPsicometrica>();
    public DbSet<RespuestaPsicometrica> RespuestasPsicometricas => Set<RespuestaPsicometrica>();

    public DbSet<CrmEmpresa> CrmEmpresas => Set<CrmEmpresa>();
    public DbSet<CrmActividad> CrmActividades => Set<CrmActividad>();
    public DbSet<CrmArchivoAdjunto> CrmArchivosAdjuntos => Set<CrmArchivoAdjunto>();

    public DbSet<ChatConversacion> ChatConversaciones => Set<ChatConversacion>();
    public DbSet<ChatMensaje> ChatMensajes => Set<ChatMensaje>();

    public DbSet<CategoriaFinanciera> CategoriasFinancieras => Set<CategoriaFinanciera>();
    public DbSet<IngresoFinanciero> IngresosFinancieros => Set<IngresoFinanciero>();
    public DbSet<GastoFinanciero> GastosFinancieros => Set<GastoFinanciero>();
    public DbSet<PeriodoFinanciero> PeriodosFinancieros => Set<PeriodoFinanciero>();
    public DbSet<AjusteFinanciero> AjustesFinancieros => Set<AjusteFinanciero>();
    public DbSet<AuditoriaFinanciera> AuditoriasFinancieras => Set<AuditoriaFinanciera>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Necesario: configura el esquema de Identity primero.

        // Ajuste al nombre de tabla para reflejar la extensión custom
        // (EstadoCuenta, FechaRegistro ya vienen incluidas como columnas
        // de ApplicationUser).
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.EstadoCuenta).HasDefaultValue(true);
            entity.Property(u => u.FechaRegistro).HasDefaultValueSql("GETUTCDATE()");
        });

        // ---------- Archivos ----------
        builder.Entity<Archivo>(entity =>
        {
            entity.Property(a => a.TipoArchivo).HasMaxLength(20).IsRequired();
            entity.Property(a => a.RutaArchivo).HasMaxLength(300).IsRequired();
            entity.Property(a => a.NombreOriginal).HasMaxLength(200).IsRequired();
        });

        // ---------- Candidatos ----------
        builder.Entity<Candidato>(entity =>
        {
            entity.HasIndex(c => c.UsuarioId).IsUnique();
            entity.Property(c => c.Nombres).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Apellidos).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Direccion).HasMaxLength(200);
            entity.Property(c => c.Disponibilidad).HasMaxLength(30);

            entity.HasOne(c => c.CvArchivo)
                  .WithMany()
                  .HasForeignKey(c => c.CvArchivoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ---------- Experiencias laborales ----------
        builder.Entity<ExperienciaLaboral>(entity =>
        {
            entity.Property(e => e.NombreEmpresa).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Puesto).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(1000);

            entity.HasOne(e => e.Candidato)
                  .WithMany(c => c.Experiencias)
                  .HasForeignKey(e => e.CandidatoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Educaciones ----------
        builder.Entity<Educacion>(entity =>
        {
            entity.Property(e => e.Institucion).HasMaxLength(150).IsRequired();
            entity.Property(e => e.TituloObtenido).HasMaxLength(150).IsRequired();
            entity.Property(e => e.NivelEducativo).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.Candidato)
                  .WithMany(c => c.Educaciones)
                  .HasForeignKey(e => e.CandidatoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.InstitucionCatalogo)
                  .WithMany()
                  .HasForeignKey(e => e.InstitucionId)
                  // Restrict (no SetNull): con SetNull, SQL Server rechaza la
                  // creación de la tabla porque detecta dos caminos posibles
                  // hacia Instituciones (uno directo desde Educaciones, otro
                  // indirecto pasando por Carreras -> Facultades) — "may
                  // cause cycles or multiple cascade paths". Mismo patrón ya
                  // usado para Carrera -> Categoria.
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Categoria)
                  .WithMany()
                  .HasForeignKey(e => e.CategoriaId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Carrera)
                  .WithMany()
                  .HasForeignKey(e => e.CarreraId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ---------- Habilidades ----------
        builder.Entity<Habilidad>(entity =>
        {
            entity.HasIndex(h => h.Nombre).IsUnique();
            entity.Property(h => h.Nombre).HasMaxLength(100).IsRequired();
        });

        // ---------- CandidatoHabilidad (N:M con clave compuesta) ----------
        builder.Entity<CandidatoHabilidad>(entity =>
        {
            entity.HasKey(ch => new { ch.CandidatoId, ch.HabilidadId });
            entity.Property(ch => ch.NivelDominio).HasMaxLength(20);

            entity.HasOne(ch => ch.Candidato)
                  .WithMany(c => c.Habilidades)
                  .HasForeignKey(ch => ch.CandidatoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ch => ch.Habilidad)
                  .WithMany(h => h.Candidatos)
                  .HasForeignKey(ch => ch.HabilidadId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Categorías / Ubicaciones (catálogos) ----------
        builder.Entity<Categoria>(entity =>
        {
            entity.HasIndex(c => c.Nombre).IsUnique();
            entity.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Descripcion).HasMaxLength(300);
        });

        builder.Entity<Carrera>(entity =>
        {
            entity.Property(c => c.Nombre).HasMaxLength(200).IsRequired();
            entity.HasIndex(c => new { c.FacultadId, c.Nombre }).IsUnique();

            entity.HasOne(c => c.Categoria)
                  .WithMany()
                  .HasForeignKey(c => c.CategoriaId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Cada Carrera pertenece a UNA Facultad (ya no es M:N con
            // Institucion como antes) — dos universidades que ofrecen
            // "Ingeniería Civil" son dos registros de Carrera distintos,
            // cada uno bajo la Facultad real de su propia universidad.
            entity.HasOne(c => c.Facultad)
                  .WithMany(f => f.Carreras)
                  .HasForeignKey(c => c.FacultadId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Institucion>(entity =>
        {
            entity.Property(i => i.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(i => i.Tipo).HasMaxLength(20).IsRequired();
            entity.HasIndex(i => i.Nombre).IsUnique();
        });

        builder.Entity<Facultad>(entity =>
        {
            entity.Property(f => f.Nombre).HasMaxLength(200).IsRequired();
            entity.HasIndex(f => new { f.InstitucionId, f.Nombre }).IsUnique();

            entity.HasOne(f => f.Institucion)
                  .WithMany(i => i.Facultades)
                  .HasForeignKey(f => f.InstitucionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });


        builder.Entity<Ubicacion>(entity =>
        {
            entity.Property(u => u.Departamento).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Ciudad).HasMaxLength(100).IsRequired();
            entity.Ignore(u => u.NombreCompleto); // propiedad calculada, no se persiste
        });

        // ---------- Empresas ----------
        builder.Entity<Empresa>(entity =>
        {
            entity.HasIndex(e => e.UsuarioId).IsUnique();
            entity.HasIndex(e => e.RUC).IsUnique();
            entity.Property(e => e.NombreEmpresa).HasMaxLength(150).IsRequired();
            entity.Property(e => e.RUC).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.SitioWeb).HasMaxLength(200);

            entity.HasOne(e => e.LogoArchivo)
                  .WithMany()
                  .HasForeignKey(e => e.LogoArchivoId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Ubicacion)
                  .WithMany()
                  .HasForeignKey(e => e.UbicacionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Paquetes de empresa (monetización) ----------
        builder.Entity<PaqueteEmpresa>(entity =>
        {
            entity.Property(p => p.Estado).HasMaxLength(20).IsRequired();
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_PaquetesEmpresa_Consumo", "[VacantesConsumidas] <= [VacantesIncluidas] OR [VacantesIncluidas] IS NULL"));

            entity.HasOne(p => p.Empresa)
                  .WithMany(e => e.Paquetes)
                  .HasForeignKey(p => p.EmpresaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.PlanSuscripcion)
                  .WithMany()
                  .HasForeignKey(p => p.PlanSuscripcionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Planes de suscripción ----------
        builder.Entity<PlanSuscripcion>(entity =>
        {
            entity.Property(p => p.Nombre).HasMaxLength(50).IsRequired();
            entity.Property(p => p.Precio).HasColumnType("decimal(10,2)");
        });

        builder.Entity<PlanSuscripcion>().HasData(
            new PlanSuscripcion { Id = 1, Nombre = PlanSuscripcion.Nombres.Basico, Precio = 20.00m, VacantesIncluidas = 10, DiasVigencia = 30, Activo = true, IncluyePruebaPsicometrica = false, IncluyeVideoCv = false, PermiteVacantesDestacadas = false },
            new PlanSuscripcion { Id = 2, Nombre = PlanSuscripcion.Nombres.Empresarial, Precio = 50.00m, VacantesIncluidas = null, DiasVigencia = 30, Activo = true, IncluyePruebaPsicometrica = true, IncluyeVideoCv = true, PermiteVacantesDestacadas = true }
        );

        // ---------- Pagos ----------
        builder.Entity<Pago>(entity =>
        {
            entity.HasIndex(p => p.PaqueteEmpresaId).IsUnique();
            entity.Property(p => p.Monto).HasColumnType("decimal(10,2)").HasDefaultValue(20.00m);
            entity.Property(p => p.MetodoPago).HasMaxLength(50).IsRequired();
            entity.Property(p => p.ReferenciaTransaccion).HasMaxLength(150);
            entity.Property(p => p.EstadoPago).HasMaxLength(20).IsRequired();
            entity.Property(p => p.IdOrdenPayPal).HasMaxLength(50);
            entity.Property(p => p.ComentarioRevision).HasMaxLength(500);

            entity.HasOne(p => p.PaqueteEmpresa)
                  .WithOne(pe => pe.Pago)
                  .HasForeignKey<Pago>(p => p.PaqueteEmpresaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Vacantes ----------
        builder.Entity<Vacante>(entity =>
        {
            entity.Property(v => v.Titulo).HasMaxLength(150).IsRequired();
            entity.Property(v => v.Descripcion).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(v => v.Requisitos).HasColumnType("nvarchar(max)");
            entity.Property(v => v.Modalidad).HasMaxLength(30).IsRequired();
            entity.Property(v => v.ExperienciaRequerida).HasMaxLength(30).IsRequired();
            entity.Property(v => v.SalarioMin).HasColumnType("decimal(10,2)");
            entity.Property(v => v.SalarioMax).HasColumnType("decimal(10,2)");
            entity.Property(v => v.Estado).HasMaxLength(20).IsRequired();

            entity.HasIndex(v => new { v.CategoriaId, v.UbicacionId, v.Estado });
            entity.HasIndex(v => v.EmpresaId);

            entity.HasOne(v => v.Empresa)
                  .WithMany(e => e.Vacantes)
                  .HasForeignKey(v => v.EmpresaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Categoria)
                  .WithMany()
                  .HasForeignKey(v => v.CategoriaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Carrera)
                  .WithMany()
                  .HasForeignKey(v => v.CarreraId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(v => v.Ubicacion)
                  .WithMany()
                  .HasForeignKey(v => v.UbicacionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.PaqueteEmpresa)
                  .WithMany(p => p.Vacantes)
                  .HasForeignKey(v => v.PaqueteEmpresaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Configuración del sistema ----------
        builder.Entity<ConfiguracionSistema>(entity =>
        {
            entity.HasIndex(c => c.Clave).IsUnique();
            entity.Property(c => c.Clave).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Valor).HasMaxLength(300).IsRequired();
            entity.Property(c => c.Descripcion).HasMaxLength(300);
        });

        // ---------- Datos semilla ----------
        builder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Tecnología", Descripcion = "Desarrollo de software, TI, soporte técnico" },
            new Categoria { Id = 2, Nombre = "Ventas", Descripcion = "Ventas, atención comercial" },
            new Categoria { Id = 3, Nombre = "Administración", Descripcion = "Roles administrativos y de oficina" },
            new Categoria { Id = 4, Nombre = "Salud", Descripcion = "Personal médico y asistencial" },
            new Categoria { Id = 5, Nombre = "Educación", Descripcion = "Docencia y formación" },
            new Categoria { Id = 6, Nombre = "Construcción", Descripcion = "Ingeniería civil y obra" },
            new Categoria { Id = 7, Nombre = "Atención al Cliente", Descripcion = "Call center, soporte al cliente" },
            new Categoria { Id = 8, Nombre = "Derecho" },
            new Categoria { Id = 9, Nombre = "Ciencias Agropecuarias y Veterinaria" },
            new Categoria { Id = 10, Nombre = "Idiomas" },
            new Categoria { Id = 11, Nombre = "Artes, Diseño y Comunicación" },
            new Categoria { Id = 12, Nombre = "Ciencias Sociales" },
            new Categoria { Id = 13, Nombre = "Turismo y Hotelería" },
            new Categoria { Id = 14, Nombre = "Industria y Oficios Técnicos" },
            new Categoria { Id = 15, Nombre = "Logística, Transporte y Mensajería", Descripcion = "Motorizados, repartidores, choferes, ayudantes de bodega" },
            new Categoria { Id = 16, Nombre = "Producción y Manufactura", Descripcion = "Operarios, ayudantes de producción, empaque, ensamblaje" },
            new Categoria { Id = 17, Nombre = "Servicios Generales y Oficios", Descripcion = "Limpieza, mantenimiento, jardinería, oficios varios" },
            new Categoria { Id = 18, Nombre = "Seguridad y Vigilancia", Descripcion = "Guardias, vigilantes, control de acceso" },
            new Categoria { Id = 19, Nombre = "Gastronomía y Alimentos", Descripcion = "Meseros, cocineros, ayudantes de cocina, baristas" },
            new Categoria { Id = 20, Nombre = "Comercio y Ventas al Detalle", Descripcion = "Cajeros, dependientes de tienda, bombas de gasolina" }
        );

        // ---------- Carreras (subcategorías dentro de cada Categoría) ----------
        builder.Entity<Institucion>().HasData(
            new Institucion { Id = 1, Nombre = "UNAN-Managua", Tipo = "Universidad" },
            new Institucion { Id = 2, Nombre = "UNAN-León", Tipo = "Universidad" },
            new Institucion { Id = 3, Nombre = "UNI - Universidad Nacional de Ingeniería", Tipo = "Universidad" },
            new Institucion { Id = 4, Nombre = "UNA - Universidad Nacional Agraria", Tipo = "Universidad" },
            new Institucion { Id = 5, Nombre = "UCA - Universidad Centroamericana (cancelada en 2023 — ver UNCSM)", Tipo = "Universidad" },
            new Institucion { Id = 6, Nombre = "UNP - Universidad Nacional Politécnica (antes UPOLI)", Tipo = "Universidad" },
            new Institucion { Id = 7, Nombre = "UAM - Universidad Americana", Tipo = "Universidad" },
            new Institucion { Id = 8, Nombre = "Keiser University", Tipo = "Universidad" },
            new Institucion { Id = 9, Nombre = "UCC - Universidad de Ciencias Comerciales", Tipo = "Universidad" },
            new Institucion { Id = 10, Nombre = "INATEC - Instituto Nacional Tecnológico", Tipo = "Técnico" },
            new Institucion { Id = 11, Nombre = "Universidad Nacional Casimiro Sotelo Montenegro (UNCSM)", Tipo = "Universidad" },
            new Institucion { Id = 12, Nombre = "Universidad Nacional Francisco Luis Espinoza Pineda (UNFLEP)", Tipo = "Universidad" },
            new Institucion { Id = 13, Nombre = "URACCAN - Universidad de las Regiones Autónomas de la Costa Caribe Nicaragüense", Tipo = "Universidad" },
            new Institucion { Id = 14, Nombre = "BICU - Bluefields Indian and Caribbean University", Tipo = "Universidad" },
            new Institucion { Id = 15, Nombre = "UCN - Universidad Central de Nicaragua", Tipo = "Universidad" },
            new Institucion { Id = 16, Nombre = "UNICIT - Universidad Iberoamericana de Ciencia y Tecnología", Tipo = "Universidad" },
            new Institucion { Id = 17, Nombre = "UCYT - Universidad Nicaragüense de Ciencia y Tecnología", Tipo = "Universidad" }
        );

        // ---------- Facultades (nivel intermedio del catálogo de 3 pasos) ----------
        builder.Entity<Facultad>().HasData(
            new Facultad { Id = 1, InstitucionId = 1, Nombre = "Facultad de Ciencias e Ingeniería" },
            new Facultad { Id = 2, InstitucionId = 1, Nombre = "Facultad de Educación e Idiomas" },
            new Facultad { Id = 3, InstitucionId = 1, Nombre = "Facultad de Humanidades y Ciencias Jurídicas" },
            new Facultad { Id = 4, InstitucionId = 1, Nombre = "Facultad de Ciencias Económicas" },
            new Facultad { Id = 5, InstitucionId = 1, Nombre = "Facultad de Ciencias Médicas / Instituto Politécnico de la Salud (POLISAL)" },
            new Facultad { Id = 6, InstitucionId = 2, Nombre = "Ciencias Jurídicas y Sociales" },
            new Facultad { Id = 7, InstitucionId = 2, Nombre = "Ciencias de la Salud" },
            new Facultad { Id = 8, InstitucionId = 2, Nombre = "Ciencias Químicas" },
            new Facultad { Id = 9, InstitucionId = 2, Nombre = "Odontología" },
            new Facultad { Id = 10, InstitucionId = 2, Nombre = "Ciencias y Tecnología" },
            new Facultad { Id = 11, InstitucionId = 2, Nombre = "Ciencias de la Educación y Humanidades" },
            new Facultad { Id = 12, InstitucionId = 2, Nombre = "Ciencias Económicas y Empresariales" },
            new Facultad { Id = 13, InstitucionId = 2, Nombre = "Ciencias Agrarias y Veterinaria" },
            new Facultad { Id = 14, InstitucionId = 3, Nombre = "Oferta académica 2025-2026" },
            new Facultad { Id = 15, InstitucionId = 4, Nombre = "Facultad de Agronomía" },
            new Facultad { Id = 16, InstitucionId = 4, Nombre = "Facultad de Ciencia Animal" },
            new Facultad { Id = 17, InstitucionId = 4, Nombre = "Facultad de Desarrollo Rural" },
            new Facultad { Id = 18, InstitucionId = 6, Nombre = "Administración, Comercio y Finanzas" },
            new Facultad { Id = 19, InstitucionId = 6, Nombre = "Derecho" },
            new Facultad { Id = 20, InstitucionId = 6, Nombre = "Diseño y Comunicación" },
            new Facultad { Id = 21, InstitucionId = 6, Nombre = "Enfermería" },
            new Facultad { Id = 22, InstitucionId = 6, Nombre = "Ingeniería" },
            new Facultad { Id = 23, InstitucionId = 6, Nombre = "Educación, Arte y Humanidades" },
            new Facultad { Id = 24, InstitucionId = 7, Nombre = "Facultad de Medicina" },
            new Facultad { Id = 25, InstitucionId = 7, Nombre = "Facultad de Odontología" },
            new Facultad { Id = 26, InstitucionId = 7, Nombre = "Facultad de Ciencias Médicas" },
            new Facultad { Id = 27, InstitucionId = 7, Nombre = "Facultad de Ciencias Jurídicas y Relaciones Internacionales" },
            new Facultad { Id = 28, InstitucionId = 7, Nombre = "Facultad de Diseño, Arquitectura y Ciencias de la Comunicación" },
            new Facultad { Id = 29, InstitucionId = 7, Nombre = "Facultad de Ciencias Administrativas y Económicas" },
            new Facultad { Id = 30, InstitucionId = 7, Nombre = "Facultad de Ingeniería y Arquitectura" },
            new Facultad { Id = 31, InstitucionId = 7, Nombre = "UAM College (programas internacionales en inglés)" },
            new Facultad { Id = 32, InstitucionId = 11, Nombre = "Carreras reportadas" },
            new Facultad { Id = 33, InstitucionId = 12, Nombre = "Carreras" },
            new Facultad { Id = 34, InstitucionId = 13, Nombre = "Área de Salud y Servicio Social" },
            new Facultad { Id = 35, InstitucionId = 13, Nombre = "Área de Ciencias Económicas y Jurídicas" },
            new Facultad { Id = 36, InstitucionId = 13, Nombre = "Área de Educación" },
            new Facultad { Id = 37, InstitucionId = 13, Nombre = "Área de Agricultura" },
            new Facultad { Id = 38, InstitucionId = 13, Nombre = "Área de Ciencias, Tecnologías e Ingenierías" },
            new Facultad { Id = 39, InstitucionId = 13, Nombre = "Área de Ciencias Sociales, Humanidades y Artes" },
            new Facultad { Id = 40, InstitucionId = 14, Nombre = "Carreras reportadas" },
            new Facultad { Id = 41, InstitucionId = 15, Nombre = "Ciencias Administrativas" },
            new Facultad { Id = 42, InstitucionId = 15, Nombre = "Ciencias Jurídicas y Sociales" },
            new Facultad { Id = 43, InstitucionId = 15, Nombre = "Ciencias Médicas" },
            new Facultad { Id = 44, InstitucionId = 15, Nombre = "Ingeniería" },
            new Facultad { Id = 45, InstitucionId = 15, Nombre = "Medicina Veterinaria" },
            new Facultad { Id = 46, InstitucionId = 16, Nombre = "Facultad de Ingeniería y Arquitectura" },
            new Facultad { Id = 47, InstitucionId = 16, Nombre = "Otras facultades" },
            new Facultad { Id = 48, InstitucionId = 17, Nombre = "Carreras reportadas" },
            new Facultad { Id = 49, InstitucionId = 10, Nombre = "Comercio y Servicios — Hotelería y Turismo" },
            new Facultad { Id = 50, InstitucionId = 10, Nombre = "Comercio y Servicios — Administración" },
            new Facultad { Id = 51, InstitucionId = 10, Nombre = "Comercio y Servicios — Actividades Físicas y Deportivas" },
            new Facultad { Id = 52, InstitucionId = 10, Nombre = "Comercio y Servicios — Docencia" },
            new Facultad { Id = 53, InstitucionId = 10, Nombre = "Comercio y Servicios — Finanzas" },
            new Facultad { Id = 54, InstitucionId = 10, Nombre = "Comercio y Servicios — Informática" },
            new Facultad { Id = 55, InstitucionId = 10, Nombre = "Industria y Construcción — Automotriz" },
            new Facultad { Id = 56, InstitucionId = 10, Nombre = "Industria y Construcción — Construcción" },
            new Facultad { Id = 57, InstitucionId = 10, Nombre = "Industria y Construcción — Cuero y Calzado" },
            new Facultad { Id = 58, InstitucionId = 10, Nombre = "Industria y Construcción — Electricidad y Electrónica" },
            new Facultad { Id = 59, InstitucionId = 10, Nombre = "Industria y Construcción — Energías Renovables" },
            new Facultad { Id = 60, InstitucionId = 10, Nombre = "Industria y Construcción — Madera Mueble" },
            new Facultad { Id = 61, InstitucionId = 10, Nombre = "Industria y Construcción — Metal Mecánica" },
            new Facultad { Id = 62, InstitucionId = 10, Nombre = "Industria y Construcción — Pesca" },
            new Facultad { Id = 63, InstitucionId = 10, Nombre = "Industria y Construcción — Química" },
            new Facultad { Id = 64, InstitucionId = 10, Nombre = "Industria y Construcción — Refrigeración" },
            new Facultad { Id = 65, InstitucionId = 10, Nombre = "Industria y Construcción — Textil-Vestuario" },
            new Facultad { Id = 66, InstitucionId = 10, Nombre = "Industria y Construcción — Producción de Palma" },
            new Facultad { Id = 67, InstitucionId = 10, Nombre = "Agropecuario y Forestal — Agroindustria de los Alimentos" },
            new Facultad { Id = 68, InstitucionId = 10, Nombre = "Agropecuario y Forestal — Agropecuaria" },
            new Facultad { Id = 69, InstitucionId = 10, Nombre = "Agropecuario y Forestal — Forestal" },
            new Facultad { Id = 70, InstitucionId = 10, Nombre = "Agropecuario y Forestal — Veterinaria" }
        );

        // ---------- Carreras (ahora cada una pertenece a UNA Facultad de UNA
        // institución — antes era M:N y esto se reemplaza por completo con
        // los datos de la investigación de universidades de Nicaragua e
        // INATEC de agosto 2026) ----------
        builder.Entity<Carrera>().HasData(
            new Carrera { Id = 1, CategoriaId = 6, FacultadId = 1, Nombre = "Arquitectura" },
            new Carrera { Id = 2, CategoriaId = 1, FacultadId = 1, Nombre = "Licenciatura en Biología" },
            new Carrera { Id = 3, CategoriaId = 1, FacultadId = 1, Nombre = "Licenciatura en Ciencias Naturales" },
            new Carrera { Id = 4, CategoriaId = 1, FacultadId = 1, Nombre = "Ingeniería Ambiental" },
            new Carrera { Id = 5, CategoriaId = 1, FacultadId = 1, Nombre = "Licenciatura en Química Ambiental" },
            new Carrera { Id = 6, CategoriaId = 1, FacultadId = 1, Nombre = "Licenciatura en Química Farmacéutica" },
            new Carrera { Id = 7, CategoriaId = 1, FacultadId = 1, Nombre = "Licenciatura en Química Industrial" },
            new Carrera { Id = 8, CategoriaId = 1, FacultadId = 1, Nombre = "Ingeniería en Ciencias de la Computación" },
            new Carrera { Id = 9, CategoriaId = 6, FacultadId = 1, Nombre = "Ingeniería Civil" },
            new Carrera { Id = 10, CategoriaId = 1, FacultadId = 1, Nombre = "Ingeniería Electrónica" },
            new Carrera { Id = 11, CategoriaId = 1, FacultadId = 1, Nombre = "Ingeniería en Sistemas de la Información" },
            new Carrera { Id = 12, CategoriaId = 1, FacultadId = 1, Nombre = "Ingeniería Industrial" },
            new Carrera { Id = 13, CategoriaId = 1, FacultadId = 1, Nombre = "Ingeniería Estadística" },
            new Carrera { Id = 14, CategoriaId = 1, FacultadId = 1, Nombre = "Ingeniería en Geofísica" },
            new Carrera { Id = 15, CategoriaId = 1, FacultadId = 1, Nombre = "Ingeniería Geológica" },
            new Carrera { Id = 16, CategoriaId = 1, FacultadId = 1, Nombre = "Ingeniería en Energías Renovables" },
            new Carrera { Id = 17, CategoriaId = 1, FacultadId = 1, Nombre = "Ingeniería en Telemática" },
            new Carrera { Id = 18, CategoriaId = 1, FacultadId = 1, Nombre = "Física" },
            new Carrera { Id = 19, CategoriaId = 1, FacultadId = 1, Nombre = "Física Médica" },
            new Carrera { Id = 20, CategoriaId = 1, FacultadId = 1, Nombre = "Física con mención en Geofísica" },
            new Carrera { Id = 21, CategoriaId = 6, FacultadId = 1, Nombre = "Técnico Superior en Construcción" },
            new Carrera { Id = 22, CategoriaId = 6, FacultadId = 1, Nombre = "Técnico Superior en Topografía" },
            new Carrera { Id = 23, CategoriaId = 1, FacultadId = 2, Nombre = "Licenciatura en Matemática" },
            new Carrera { Id = 24, CategoriaId = 10, FacultadId = 2, Nombre = "Francés" },
            new Carrera { Id = 25, CategoriaId = 10, FacultadId = 2, Nombre = "Inglés" },
            new Carrera { Id = 26, CategoriaId = 1, FacultadId = 2, Nombre = "Lengua y Literatura Hispánicas" },
            new Carrera { Id = 27, CategoriaId = 5, FacultadId = 2, Nombre = "Licenciatura en Informática Educativa" },
            new Carrera { Id = 28, CategoriaId = 1, FacultadId = 2, Nombre = "Licenciatura en Innovación y Emprendimiento" },
            new Carrera { Id = 29, CategoriaId = 11, FacultadId = 2, Nombre = "Licenciatura en Cultura y Artes" },
            new Carrera { Id = 30, CategoriaId = 11, FacultadId = 2, Nombre = "Licenciatura en Danza" },
            new Carrera { Id = 31, CategoriaId = 11, FacultadId = 2, Nombre = "Licenciatura en Diseño Gráfico y Multimedia" },
            new Carrera { Id = 32, CategoriaId = 5, FacultadId = 2, Nombre = "Licenciatura en Educación Física y Deportes" },
            new Carrera { Id = 33, CategoriaId = 1, FacultadId = 2, Nombre = "Licenciatura en Traducción e Interpretación de Lenguas Extranjeras" },
            new Carrera { Id = 34, CategoriaId = 13, FacultadId = 2, Nombre = "Licenciatura en Turismo Sostenible" },
            new Carrera { Id = 35, CategoriaId = 3, FacultadId = 2, Nombre = "Licenciatura en Administración de la Educación" },
            new Carrera { Id = 36, CategoriaId = 5, FacultadId = 2, Nombre = "Pedagogía con mención en Educación para la Diversidad" },
            new Carrera { Id = 37, CategoriaId = 5, FacultadId = 2, Nombre = "Licenciatura en Educación Infantil" },
            new Carrera { Id = 38, CategoriaId = 5, FacultadId = 2, Nombre = "Licenciatura en Educación Primaria" },
            new Carrera { Id = 39, CategoriaId = 11, FacultadId = 2, Nombre = "Licenciatura en Educación Musical" },
            new Carrera { Id = 40, CategoriaId = 5, FacultadId = 2, Nombre = "Licenciatura en Educación Especial" },
            new Carrera { Id = 41, CategoriaId = 5, FacultadId = 2, Nombre = "Licenciatura en Educación Comercial" },
            new Carrera { Id = 42, CategoriaId = 5, FacultadId = 2, Nombre = "Ciencias de la Educación con mención en Ciencias Naturales" },
            new Carrera { Id = 43, CategoriaId = 5, FacultadId = 2, Nombre = "Ciencias de la Educación con mención en Ciencias Sociales" },
            new Carrera { Id = 44, CategoriaId = 5, FacultadId = 2, Nombre = "Ciencias de la Educación con mención en Física-Matemática" },
            new Carrera { Id = 45, CategoriaId = 10, FacultadId = 2, Nombre = "Ciencias de la Educación con mención en Inglés" },
            new Carrera { Id = 46, CategoriaId = 5, FacultadId = 2, Nombre = "Ciencias de la Educación con mención en Lengua y Literatura Hispánica" },
            new Carrera { Id = 47, CategoriaId = 12, FacultadId = 3, Nombre = "Antropología Social" },
            new Carrera { Id = 48, CategoriaId = 12, FacultadId = 3, Nombre = "Ciencia Política y Relaciones Internacionales" },
            new Carrera { Id = 49, CategoriaId = 11, FacultadId = 3, Nombre = "Comunicación para el Desarrollo" },
            new Carrera { Id = 50, CategoriaId = 8, FacultadId = 3, Nombre = "Derecho" },
            new Carrera { Id = 51, CategoriaId = 12, FacultadId = 3, Nombre = "Geografía" },
            new Carrera { Id = 52, CategoriaId = 12, FacultadId = 3, Nombre = "Gestión de la Información" },
            new Carrera { Id = 53, CategoriaId = 12, FacultadId = 3, Nombre = "Historia" },
            new Carrera { Id = 54, CategoriaId = 12, FacultadId = 3, Nombre = "Psicología" },
            new Carrera { Id = 55, CategoriaId = 12, FacultadId = 3, Nombre = "Trabajo Social" },
            new Carrera { Id = 56, CategoriaId = 1, FacultadId = 3, Nombre = "Ciencias Sociales" },
            new Carrera { Id = 57, CategoriaId = 3, FacultadId = 4, Nombre = "Administración de Empresas" },
            new Carrera { Id = 58, CategoriaId = 3, FacultadId = 4, Nombre = "Contaduría Pública y Finanzas" },
            new Carrera { Id = 59, CategoriaId = 3, FacultadId = 4, Nombre = "Banca y Finanzas" },
            new Carrera { Id = 60, CategoriaId = 3, FacultadId = 4, Nombre = "Economía" },
            new Carrera { Id = 61, CategoriaId = 9, FacultadId = 4, Nombre = "Economía Agrícola" },
            new Carrera { Id = 62, CategoriaId = 3, FacultadId = 4, Nombre = "Mercadotecnia" },
            new Carrera { Id = 63, CategoriaId = 4, FacultadId = 5, Nombre = "Medicina y Cirugía" },
            new Carrera { Id = 64, CategoriaId = 4, FacultadId = 5, Nombre = "Odontología" },
            new Carrera { Id = 65, CategoriaId = 4, FacultadId = 5, Nombre = "Optometría Médica" },
            new Carrera { Id = 66, CategoriaId = 4, FacultadId = 5, Nombre = "Licenciatura en Anestesia y Reanimación" },
            new Carrera { Id = 67, CategoriaId = 4, FacultadId = 5, Nombre = "Licenciatura en Bioanálisis Clínico" },
            new Carrera { Id = 68, CategoriaId = 4, FacultadId = 5, Nombre = "Fisioterapia" },
            new Carrera { Id = 69, CategoriaId = 4, FacultadId = 5, Nombre = "Enfermería (Licenciatura general)" },
            new Carrera { Id = 70, CategoriaId = 4, FacultadId = 5, Nombre = "Licenciatura en Enfermería Obstétrica y Perinatal" },
            new Carrera { Id = 71, CategoriaId = 4, FacultadId = 5, Nombre = "Licenciatura en Enfermería en Cuidados Críticos" },
            new Carrera { Id = 72, CategoriaId = 4, FacultadId = 5, Nombre = "Licenciatura en Enfermería en Salud Pública" },
            new Carrera { Id = 73, CategoriaId = 4, FacultadId = 5, Nombre = "Licenciatura en Enfermería en Materno Infantil" },
            new Carrera { Id = 74, CategoriaId = 4, FacultadId = 5, Nombre = "Microbiología" },
            new Carrera { Id = 75, CategoriaId = 4, FacultadId = 5, Nombre = "Nutrición" },
            new Carrera { Id = 76, CategoriaId = 4, FacultadId = 5, Nombre = "Técnico Superior en Citología Cervical" },
            new Carrera { Id = 77, CategoriaId = 4, FacultadId = 5, Nombre = "Técnico Superior en Enfermería General" },
            new Carrera { Id = 78, CategoriaId = 8, FacultadId = 6, Nombre = "Derecho" },
            new Carrera { Id = 79, CategoriaId = 8, FacultadId = 6, Nombre = "Administración y Políticas Públicas" },
            new Carrera { Id = 80, CategoriaId = 4, FacultadId = 7, Nombre = "Medicina" },
            new Carrera { Id = 81, CategoriaId = 12, FacultadId = 7, Nombre = "Psicología" },
            new Carrera { Id = 82, CategoriaId = 4, FacultadId = 7, Nombre = "Bioanálisis Clínico" },
            new Carrera { Id = 83, CategoriaId = 4, FacultadId = 7, Nombre = "Ciencias de Enfermería" },
            new Carrera { Id = 84, CategoriaId = 4, FacultadId = 7, Nombre = "Técnico Superior en Radiología" },
            new Carrera { Id = 85, CategoriaId = 4, FacultadId = 7, Nombre = "Técnico Superior en Enfermería en Técnicas Quirúrgicas" },
            new Carrera { Id = 86, CategoriaId = 1, FacultadId = 7, Nombre = "Técnico Superior en Anestesiología" },
            new Carrera { Id = 87, CategoriaId = 4, FacultadId = 7, Nombre = "Técnico Superior en Citotecnología" },
            new Carrera { Id = 88, CategoriaId = 4, FacultadId = 7, Nombre = "Técnico Superior en Higiene y Seguridad Laboral" },
            new Carrera { Id = 89, CategoriaId = 1, FacultadId = 8, Nombre = "Ingeniería en Alimentos" },
            new Carrera { Id = 90, CategoriaId = 4, FacultadId = 8, Nombre = "Farmacia" },
            new Carrera { Id = 91, CategoriaId = 4, FacultadId = 9, Nombre = "Odontología" },
            new Carrera { Id = 92, CategoriaId = 1, FacultadId = 10, Nombre = "Matemática" },
            new Carrera { Id = 93, CategoriaId = 1, FacultadId = 10, Nombre = "Biología" },
            new Carrera { Id = 94, CategoriaId = 1, FacultadId = 10, Nombre = "Ciencias Actuariales y Financieras" },
            new Carrera { Id = 95, CategoriaId = 1, FacultadId = 10, Nombre = "Ingeniería en Telemática" },
            new Carrera { Id = 96, CategoriaId = 1, FacultadId = 10, Nombre = "Ingeniería en Estadística" },
            new Carrera { Id = 97, CategoriaId = 1, FacultadId = 10, Nombre = "Ingeniería en Sistemas" },
            new Carrera { Id = 98, CategoriaId = 1, FacultadId = 10, Nombre = "Ingeniería en Tecnología de la Información con énfasis en Desarrollo Web y Marketing Digital" },
            new Carrera { Id = 99, CategoriaId = 11, FacultadId = 11, Nombre = "Comunicación Social" },
            new Carrera { Id = 100, CategoriaId = 12, FacultadId = 11, Nombre = "Trabajo Social" },
            new Carrera { Id = 101, CategoriaId = 10, FacultadId = 11, Nombre = "Lengua Inglesa" },
            new Carrera { Id = 102, CategoriaId = 5, FacultadId = 11, Nombre = "Ciencias de la Educación con mención en Lengua y Literatura" },
            new Carrera { Id = 103, CategoriaId = 5, FacultadId = 11, Nombre = "Ciencias de la Educación con mención en Educación Especial Incluyente" },
            new Carrera { Id = 104, CategoriaId = 10, FacultadId = 11, Nombre = "Ciencias de la Educación con mención en Inglés" },
            new Carrera { Id = 105, CategoriaId = 5, FacultadId = 11, Nombre = "Ciencias de la Educación con mención en Ciencias Sociales" },
            new Carrera { Id = 106, CategoriaId = 5, FacultadId = 11, Nombre = "Educación Física y Deportes" },
            new Carrera { Id = 107, CategoriaId = 5, FacultadId = 11, Nombre = "Matemática Educativa y Computación" },
            new Carrera { Id = 108, CategoriaId = 1, FacultadId = 11, Nombre = "Ciencias Naturales" },
            new Carrera { Id = 109, CategoriaId = 3, FacultadId = 12, Nombre = "Contaduría Pública y Finanzas" },
            new Carrera { Id = 110, CategoriaId = 3, FacultadId = 12, Nombre = "Administración de Empresas" },
            new Carrera { Id = 111, CategoriaId = 3, FacultadId = 12, Nombre = "Economía" },
            new Carrera { Id = 112, CategoriaId = 1, FacultadId = 12, Nombre = "Gestión de Empresas Turísticas" },
            new Carrera { Id = 113, CategoriaId = 3, FacultadId = 12, Nombre = "Mercadotecnia" },
            new Carrera { Id = 114, CategoriaId = 10, FacultadId = 12, Nombre = "Administración en Lengua Extranjera" },
            new Carrera { Id = 115, CategoriaId = 3, FacultadId = 12, Nombre = "Técnico en Comercio Internacional" },
            new Carrera { Id = 116, CategoriaId = 3, FacultadId = 12, Nombre = "Técnico Superior en Finanzas" },
            new Carrera { Id = 117, CategoriaId = 9, FacultadId = 13, Nombre = "Ingeniería en Agroecología Tropical" },
            new Carrera { Id = 118, CategoriaId = 9, FacultadId = 13, Nombre = "Ingeniería en Agronegocios" },
            new Carrera { Id = 119, CategoriaId = 9, FacultadId = 13, Nombre = "Ingeniería Acuícola" },
            new Carrera { Id = 120, CategoriaId = 9, FacultadId = 13, Nombre = "Medicina Veterinaria" },
            new Carrera { Id = 121, CategoriaId = 9, FacultadId = 13, Nombre = "Ingeniería en Zootecnia" },
            new Carrera { Id = 122, CategoriaId = 9, FacultadId = 13, Nombre = "Ingeniería Agropecuaria" },
            new Carrera { Id = 123, CategoriaId = 6, FacultadId = 14, Nombre = "Arquitectura" },
            new Carrera { Id = 124, CategoriaId = 6, FacultadId = 14, Nombre = "Ingeniería Civil (diurno y nocturno)" },
            new Carrera { Id = 125, CategoriaId = 9, FacultadId = 14, Nombre = "Ingeniería Agrícola" },
            new Carrera { Id = 126, CategoriaId = 1, FacultadId = 14, Nombre = "Ingeniería Industrial (diurno y nocturno)" },
            new Carrera { Id = 127, CategoriaId = 1, FacultadId = 14, Nombre = "Ingeniería Mecánica (diurno y nocturno)" },
            new Carrera { Id = 128, CategoriaId = 1, FacultadId = 14, Nombre = "Ingeniería de Sistemas" },
            new Carrera { Id = 129, CategoriaId = 1, FacultadId = 14, Nombre = "Ingeniería Química" },
            new Carrera { Id = 130, CategoriaId = 1, FacultadId = 14, Nombre = "Ingeniería Electrónica (diurno y nocturno)" },
            new Carrera { Id = 131, CategoriaId = 1, FacultadId = 14, Nombre = "Ingeniería Eléctrica (diurno y nocturno)" },
            new Carrera { Id = 132, CategoriaId = 1, FacultadId = 14, Nombre = "Ingeniería en Computación" },
            new Carrera { Id = 133, CategoriaId = 9, FacultadId = 15, Nombre = "Ingeniería Agronómica" },
            new Carrera { Id = 134, CategoriaId = 9, FacultadId = 15, Nombre = "Ingeniería en Sanidad Vegetal" },
            new Carrera { Id = 135, CategoriaId = 9, FacultadId = 15, Nombre = "Ingeniería Agrícola" },
            new Carrera { Id = 136, CategoriaId = 9, FacultadId = 15, Nombre = "Ingeniería en Agroindustria de los Alimentos" },
            new Carrera { Id = 137, CategoriaId = 9, FacultadId = 15, Nombre = "Ingeniería Forestal" },
            new Carrera { Id = 138, CategoriaId = 9, FacultadId = 15, Nombre = "Ingeniería en Recursos Naturales Renovables" },
            new Carrera { Id = 139, CategoriaId = 9, FacultadId = 16, Nombre = "Ingeniería en Zootecnia" },
            new Carrera { Id = 140, CategoriaId = 9, FacultadId = 16, Nombre = "Medicina Veterinaria" },
            new Carrera { Id = 141, CategoriaId = 9, FacultadId = 17, Nombre = "Licenciatura en Agronegocios" },
            new Carrera { Id = 142, CategoriaId = 9, FacultadId = 17, Nombre = "Licenciatura en Desarrollo Rural" },
            new Carrera { Id = 143, CategoriaId = 13, FacultadId = 17, Nombre = "Licenciatura en Turismo Rural y Comunitario" },
            new Carrera { Id = 144, CategoriaId = 3, FacultadId = 18, Nombre = "Administración de Empresas" },
            new Carrera { Id = 145, CategoriaId = 3, FacultadId = 18, Nombre = "Banca y Finanzas" },
            new Carrera { Id = 146, CategoriaId = 8, FacultadId = 19, Nombre = "Derecho" },
            new Carrera { Id = 147, CategoriaId = 11, FacultadId = 20, Nombre = "Diseño Gráfico" },
            new Carrera { Id = 148, CategoriaId = 11, FacultadId = 20, Nombre = "Diseño de Producto" },
            new Carrera { Id = 149, CategoriaId = 11, FacultadId = 20, Nombre = "Diseño Integral de Comunicaciones" },
            new Carrera { Id = 150, CategoriaId = 11, FacultadId = 20, Nombre = "Comunicación Social" },
            new Carrera { Id = 151, CategoriaId = 4, FacultadId = 21, Nombre = "Enfermería" },
            new Carrera { Id = 152, CategoriaId = 1, FacultadId = 22, Nombre = "Ingeniería Industrial" },
            new Carrera { Id = 153, CategoriaId = 11, FacultadId = 23, Nombre = "Producción de Espectáculos" },
            new Carrera { Id = 154, CategoriaId = 11, FacultadId = 23, Nombre = "Enseñanza Artística Musical (Conservatorio)" },
            new Carrera { Id = 155, CategoriaId = 10, FacultadId = 23, Nombre = "Idiomas" },
            new Carrera { Id = 156, CategoriaId = 4, FacultadId = 24, Nombre = "Medicina y Cirugía" },
            new Carrera { Id = 157, CategoriaId = 4, FacultadId = 25, Nombre = "Odontología" },
            new Carrera { Id = 158, CategoriaId = 4, FacultadId = 26, Nombre = "Enfermería" },
            new Carrera { Id = 159, CategoriaId = 8, FacultadId = 27, Nombre = "Derecho" },
            new Carrera { Id = 160, CategoriaId = 12, FacultadId = 27, Nombre = "Diplomacia y Relaciones Internacionales" },
            new Carrera { Id = 161, CategoriaId = 6, FacultadId = 28, Nombre = "Arquitectura" },
            new Carrera { Id = 162, CategoriaId = 11, FacultadId = 28, Nombre = "Diseño y Comunicación Visual" },
            new Carrera { Id = 163, CategoriaId = 11, FacultadId = 28, Nombre = "Comunicación y Relaciones Públicas" },
            new Carrera { Id = 164, CategoriaId = 3, FacultadId = 29, Nombre = "Administración de Empresas" },
            new Carrera { Id = 165, CategoriaId = 3, FacultadId = 29, Nombre = "Contabilidad y Finanzas" },
            new Carrera { Id = 166, CategoriaId = 3, FacultadId = 29, Nombre = "Negocios Internacionales" },
            new Carrera { Id = 167, CategoriaId = 11, FacultadId = 29, Nombre = "Marketing y Publicidad" },
            new Carrera { Id = 168, CategoriaId = 3, FacultadId = 29, Nombre = "Economía Empresarial" },
            new Carrera { Id = 169, CategoriaId = 1, FacultadId = 30, Nombre = "Ingeniería Industrial" },
            new Carrera { Id = 170, CategoriaId = 6, FacultadId = 30, Nombre = "Ingeniería Civil" },
            new Carrera { Id = 171, CategoriaId = 1, FacultadId = 30, Nombre = "Ingeniería en Sistemas de Información" },
            new Carrera { Id = 172, CategoriaId = 1, FacultadId = 31, Nombre = "Strategic Marketing" },
            new Carrera { Id = 173, CategoriaId = 1, FacultadId = 31, Nombre = "Global Finance" },
            new Carrera { Id = 174, CategoriaId = 1, FacultadId = 31, Nombre = "Global Management" },
            new Carrera { Id = 175, CategoriaId = 1, FacultadId = 31, Nombre = "Global Business" },
            new Carrera { Id = 176, CategoriaId = 1, FacultadId = 31, Nombre = "Natural Resource Management" },
            new Carrera { Id = 177, CategoriaId = 8, FacultadId = 32, Nombre = "Derecho" },
            new Carrera { Id = 178, CategoriaId = 3, FacultadId = 32, Nombre = "Administración de Empresas" },
            new Carrera { Id = 179, CategoriaId = 3, FacultadId = 32, Nombre = "Economía" },
            new Carrera { Id = 180, CategoriaId = 12, FacultadId = 32, Nombre = "Psicología" },
            new Carrera { Id = 181, CategoriaId = 11, FacultadId = 32, Nombre = "Comunicación Social" },
            new Carrera { Id = 182, CategoriaId = 6, FacultadId = 32, Nombre = "Arquitectura" },
            new Carrera { Id = 183, CategoriaId = 6, FacultadId = 32, Nombre = "Ingeniería Civil" },
            new Carrera { Id = 184, CategoriaId = 1, FacultadId = 32, Nombre = "Ingeniería Industrial" },
            new Carrera { Id = 185, CategoriaId = 1, FacultadId = 32, Nombre = "Ingeniería en Sistemas" },
            new Carrera { Id = 186, CategoriaId = 9, FacultadId = 33, Nombre = "Ingeniería Agronómica" },
            new Carrera { Id = 187, CategoriaId = 9, FacultadId = 33, Nombre = "Ingeniería Agroindustrial" },
            new Carrera { Id = 188, CategoriaId = 9, FacultadId = 33, Nombre = "Ingeniería en Zootecnia" },
            new Carrera { Id = 189, CategoriaId = 9, FacultadId = 33, Nombre = "Medicina Veterinaria" },
            new Carrera { Id = 190, CategoriaId = 4, FacultadId = 34, Nombre = "Medicina Intercultural" },
            new Carrera { Id = 191, CategoriaId = 9, FacultadId = 34, Nombre = "Medicina Veterinaria" },
            new Carrera { Id = 192, CategoriaId = 4, FacultadId = 34, Nombre = "Licenciatura en Farmacia" },
            new Carrera { Id = 193, CategoriaId = 4, FacultadId = 34, Nombre = "Licenciatura en Bioanálisis Clínico" },
            new Carrera { Id = 194, CategoriaId = 4, FacultadId = 34, Nombre = "Licenciatura en Enfermería" },
            new Carrera { Id = 195, CategoriaId = 4, FacultadId = 34, Nombre = "Técnico Superior en Enfermería Intercultural" },
            new Carrera { Id = 196, CategoriaId = 3, FacultadId = 35, Nombre = "Licenciatura en Contabilidad Pública y Auditoría" },
            new Carrera { Id = 197, CategoriaId = 13, FacultadId = 35, Nombre = "Licenciatura en Administración de Empresas con mención en Turismo y Hotelería" },
            new Carrera { Id = 198, CategoriaId = 3, FacultadId = 35, Nombre = "Licenciatura en Administración de Empresas con mención en Comercio Internacional" },
            new Carrera { Id = 199, CategoriaId = 8, FacultadId = 35, Nombre = "Licenciatura en Derecho" },
            new Carrera { Id = 200, CategoriaId = 10, FacultadId = 36, Nombre = "Ciencias de la Educación con mención en Inglés" },
            new Carrera { Id = 201, CategoriaId = 5, FacultadId = 36, Nombre = "Ciencias de la Educación con mención en Ciencias Sociales" },
            new Carrera { Id = 202, CategoriaId = 5, FacultadId = 36, Nombre = "Ciencias de la Educación con mención en Ciencias Naturales" },
            new Carrera { Id = 203, CategoriaId = 5, FacultadId = 36, Nombre = "Ciencias de la Educación con mención en Física-Matemática" },
            new Carrera { Id = 204, CategoriaId = 5, FacultadId = 36, Nombre = "Licenciatura en Educación Intercultural Bilingüe" },
            new Carrera { Id = 205, CategoriaId = 12, FacultadId = 36, Nombre = "Licenciatura en Lingüística Intercultural" },
            new Carrera { Id = 206, CategoriaId = 9, FacultadId = 37, Nombre = "Ingeniería Agronómica" },
            new Carrera { Id = 207, CategoriaId = 9, FacultadId = 37, Nombre = "Ingeniería Agroforestal" },
            new Carrera { Id = 208, CategoriaId = 9, FacultadId = 37, Nombre = "Ingeniería en Zootecnia" },
            new Carrera { Id = 209, CategoriaId = 9, FacultadId = 37, Nombre = "Ingeniería en Pesca" },
            new Carrera { Id = 210, CategoriaId = 9, FacultadId = 37, Nombre = "Técnico Superior en Pesca" },
            new Carrera { Id = 211, CategoriaId = 13, FacultadId = 37, Nombre = "Técnico Superior en Ecoturismo" },
            new Carrera { Id = 212, CategoriaId = 1, FacultadId = 38, Nombre = "Ingeniería de Sistemas" },
            new Carrera { Id = 213, CategoriaId = 6, FacultadId = 38, Nombre = "Ingeniería Civil" },
            new Carrera { Id = 214, CategoriaId = 1, FacultadId = 38, Nombre = "Ingeniería Industrial" },
            new Carrera { Id = 215, CategoriaId = 1, FacultadId = 38, Nombre = "Ingeniería en Computación" },
            new Carrera { Id = 216, CategoriaId = 12, FacultadId = 39, Nombre = "Licenciatura en Psicología en Contextos Multiculturales" },
            new Carrera { Id = 217, CategoriaId = 12, FacultadId = 39, Nombre = "Licenciatura en Sociología" },
            new Carrera { Id = 218, CategoriaId = 11, FacultadId = 39, Nombre = "Licenciatura en Comunicación Intercultural" },
            new Carrera { Id = 219, CategoriaId = 12, FacultadId = 39, Nombre = "Técnico Superior en Gestión Cultural" },
            new Carrera { Id = 220, CategoriaId = 9, FacultadId = 40, Nombre = "Ingeniería Agroforestal" },
            new Carrera { Id = 221, CategoriaId = 4, FacultadId = 40, Nombre = "Licenciatura en Enfermería Profesional" },
            new Carrera { Id = 222, CategoriaId = 3, FacultadId = 40, Nombre = "Licenciatura en Contaduría Pública y Finanzas" },
            new Carrera { Id = 223, CategoriaId = 8, FacultadId = 40, Nombre = "Licenciatura en Derecho" },
            new Carrera { Id = 224, CategoriaId = 5, FacultadId = 40, Nombre = "Licenciatura en Ciencias de la Educación" },
            new Carrera { Id = 225, CategoriaId = 6, FacultadId = 40, Nombre = "Técnico Superior en Construcción Civil" },
            new Carrera { Id = 226, CategoriaId = 1, FacultadId = 40, Nombre = "Técnico Superior en Geología" },
            new Carrera { Id = 227, CategoriaId = 3, FacultadId = 41, Nombre = "Administración de Empresas" },
            new Carrera { Id = 228, CategoriaId = 8, FacultadId = 42, Nombre = "Derecho" },
            new Carrera { Id = 229, CategoriaId = 12, FacultadId = 42, Nombre = "Psicología" },
            new Carrera { Id = 230, CategoriaId = 4, FacultadId = 43, Nombre = "Medicina" },
            new Carrera { Id = 231, CategoriaId = 4, FacultadId = 43, Nombre = "Enfermería" },
            new Carrera { Id = 232, CategoriaId = 6, FacultadId = 44, Nombre = "Ingeniería Civil" },
            new Carrera { Id = 233, CategoriaId = 9, FacultadId = 45, Nombre = "Medicina Veterinaria" },
            new Carrera { Id = 234, CategoriaId = 6, FacultadId = 46, Nombre = "Arquitectura" },
            new Carrera { Id = 235, CategoriaId = 11, FacultadId = 46, Nombre = "Diseño Gráfico" },
            new Carrera { Id = 236, CategoriaId = 11, FacultadId = 46, Nombre = "Diseño de Interiores" },
            new Carrera { Id = 237, CategoriaId = 6, FacultadId = 46, Nombre = "Ingeniería Civil" },
            new Carrera { Id = 238, CategoriaId = 3, FacultadId = 47, Nombre = "Administración de Empresas" },
            new Carrera { Id = 239, CategoriaId = 3, FacultadId = 47, Nombre = "Contaduría Pública" },
            new Carrera { Id = 240, CategoriaId = 8, FacultadId = 47, Nombre = "Derecho" },
            new Carrera { Id = 241, CategoriaId = 1, FacultadId = 47, Nombre = "Informática" },
            new Carrera { Id = 242, CategoriaId = 3, FacultadId = 47, Nombre = "Mercadotecnia" },
            new Carrera { Id = 243, CategoriaId = 12, FacultadId = 47, Nombre = "Psicología" },
            new Carrera { Id = 244, CategoriaId = 13, FacultadId = 47, Nombre = "Turismo" },
            new Carrera { Id = 245, CategoriaId = 3, FacultadId = 48, Nombre = "Administración de Empresas" },
            new Carrera { Id = 246, CategoriaId = 1, FacultadId = 48, Nombre = "Informática" },
            new Carrera { Id = 247, CategoriaId = 8, FacultadId = 48, Nombre = "Derecho" },
            new Carrera { Id = 248, CategoriaId = 3, FacultadId = 48, Nombre = "Contaduría Pública" },
            new Carrera { Id = 249, CategoriaId = 3, FacultadId = 48, Nombre = "Mercadotecnia" },
            new Carrera { Id = 250, CategoriaId = 13, FacultadId = 49, Nombre = "Técnico General en Cocina y Gastronomía" },
            new Carrera { Id = 251, CategoriaId = 13, FacultadId = 49, Nombre = "Técnico General en Pastelería y Panadería" },
            new Carrera { Id = 252, CategoriaId = 13, FacultadId = 49, Nombre = "Técnico General en Servicio de Restaurante, Bar y Cafetería" },
            new Carrera { Id = 253, CategoriaId = 13, FacultadId = 49, Nombre = "Técnico Especialista en Dirección de Alimentos y Bebidas en Hotelería" },
            new Carrera { Id = 254, CategoriaId = 13, FacultadId = 49, Nombre = "Técnico Especialista en Atención al Cliente y Recepción Hotelera" },
            new Carrera { Id = 255, CategoriaId = 1, FacultadId = 49, Nombre = "Técnico Especialista en Guía de Turista Nacional" },
            new Carrera { Id = 256, CategoriaId = 13, FacultadId = 49, Nombre = "Técnico Especialista en Hotelería Rural" },
            new Carrera { Id = 257, CategoriaId = 13, FacultadId = 49, Nombre = "Técnico General en Gestión del Departamento de Habitaciones en Hotelería" },
            new Carrera { Id = 258, CategoriaId = 1, FacultadId = 49, Nombre = "Técnico Especialista en Guía de Turista por Espacios Naturales" },
            new Carrera { Id = 259, CategoriaId = 1, FacultadId = 49, Nombre = "Bachillerato Técnico en Guía de Turista" },
            new Carrera { Id = 260, CategoriaId = 13, FacultadId = 49, Nombre = "Técnico General en Administración de Empresas Turísticas y Hoteleras" },
            new Carrera { Id = 261, CategoriaId = 3, FacultadId = 50, Nombre = "Técnico General en Asistente Ejecutivo" },
            new Carrera { Id = 262, CategoriaId = 3, FacultadId = 50, Nombre = "Técnico General en Administración" },
            new Carrera { Id = 263, CategoriaId = 3, FacultadId = 50, Nombre = "Técnico General en Gestión Aduanera" },
            new Carrera { Id = 264, CategoriaId = 3, FacultadId = 50, Nombre = "Técnico General en Gestión de Recursos Humanos" },
            new Carrera { Id = 265, CategoriaId = 3, FacultadId = 50, Nombre = "Bachillerato Técnico en Administración" },
            new Carrera { Id = 266, CategoriaId = 3, FacultadId = 50, Nombre = "Bachillerato Técnico en Asistente Ejecutivo" },
            new Carrera { Id = 267, CategoriaId = 11, FacultadId = 50, Nombre = "Técnico Especialista en Marketing y Publicidad" },
            new Carrera { Id = 268, CategoriaId = 5, FacultadId = 51, Nombre = "Técnico Especialista en Entrenamiento Físico y Deportivo" },
            new Carrera { Id = 269, CategoriaId = 5, FacultadId = 51, Nombre = "Técnico General en Entrenamiento de Deporte en Combate" },
            new Carrera { Id = 270, CategoriaId = 5, FacultadId = 51, Nombre = "Técnico General en Entrenamiento de Deporte en Balón y Pelota" },
            new Carrera { Id = 271, CategoriaId = 5, FacultadId = 52, Nombre = "Técnico Especialista en Docencia de Educación Técnica y Formación Profesional" },
            new Carrera { Id = 272, CategoriaId = 10, FacultadId = 52, Nombre = "Técnico Especialista en Didáctica del Idioma Inglés" },
            new Carrera { Id = 273, CategoriaId = 3, FacultadId = 53, Nombre = "Técnico Especialista en Banca y Finanzas" },
            new Carrera { Id = 274, CategoriaId = 3, FacultadId = 53, Nombre = "Técnico General en Contabilidad" },
            new Carrera { Id = 275, CategoriaId = 3, FacultadId = 53, Nombre = "Bachillerato Técnico en Contabilidad" },
            new Carrera { Id = 276, CategoriaId = 1, FacultadId = 54, Nombre = "Técnico General en Computación" },
            new Carrera { Id = 277, CategoriaId = 1, FacultadId = 54, Nombre = "Técnico Especialista en Programación" },
            new Carrera { Id = 278, CategoriaId = 1, FacultadId = 54, Nombre = "Técnico Especialista en Seguridad Informática" },
            new Carrera { Id = 279, CategoriaId = 11, FacultadId = 54, Nombre = "Técnico Especialista en Diseño Gráfico" },
            new Carrera { Id = 280, CategoriaId = 1, FacultadId = 54, Nombre = "Bachillerato Técnico en Computación" },
            new Carrera { Id = 281, CategoriaId = 5, FacultadId = 54, Nombre = "Técnico Especialista en Tecnología Educativa" },
            new Carrera { Id = 282, CategoriaId = 10, FacultadId = 54, Nombre = "Técnico Especialista en Inglés" },
            new Carrera { Id = 283, CategoriaId = 14, FacultadId = 55, Nombre = "Técnico General en Mecánica Automotriz de Vehículos Livianos Diésel y Gasolina" },
            new Carrera { Id = 284, CategoriaId = 1, FacultadId = 55, Nombre = "Técnico General en Enderezado y Pintura" },
            new Carrera { Id = 285, CategoriaId = 14, FacultadId = 55, Nombre = "Técnico Especialista en Administración de Flota Vehicular" },
            new Carrera { Id = 286, CategoriaId = 14, FacultadId = 55, Nombre = "Técnico General en Mecánica de Motocicleta" },
            new Carrera { Id = 287, CategoriaId = 14, FacultadId = 55, Nombre = "Técnico General en Mecánica de Vehículos Pesados" },
            new Carrera { Id = 288, CategoriaId = 6, FacultadId = 56, Nombre = "Técnico General en Topografía" },
            new Carrera { Id = 289, CategoriaId = 6, FacultadId = 56, Nombre = "Técnico General en Construcción Civil" },
            new Carrera { Id = 290, CategoriaId = 6, FacultadId = 56, Nombre = "Técnico General en Maestro de Obras en Construcciones Verticales" },
            new Carrera { Id = 291, CategoriaId = 6, FacultadId = 56, Nombre = "Técnico Especialista en Dibujo Arquitectónico" },
            new Carrera { Id = 292, CategoriaId = 6, FacultadId = 56, Nombre = "Bachillerato Técnico en Maestro de Obras en Construcciones Verticales" },
            new Carrera { Id = 293, CategoriaId = 11, FacultadId = 57, Nombre = "Técnico General en Diseño y Elaboración de Productos de Cuero" },
            new Carrera { Id = 294, CategoriaId = 14, FacultadId = 58, Nombre = "Técnico General en Electricidad Industrial" },
            new Carrera { Id = 295, CategoriaId = 14, FacultadId = 58, Nombre = "Técnico General en Instalaciones Eléctricas Automatizadas" },
            new Carrera { Id = 296, CategoriaId = 1, FacultadId = 58, Nombre = "Técnico General en Instalación y Mantenimiento de Equipos Electrónicos Médicos" },
            new Carrera { Id = 297, CategoriaId = 14, FacultadId = 58, Nombre = "Técnico General en Instalaciones de Telecomunicaciones" },
            new Carrera { Id = 298, CategoriaId = 14, FacultadId = 58, Nombre = "Técnico General en Electrónica" },
            new Carrera { Id = 299, CategoriaId = 14, FacultadId = 58, Nombre = "Bachillerato Técnico en Electrónica" },
            new Carrera { Id = 300, CategoriaId = 14, FacultadId = 58, Nombre = "Bachillerato Técnico en Electricidad Industrial" },
            new Carrera { Id = 301, CategoriaId = 1, FacultadId = 59, Nombre = "Técnico General en Energías Renovables" },
            new Carrera { Id = 302, CategoriaId = 14, FacultadId = 60, Nombre = "Técnico General en Fabricación de Productos de Madera" },
            new Carrera { Id = 303, CategoriaId = 14, FacultadId = 61, Nombre = "Técnico General en Corte y Soldadura" },
            new Carrera { Id = 304, CategoriaId = 14, FacultadId = 61, Nombre = "Técnico Especialista en Mantenimiento Industrial" },
            new Carrera { Id = 305, CategoriaId = 14, FacultadId = 61, Nombre = "Técnico General en Operación de Máquinas Herramientas" },
            new Carrera { Id = 306, CategoriaId = 14, FacultadId = 61, Nombre = "Técnico Especialista en Supervisión de Procesos Industriales" },
            new Carrera { Id = 307, CategoriaId = 14, FacultadId = 61, Nombre = "Técnico Especialista en Higiene y Seguridad Industrial" },
            new Carrera { Id = 308, CategoriaId = 9, FacultadId = 62, Nombre = "Técnico General en Pesca" },
            new Carrera { Id = 309, CategoriaId = 14, FacultadId = 62, Nombre = "Técnico General en Mecánica Naval" },
            new Carrera { Id = 310, CategoriaId = 14, FacultadId = 63, Nombre = "Técnico Especialista en Análisis Químico Industrial" },
            new Carrera { Id = 311, CategoriaId = 14, FacultadId = 64, Nombre = "Técnico General en Refrigeración y Aire Acondicionado Comercial" },
            new Carrera { Id = 312, CategoriaId = 11, FacultadId = 65, Nombre = "Técnico General en Diseño, Corte y Confección" },
            new Carrera { Id = 313, CategoriaId = 1, FacultadId = 66, Nombre = "Técnico General en Procesamiento de Aceite de Palma" },
            new Carrera { Id = 314, CategoriaId = 1, FacultadId = 66, Nombre = "Técnico General en Producción de Palma Aceitera" },
            new Carrera { Id = 315, CategoriaId = 9, FacultadId = 67, Nombre = "Técnico General en Agroindustria de los Alimentos" },
            new Carrera { Id = 316, CategoriaId = 9, FacultadId = 67, Nombre = "Técnico General en Agroindustria del Café" },
            new Carrera { Id = 317, CategoriaId = 9, FacultadId = 67, Nombre = "Técnico General en Transformación de Productos Apícolas" },
            new Carrera { Id = 318, CategoriaId = 1, FacultadId = 67, Nombre = "Técnico General en Procesamiento de Productos Lácteos" },
            new Carrera { Id = 319, CategoriaId = 9, FacultadId = 67, Nombre = "Técnico General en Procesamiento de Productos Cárnicos, Pescados y Mariscos" },
            new Carrera { Id = 320, CategoriaId = 1, FacultadId = 67, Nombre = "Técnico General en Procesamiento de Productos de Granos, Frutas y Hortalizas" },
            new Carrera { Id = 321, CategoriaId = 9, FacultadId = 68, Nombre = "Técnico General Agropecuario" },
            new Carrera { Id = 322, CategoriaId = 1, FacultadId = 68, Nombre = "Técnico General en Agronomía" },
            new Carrera { Id = 323, CategoriaId = 9, FacultadId = 68, Nombre = "Técnico General en Zootecnia" },
            new Carrera { Id = 324, CategoriaId = 9, FacultadId = 68, Nombre = "Técnico General en Acuicultura" },
            new Carrera { Id = 325, CategoriaId = 9, FacultadId = 68, Nombre = "Técnico Especialista en Gestión de Fincas Ganaderas" },
            new Carrera { Id = 326, CategoriaId = 9, FacultadId = 68, Nombre = "Técnico General en Producción Apícola" },
            new Carrera { Id = 327, CategoriaId = 9, FacultadId = 68, Nombre = "Técnico General en Riego Agrícola" },
            new Carrera { Id = 328, CategoriaId = 9, FacultadId = 68, Nombre = "Bachillerato Técnico Agropecuario" },
            new Carrera { Id = 329, CategoriaId = 9, FacultadId = 68, Nombre = "Técnico General Agrícola en Café" },
            new Carrera { Id = 330, CategoriaId = 9, FacultadId = 69, Nombre = "Técnico Especialista Forestal" },
            new Carrera { Id = 331, CategoriaId = 9, FacultadId = 70, Nombre = "Técnico General en Veterinaria" }
        );

        builder.Entity<Ubicacion>().HasData(
            new Ubicacion { Id = 1, Departamento = "Managua", Ciudad = "Managua" },
            new Ubicacion { Id = 2, Departamento = "León", Ciudad = "León" },
            new Ubicacion { Id = 3, Departamento = "Granada", Ciudad = "Granada" },
            new Ubicacion { Id = 4, Departamento = "Masaya", Ciudad = "Masaya" },
            new Ubicacion { Id = 5, Departamento = "Chinandega", Ciudad = "Chinandega" }
        );

        builder.Entity<ConfiguracionSistema>().HasData(
            new ConfiguracionSistema { Id = 1, Clave = ConfiguracionSistema.Claves.PaqueteVacantesPrecio, Valor = "20.00", Descripcion = "Precio en USD del paquete de publicación de vacantes" },
            new ConfiguracionSistema { Id = 2, Clave = ConfiguracionSistema.Claves.PaqueteVacantesCantidad, Valor = "10", Descripcion = "Cantidad de vacantes incluidas por paquete" },
            new ConfiguracionSistema { Id = 3, Clave = ConfiguracionSistema.Claves.PaqueteVacantesDiasVigencia, Valor = "30", Descripcion = "Días de vigencia del paquete desde la aprobación" }
        );

        // ---------- Estados de postulación (catálogo) ----------
        builder.Entity<EstadoPostulacion>(entity =>
        {
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Property(e => e.Nombre).HasMaxLength(50).IsRequired();
        });

        // ---------- Postulaciones ----------
        builder.Entity<Postulacion>(entity =>
        {
            entity.HasIndex(p => new { p.CandidatoId, p.VacanteId }).IsUnique(); // evita doble postulación

            entity.HasOne(p => p.Candidato)
                  .WithMany()
                  .HasForeignKey(p => p.CandidatoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Vacante)
                  .WithMany()
                  .HasForeignKey(p => p.VacanteId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.EstadoPostulacion)
                  .WithMany()
                  .HasForeignKey(p => p.EstadoPostulacionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Pasantías (módulo independiente de Vacantes de empleo) ----------
        builder.Entity<Pasantia>(entity =>
        {
            entity.Property(p => p.Titulo).HasMaxLength(150).IsRequired();
            entity.Property(p => p.Descripcion).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(p => p.Requisitos).HasColumnType("nvarchar(max)");
            entity.Property(p => p.Modalidad).HasMaxLength(30).IsRequired();
            entity.Property(p => p.MontoRemuneracion).HasColumnType("decimal(10,2)");
            entity.Property(p => p.Estado).HasMaxLength(20).IsRequired();

            entity.HasIndex(p => new { p.CategoriaId, p.UbicacionId, p.Estado });
            entity.HasIndex(p => p.EmpresaId);

            entity.HasOne(p => p.Empresa)
                  .WithMany()
                  .HasForeignKey(p => p.EmpresaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Categoria)
                  .WithMany()
                  .HasForeignKey(p => p.CategoriaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Ubicacion)
                  .WithMany()
                  .HasForeignKey(p => p.UbicacionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PostulacionPasantia>(entity =>
        {
            entity.Property(p => p.Estado).HasMaxLength(20).IsRequired();
            entity.HasIndex(p => new { p.CandidatoId, p.PasantiaId }).IsUnique();

            entity.HasOne(p => p.Candidato)
                  .WithMany()
                  .HasForeignKey(p => p.CandidatoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Pasantia)
                  .WithMany(pa => pa.Postulaciones)
                  .HasForeignKey(p => p.PasantiaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Favoritos (N:M con clave compuesta) ----------
        builder.Entity<Favorito>(entity =>
        {
            entity.HasKey(f => new { f.CandidatoId, f.VacanteId });

            entity.HasOne(f => f.Candidato)
                  .WithMany()
                  .HasForeignKey(f => f.CandidatoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Vacante)
                  .WithMany()
                  .HasForeignKey(f => f.VacanteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EstadoPostulacion>().HasData(
            new EstadoPostulacion { Id = 1, Nombre = EstadoPostulacion.Nombres.Postulado },
            new EstadoPostulacion { Id = 2, Nombre = EstadoPostulacion.Nombres.EnRevision },
            new EstadoPostulacion { Id = 3, Nombre = EstadoPostulacion.Nombres.Entrevista },
            new EstadoPostulacion { Id = 4, Nombre = EstadoPostulacion.Nombres.Contratado },
            new EstadoPostulacion { Id = 5, Nombre = EstadoPostulacion.Nombres.Rechazado }
        );

        // ---------- Auditoría ----------
        builder.Entity<Auditoria>(entity =>
        {
            entity.Property(a => a.Accion).HasMaxLength(100).IsRequired();
            entity.Property(a => a.EntidadAfectada).HasMaxLength(100).IsRequired();
            entity.Property(a => a.EntidadId).HasMaxLength(50).IsRequired();
            entity.Property(a => a.DetalleJson).HasColumnType("nvarchar(max)");
            entity.HasIndex(a => a.FechaHora);
        });

        // ---------- Video CV (extensión de Candidato) ----------
        builder.Entity<Candidato>(entity =>
        {
            entity.Property(c => c.VideoCvUrl).HasMaxLength(300);

            // NoAction (en vez de SetNull) porque Candidatos ya tiene otra FK
            // hacia Archivos (CvArchivoId) con SetNull — SQL Server no permite
            // dos rutas de acción en cascada desde la misma tabla hacia el
            // mismo destino ("multiple cascade paths"). Con NoAction, si se
            // elimina el Archivo del video, hay que limpiar VideoCvArchivoId
            // manualmente antes (ya lo hace el service al reemplazar el video).
            entity.HasOne(c => c.VideoCvArchivo)
                  .WithMany()
                  .HasForeignKey(c => c.VideoCvArchivoId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // ---------- Idiomas (catálogo) ----------
        builder.Entity<Idioma>(entity =>
        {
            entity.HasIndex(i => i.Nombre).IsUnique();
            entity.Property(i => i.Nombre).HasMaxLength(60).IsRequired();
        });

        // ---------- CandidatoIdioma (N:M con clave compuesta) ----------
        builder.Entity<CandidatoIdioma>(entity =>
        {
            entity.HasKey(ci => new { ci.CandidatoId, ci.IdiomaId });
            entity.Property(ci => ci.Nivel).HasMaxLength(20).IsRequired();

            entity.HasOne(ci => ci.Candidato)
                  .WithMany(c => c.Idiomas)
                  .HasForeignKey(ci => ci.CandidatoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ci => ci.Idioma)
                  .WithMany(i => i.Candidatos)
                  .HasForeignKey(ci => ci.IdiomaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Certificados (PDF adjuntos) ----------
        builder.Entity<Certificado>(entity =>
        {
            entity.Property(c => c.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(c => c.InstitucionEmisora).HasMaxLength(200);
            entity.Property(c => c.TipoDocumento).HasMaxLength(50).IsRequired();

            entity.HasOne(c => c.Candidato)
                  .WithMany(c => c.Certificados)
                  .HasForeignKey(c => c.CandidatoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Archivo)
                  .WithMany()
                  .HasForeignKey(c => c.ArchivoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Cursos ----------
        builder.Entity<Curso>(entity =>
        {
            entity.Property(c => c.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Institucion).HasMaxLength(200);

            entity.HasOne(c => c.Candidato)
                  .WithMany(c => c.Cursos)
                  .HasForeignKey(c => c.CandidatoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Archivo)
                  .WithMany()
                  .HasForeignKey(c => c.ArchivoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ---------- Postulación: ranking y seguimiento ----------
        builder.Entity<Postulacion>(entity =>
        {
            entity.Property(p => p.PuntajeCompatibilidad).HasColumnType("decimal(5,2)");
            entity.Property(p => p.NotaEntrevista).HasMaxLength(1000);
            entity.Property(p => p.SalaVideollamadaId).HasMaxLength(60);
            // Ranking automático: índice para ordenar por compatibilidad descendente por vacante.
            entity.HasIndex(p => new { p.VacanteId, p.PuntajeCompatibilidad });
        });

        // ---------- Comentarios sobre el Video CV ----------
        builder.Entity<ComentarioVideoCv>(entity =>
        {
            entity.Property(c => c.ReclutadorUsuarioId).HasMaxLength(450).IsRequired();
            entity.Property(c => c.ReclutadorNombre).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Comentario).HasMaxLength(1000).IsRequired();
            entity.HasIndex(c => c.PostulacionId);

            entity.HasOne(c => c.Postulacion)
                  .WithMany(p => p.ComentariosVideoCv)
                  .HasForeignKey(c => c.PostulacionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Empresa: perfil enriquecido ----------
        builder.Entity<Empresa>(entity =>
        {
            entity.Property(e => e.Historia).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Mision).HasMaxLength(2000);
            entity.Property(e => e.Vision).HasMaxLength(2000);
            entity.Property(e => e.CulturaOrganizacional).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Beneficios).HasColumnType("nvarchar(max)");
            entity.Property(e => e.NumeroColaboradores).HasMaxLength(20);
            entity.Property(e => e.FacebookUrl).HasMaxLength(250);
            entity.Property(e => e.InstagramUrl).HasMaxLength(250);
            entity.Property(e => e.LinkedInUrl).HasMaxLength(250);
            entity.Property(e => e.TiktokUrl).HasMaxLength(250);
        });

        // ---------- Galería de la empresa (fotos/videos) ----------
        builder.Entity<EmpresaGaleria>(entity =>
        {
            entity.Property(g => g.TipoMedio).HasMaxLength(10).IsRequired();
            entity.Property(g => g.UrlVideo).HasMaxLength(300);
            entity.Property(g => g.Titulo).HasMaxLength(150);
            entity.HasIndex(g => new { g.EmpresaId, g.Orden });

            entity.HasOne(g => g.Empresa)
                  .WithMany(e => e.Galeria)
                  .HasForeignKey(g => g.EmpresaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(g => g.Archivo)
                  .WithMany()
                  .HasForeignKey(g => g.ArchivoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Notificaciones (centro de notificaciones 🔔) ----------
        builder.Entity<Notificacion>(entity =>
        {
            entity.Property(n => n.Tipo).HasMaxLength(50).IsRequired();
            entity.Property(n => n.Mensaje).HasMaxLength(300).IsRequired();
            entity.Property(n => n.UrlDestino).HasMaxLength(300);
            entity.HasIndex(n => new { n.UsuarioId, n.Leida, n.FechaCreacion });
        });

        // Datos semilla del catálogo de idiomas más comunes en Nicaragua/región.
        builder.Entity<Idioma>().HasData(
            new Idioma { Id = 1, Nombre = "Español" },
            new Idioma { Id = 2, Nombre = "Inglés" },
            new Idioma { Id = 3, Nombre = "Francés" },
            new Idioma { Id = 4, Nombre = "Portugués" },
            new Idioma { Id = 5, Nombre = "Italiano" }
        );

        // ---------- Tickets de soporte ----------
        builder.Entity<TicketSoporte>(entity =>
        {
            entity.Property(t => t.NombreContacto).HasMaxLength(150).IsRequired();
            entity.Property(t => t.CorreoContacto).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Asunto).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Mensaje).HasMaxLength(2000).IsRequired();
            entity.Property(t => t.Estado).HasMaxLength(20).IsRequired();
            entity.Property(t => t.RespuestaAdmin).HasMaxLength(2000);
            entity.HasIndex(t => new { t.Estado, t.FechaCreacion });
        });

        // ---------- Mensajes del chat de soporte ----------
        builder.Entity<MensajeSoporte>(entity =>
        {
            entity.Property(m => m.AutorNombre).HasMaxLength(150).IsRequired();
            entity.Property(m => m.Mensaje).HasMaxLength(2000).IsRequired();
            entity.HasOne(m => m.Ticket)
                  .WithMany()
                  .HasForeignKey(m => m.TicketSoporteId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(m => new { m.TicketSoporteId, m.FechaEnvio });
        });

        // ---------- Preguntas psicométricas (catálogo) ----------
        builder.Entity<PreguntaPsicometrica>(entity =>
        {
            entity.Property(p => p.Texto).HasMaxLength(300).IsRequired();
            entity.Property(p => p.Rasgo).HasMaxLength(50).IsRequired();
        });

        // ---------- Evaluaciones psicométricas ----------
        builder.Entity<EvaluacionPsicometrica>(entity =>
        {
            entity.Property(e => e.Mensaje).HasMaxLength(1000);
            entity.Property(e => e.Estado).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.PostulacionId).IsUnique(); // una evaluación por postulación

            entity.HasOne(e => e.Postulacion)
                  .WithMany()
                  .HasForeignKey(e => e.PostulacionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Respuestas psicométricas ----------
        builder.Entity<RespuestaPsicometrica>(entity =>
        {
            entity.HasKey(r => new { r.EvaluacionId, r.PreguntaId });

            entity.HasOne(r => r.Evaluacion)
                  .WithMany(e => e.Respuestas)
                  .HasForeignKey(r => r.EvaluacionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Pregunta)
                  .WithMany()
                  .HasForeignKey(r => r.PreguntaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PreguntaPsicometrica>().HasData(
            new PreguntaPsicometrica { Id = 1, Orden = 1, Rasgo = RasgosBigFive.Responsabilidad, EsInversa = false, Texto = "Cumplo mis tareas y compromisos a tiempo." },
            new PreguntaPsicometrica { Id = 2, Orden = 2, Rasgo = RasgosBigFive.Responsabilidad, EsInversa = false, Texto = "Presto atención a los detalles en mi trabajo." },
            new PreguntaPsicometrica { Id = 3, Orden = 3, Rasgo = RasgosBigFive.Responsabilidad, EsInversa = true, Texto = "Suelo dejar las cosas para el último momento." },
            new PreguntaPsicometrica { Id = 4, Orden = 4, Rasgo = RasgosBigFive.Responsabilidad, EsInversa = false, Texto = "Mantengo mis pertenencias y espacio de trabajo organizados." },
            new PreguntaPsicometrica { Id = 5, Orden = 5, Rasgo = RasgosBigFive.Responsabilidad, EsInversa = true, Texto = "Me cuesta seguir un plan hasta el final." },

            new PreguntaPsicometrica { Id = 6, Orden = 6, Rasgo = RasgosBigFive.Extraversion, EsInversa = false, Texto = "Me siento cómodo iniciando conversaciones con personas nuevas." },
            new PreguntaPsicometrica { Id = 7, Orden = 7, Rasgo = RasgosBigFive.Extraversion, EsInversa = false, Texto = "Disfruto ser el centro de atención en reuniones o eventos." },
            new PreguntaPsicometrica { Id = 8, Orden = 8, Rasgo = RasgosBigFive.Extraversion, EsInversa = true, Texto = "Prefiero trabajar solo antes que en equipo." },
            new PreguntaPsicometrica { Id = 9, Orden = 9, Rasgo = RasgosBigFive.Extraversion, EsInversa = false, Texto = "Tengo energía de sobra para socializar después de una jornada larga." },
            new PreguntaPsicometrica { Id = 10, Orden = 10, Rasgo = RasgosBigFive.Extraversion, EsInversa = true, Texto = "Me cuesta expresar mis ideas frente a un grupo." },

            new PreguntaPsicometrica { Id = 11, Orden = 11, Rasgo = RasgosBigFive.Amabilidad, EsInversa = false, Texto = "Me preocupo genuinamente por el bienestar de mis compañeros." },
            new PreguntaPsicometrica { Id = 12, Orden = 12, Rasgo = RasgosBigFive.Amabilidad, EsInversa = false, Texto = "Estoy dispuesto a ceder en una discusión para mantener la armonía." },
            new PreguntaPsicometrica { Id = 13, Orden = 13, Rasgo = RasgosBigFive.Amabilidad, EsInversa = true, Texto = "Suelo desconfiar de las intenciones de los demás." },
            new PreguntaPsicometrica { Id = 14, Orden = 14, Rasgo = RasgosBigFive.Amabilidad, EsInversa = false, Texto = "Ayudo a otros incluso cuando no me beneficia directamente." },
            new PreguntaPsicometrica { Id = 15, Orden = 15, Rasgo = RasgosBigFive.Amabilidad, EsInversa = true, Texto = "Me resulta difícil ponerme en el lugar de otra persona." },

            new PreguntaPsicometrica { Id = 16, Orden = 16, Rasgo = RasgosBigFive.Apertura, EsInversa = false, Texto = "Disfruto aprender sobre temas que no conozco." },
            new PreguntaPsicometrica { Id = 17, Orden = 17, Rasgo = RasgosBigFive.Apertura, EsInversa = false, Texto = "Me gusta probar formas nuevas de resolver un problema." },
            new PreguntaPsicometrica { Id = 18, Orden = 18, Rasgo = RasgosBigFive.Apertura, EsInversa = true, Texto = "Prefiero métodos probados antes que experimentar con algo nuevo." },
            new PreguntaPsicometrica { Id = 19, Orden = 19, Rasgo = RasgosBigFive.Apertura, EsInversa = false, Texto = "Me interesa el arte, la cultura o las ideas abstractas." },
            new PreguntaPsicometrica { Id = 20, Orden = 20, Rasgo = RasgosBigFive.Apertura, EsInversa = true, Texto = "Me incomoda salir de mi rutina habitual." },

            new PreguntaPsicometrica { Id = 21, Orden = 21, Rasgo = RasgosBigFive.EstabilidadEmocional, EsInversa = false, Texto = "Mantengo la calma incluso bajo presión." },
            new PreguntaPsicometrica { Id = 22, Orden = 22, Rasgo = RasgosBigFive.EstabilidadEmocional, EsInversa = false, Texto = "Me recupero rápido después de un contratiempo." },
            new PreguntaPsicometrica { Id = 23, Orden = 23, Rasgo = RasgosBigFive.EstabilidadEmocional, EsInversa = true, Texto = "Me preocupo con facilidad por cosas que podrían salir mal." },
            new PreguntaPsicometrica { Id = 24, Orden = 24, Rasgo = RasgosBigFive.EstabilidadEmocional, EsInversa = false, Texto = "Rara vez pierdo la paciencia con los demás." },
            new PreguntaPsicometrica { Id = 25, Orden = 25, Rasgo = RasgosBigFive.EstabilidadEmocional, EsInversa = true, Texto = "Los cambios inesperados me generan mucha ansiedad." }
        );

        // ---------- CRM: CrmEmpresa / CrmActividad ----------
        builder.Entity<CrmEmpresa>(entity =>
        {
            entity.Property(e => e.NombreEmpresa).HasMaxLength(150).IsRequired();
            entity.Property(e => e.ContactoPrincipal).HasMaxLength(150);
            entity.Property(e => e.Telefono).HasMaxLength(30);
            entity.Property(e => e.Correo).HasMaxLength(200);
            entity.Property(e => e.RUC).HasMaxLength(30);
            entity.Property(e => e.Direccion).HasMaxLength(300);
            entity.Property(e => e.SectorEmpresarial).HasMaxLength(100);
            entity.Property(e => e.TamanoEmpresa).HasMaxLength(30);
            entity.Property(e => e.SitioWeb).HasMaxLength(200);
            entity.Property(e => e.RedesSociales).HasMaxLength(500);
            entity.Property(e => e.Observaciones).HasMaxLength(2000);
            entity.Property(e => e.Etapa).HasConversion<string>().HasMaxLength(30);
            entity.HasIndex(e => e.Etapa);
            entity.HasOne(e => e.Empresa)
                  .WithMany()
                  .HasForeignKey(e => e.EmpresaId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<CrmActividad>(entity =>
        {
            entity.Property(a => a.Descripcion).HasMaxLength(2000).IsRequired();
            entity.Property(a => a.UsuarioNombre).HasMaxLength(150);
            entity.Property(a => a.Tipo).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(a => new { a.CrmEmpresaId, a.FechaActividad });
            entity.HasOne(a => a.CrmEmpresa)
                  .WithMany(e => e.Actividades)
                  .HasForeignKey(a => a.CrmEmpresaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CrmArchivoAdjunto>(entity =>
        {
            entity.Property(a => a.NombreOriginal).HasMaxLength(255).IsRequired();
            entity.Property(a => a.RutaArchivo).HasMaxLength(500).IsRequired();
            entity.Property(a => a.Descripcion).HasMaxLength(300);
            entity.HasOne(a => a.CrmEmpresa)
                  .WithMany(e => e.ArchivosAdjuntos)
                  .HasForeignKey(a => a.CrmEmpresaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ChatConversacion>(entity =>
        {
            entity.Property(c => c.RolUsuario).HasMaxLength(20).IsRequired();
            entity.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(c => new { c.UsuarioId, c.Estado });
        });

        builder.Entity<ChatMensaje>(entity =>
        {
            entity.Property(m => m.Texto).HasMaxLength(2000).IsRequired();
            entity.Property(m => m.UrlAccion).HasMaxLength(300);
            entity.Property(m => m.Emisor).HasConversion<string>().HasMaxLength(10);
            entity.HasIndex(m => new { m.ConversacionId, m.FechaEnvio });
            entity.HasOne(m => m.Conversacion)
                  .WithMany(c => c.Mensajes)
                  .HasForeignKey(m => m.ConversacionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Módulo Contabilidad y Finanzas ----------
        builder.Entity<CategoriaFinanciera>(entity =>
        {
            entity.Property(c => c.Nombre).HasMaxLength(150).IsRequired();
            entity.Property(c => c.Descripcion).HasMaxLength(400);
            entity.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(c => new { c.Tipo, c.Nombre }).IsUnique();
        });

        builder.Entity<IngresoFinanciero>(entity =>
        {
            // Regla dura: un pago jamás genera dos ingresos.
            entity.HasIndex(i => i.PagoId).IsUnique();

            entity.Property(i => i.Monto).HasColumnType("decimal(18,2)");
            entity.ToTable(t => t.HasCheckConstraint("CK_IngresoFinanciero_Monto", "[Monto] > 0"));
            entity.Property(i => i.Moneda).HasConversion<string>().HasMaxLength(10);
            entity.Property(i => i.Estado).HasConversion<string>().HasMaxLength(15);
            entity.Property(i => i.MetodoPago).HasMaxLength(80);
            entity.Property(i => i.Referencia).HasMaxLength(150);
            entity.Property(i => i.GeneradoPor).HasMaxLength(100);
            entity.Property(i => i.MotivoAnulacion).HasMaxLength(500);
            entity.HasIndex(i => new { i.PeriodoFinancieroId, i.Estado });
            entity.HasIndex(i => i.EmpresaId);

            entity.HasOne(i => i.Pago).WithMany().HasForeignKey(i => i.PagoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(i => i.Empresa).WithMany().HasForeignKey(i => i.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(i => i.PlanSuscripcion).WithMany().HasForeignKey(i => i.PlanSuscripcionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(i => i.CategoriaFinanciera).WithMany().HasForeignKey(i => i.CategoriaFinancieraId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(i => i.PeriodoFinanciero).WithMany().HasForeignKey(i => i.PeriodoFinancieroId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<GastoFinanciero>(entity =>
        {
            entity.Property(g => g.Descripcion).HasMaxLength(300).IsRequired();
            entity.Property(g => g.Proveedor).HasMaxLength(200);
            entity.Property(g => g.Monto).HasColumnType("decimal(18,2)");
            entity.ToTable(t => t.HasCheckConstraint("CK_GastoFinanciero_Monto", "[Monto] > 0"));
            entity.Property(g => g.Moneda).HasConversion<string>().HasMaxLength(10);
            entity.Property(g => g.Estado).HasConversion<string>().HasMaxLength(15);
            entity.Property(g => g.MetodoPago).HasMaxLength(80);
            entity.Property(g => g.NumeroReferencia).HasMaxLength(150);
            entity.Property(g => g.MotivoAnulacion).HasMaxLength(500);
            entity.HasIndex(g => new { g.PeriodoFinancieroId, g.Estado });

            entity.HasOne(g => g.CategoriaFinanciera).WithMany().HasForeignKey(g => g.CategoriaFinancieraId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(g => g.PeriodoFinanciero).WithMany().HasForeignKey(g => g.PeriodoFinancieroId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(g => g.ArchivoComprobante).WithMany().HasForeignKey(g => g.ArchivoComprobanteId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<PeriodoFinanciero>(entity =>
        {
            entity.Property(p => p.Nombre).HasMaxLength(50).IsRequired();
            entity.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(10);
            entity.HasIndex(p => new { p.Tipo, p.Anio, p.Mes }).IsUnique();
        });

        builder.Entity<AjusteFinanciero>(entity =>
        {
            entity.Property(a => a.Motivo).HasMaxLength(500).IsRequired();
            entity.Property(a => a.MontoAnterior).HasColumnType("decimal(18,2)");
            entity.Property(a => a.MontoNuevo).HasColumnType("decimal(18,2)");
            entity.Property(a => a.TipoEntidad).HasConversion<string>().HasMaxLength(10);
            entity.HasIndex(a => new { a.TipoEntidad, a.EntidadId });
        });

        builder.Entity<AuditoriaFinanciera>(entity =>
        {
            entity.Property(a => a.Accion).HasMaxLength(30).IsRequired();
            entity.Property(a => a.Modulo).HasMaxLength(50).IsRequired();
            entity.Property(a => a.RegistroAfectado).HasMaxLength(150).IsRequired();
            entity.Property(a => a.ValorAnterior).HasMaxLength(2000);
            entity.Property(a => a.ValorNuevo).HasMaxLength(2000);
            entity.Property(a => a.DireccionIp).HasMaxLength(45);
            entity.Property(a => a.Resultado).HasMaxLength(15);
            entity.HasIndex(a => new { a.Modulo, a.FechaHora });
        });

        // Las configuraciones Fluent API de Contactos, etc.
        // se añaden en los próximos módulos.
    }
}
