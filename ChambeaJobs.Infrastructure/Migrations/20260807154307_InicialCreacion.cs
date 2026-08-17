using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChambeaJobs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InicialCreacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Archivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NombreOriginal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PesoBytes = table.Column<int>(type: "int", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archivos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EstadoCuenta = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntidadAfectada = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntidadId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DetalleJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadosPostulacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosPostulacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Habilidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habilidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Idiomas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Idiomas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instituciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instituciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    UrlDestino = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Leida = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanesSuscripcion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    VacantesIncluidas = table.Column<int>(type: "int", nullable: true),
                    DiasVigencia = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    IncluyePruebaPsicometrica = table.Column<bool>(type: "bit", nullable: false),
                    IncluyeVideoCv = table.Column<bool>(type: "bit", nullable: false),
                    PermiteVacantesDestacadas = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesSuscripcion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreguntasPsicometricas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Texto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Rasgo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EsInversa = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreguntasPsicometricas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketsSoporte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreContacto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CorreoContacto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Asunto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RespuestaAdmin = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AdminUsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRespuesta = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketsSoporte", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ubicaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Departamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ciudad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ubicaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Candidatos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CvArchivoId = table.Column<int>(type: "int", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Disponibilidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VideoCvUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    VideoCvArchivoId = table.Column<int>(type: "int", nullable: true),
                    VideoCvFechaSubida = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidatos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Candidatos_Archivos_CvArchivoId",
                        column: x => x.CvArchivoId,
                        principalTable: "Archivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Candidatos_Archivos_VideoCvArchivoId",
                        column: x => x.VideoCvArchivoId,
                        principalTable: "Archivos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Facultades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstitucionId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facultades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facultades_Instituciones_InstitucionId",
                        column: x => x.InstitucionId,
                        principalTable: "Instituciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MensajesSoporte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketSoporteId = table.Column<int>(type: "int", nullable: false),
                    AutorUsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AutorNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensajesSoporte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MensajesSoporte_TicketsSoporte_TicketSoporteId",
                        column: x => x.TicketSoporteId,
                        principalTable: "TicketsSoporte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NombreEmpresa = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RUC = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LogoArchivoId = table.Column<int>(type: "int", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SitioWeb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UbicacionId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Historia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mision = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Vision = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CulturaOrganizacional = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Beneficios = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroColaboradores = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FacebookUrl = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InstagramUrl = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TiktokUrl = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Empresas_Archivos_LogoArchivoId",
                        column: x => x.LogoArchivoId,
                        principalTable: "Archivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Empresas_Ubicaciones_UbicacionId",
                        column: x => x.UbicacionId,
                        principalTable: "Ubicaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CandidatoHabilidades",
                columns: table => new
                {
                    CandidatoId = table.Column<int>(type: "int", nullable: false),
                    HabilidadId = table.Column<int>(type: "int", nullable: false),
                    NivelDominio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidatoHabilidades", x => new { x.CandidatoId, x.HabilidadId });
                    table.ForeignKey(
                        name: "FK_CandidatoHabilidades_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidatoHabilidades_Habilidades_HabilidadId",
                        column: x => x.HabilidadId,
                        principalTable: "Habilidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CandidatoIdiomas",
                columns: table => new
                {
                    CandidatoId = table.Column<int>(type: "int", nullable: false),
                    IdiomaId = table.Column<int>(type: "int", nullable: false),
                    Nivel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidatoIdiomas", x => new { x.CandidatoId, x.IdiomaId });
                    table.ForeignKey(
                        name: "FK_CandidatoIdiomas_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidatoIdiomas_Idiomas_IdiomaId",
                        column: x => x.IdiomaId,
                        principalTable: "Idiomas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Certificados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidatoId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InstitucionEmisora = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaObtencion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TipoDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ArchivoId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificados_Archivos_ArchivoId",
                        column: x => x.ArchivoId,
                        principalTable: "Archivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificados_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cursos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidatoId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Institucion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HorasDuracion = table.Column<int>(type: "int", nullable: true),
                    FechaFinalizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cursos_Archivos_ArchivoId",
                        column: x => x.ArchivoId,
                        principalTable: "Archivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Cursos_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExperienciasLaborales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidatoId = table.Column<int>(type: "int", nullable: false),
                    NombreEmpresa = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Puesto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperienciasLaborales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExperienciasLaborales_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carreras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FacultadId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carreras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carreras_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Carreras_Facultades_FacultadId",
                        column: x => x.FacultadId,
                        principalTable: "Facultades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmpresaGalerias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    TipoMedio = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ArchivoId = table.Column<int>(type: "int", nullable: true),
                    UrlVideo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaGalerias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpresaGalerias_Archivos_ArchivoId",
                        column: x => x.ArchivoId,
                        principalTable: "Archivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpresaGalerias_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaquetesEmpresa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    PlanSuscripcionId = table.Column<int>(type: "int", nullable: false),
                    FechaCompra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VacantesIncluidas = table.Column<int>(type: "int", nullable: true),
                    VacantesConsumidas = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EsPruebaGratis = table.Column<bool>(type: "bit", nullable: false),
                    RenovacionAutomatica = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaquetesEmpresa", x => x.Id);
                    table.CheckConstraint("CK_PaquetesEmpresa_Consumo", "[VacantesConsumidas] <= [VacantesIncluidas] OR [VacantesIncluidas] IS NULL");
                    table.ForeignKey(
                        name: "FK_PaquetesEmpresa_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaquetesEmpresa_PlanesSuscripcion_PlanSuscripcionId",
                        column: x => x.PlanSuscripcionId,
                        principalTable: "PlanesSuscripcion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pasantias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    UbicacionId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Requisitos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modalidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DuracionMeses = table.Column<int>(type: "int", nullable: false),
                    EsRemunerada = table.Column<bool>(type: "bit", nullable: false),
                    MontoRemuneracion = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pasantias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pasantias_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pasantias_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pasantias_Ubicaciones_UbicacionId",
                        column: x => x.UbicacionId,
                        principalTable: "Ubicaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Educaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidatoId = table.Column<int>(type: "int", nullable: false),
                    Institucion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    InstitucionId = table.Column<int>(type: "int", nullable: true),
                    TituloObtenido = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NivelEducativo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CategoriaId = table.Column<int>(type: "int", nullable: true),
                    CarreraId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Educaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Educaciones_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Educaciones_Carreras_CarreraId",
                        column: x => x.CarreraId,
                        principalTable: "Carreras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Educaciones_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Educaciones_Instituciones_InstitucionId",
                        column: x => x.InstitucionId,
                        principalTable: "Instituciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaqueteEmpresaId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 20.00m),
                    MetodoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenciaTransaccion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    EstadoPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ComentarioRevision = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaRevision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdOrdenPayPal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_PaquetesEmpresa_PaqueteEmpresaId",
                        column: x => x.PaqueteEmpresaId,
                        principalTable: "PaquetesEmpresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vacantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    CarreraId = table.Column<int>(type: "int", nullable: true),
                    UbicacionId = table.Column<int>(type: "int", nullable: false),
                    PaqueteEmpresaId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Requisitos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modalidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SalarioMin = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    SalarioMax = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ExperienciaRequerida = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EsDestacada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vacantes_Carreras_CarreraId",
                        column: x => x.CarreraId,
                        principalTable: "Carreras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Vacantes_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vacantes_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vacantes_PaquetesEmpresa_PaqueteEmpresaId",
                        column: x => x.PaqueteEmpresaId,
                        principalTable: "PaquetesEmpresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vacantes_Ubicaciones_UbicacionId",
                        column: x => x.UbicacionId,
                        principalTable: "Ubicaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostulacionesPasantia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidatoId = table.Column<int>(type: "int", nullable: false),
                    PasantiaId = table.Column<int>(type: "int", nullable: false),
                    FechaPostulacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NotaEmpresa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostulacionesPasantia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostulacionesPasantia_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostulacionesPasantia_Pasantias_PasantiaId",
                        column: x => x.PasantiaId,
                        principalTable: "Pasantias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Favoritos",
                columns: table => new
                {
                    CandidatoId = table.Column<int>(type: "int", nullable: false),
                    VacanteId = table.Column<int>(type: "int", nullable: false),
                    FechaAgregado = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritos", x => new { x.CandidatoId, x.VacanteId });
                    table.ForeignKey(
                        name: "FK_Favoritos_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoritos_Vacantes_VacanteId",
                        column: x => x.VacanteId,
                        principalTable: "Vacantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Postulaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidatoId = table.Column<int>(type: "int", nullable: false),
                    VacanteId = table.Column<int>(type: "int", nullable: false),
                    EstadoPostulacionId = table.Column<int>(type: "int", nullable: false),
                    FechaPostulacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionEstado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PuntajeCompatibilidad = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CvRevisado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCvRevisado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VideoCvVisto = table.Column<bool>(type: "bit", nullable: false),
                    FechaVideoCvVisto = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PruebaPsicometricaAprobada = table.Column<bool>(type: "bit", nullable: true),
                    PruebaPsicometricaPuntaje = table.Column<int>(type: "int", nullable: true),
                    FechaPruebaPsicometrica = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EntrevistaProgramada = table.Column<bool>(type: "bit", nullable: false),
                    FechaEntrevista = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotaEntrevista = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SalaVideollamadaId = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    RecordatorioEntrevistaEnviado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postulaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Postulaciones_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Postulaciones_EstadosPostulacion_EstadoPostulacionId",
                        column: x => x.EstadoPostulacionId,
                        principalTable: "EstadosPostulacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Postulaciones_Vacantes_VacanteId",
                        column: x => x.VacanteId,
                        principalTable: "Vacantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComentariosVideoCv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostulacionId = table.Column<int>(type: "int", nullable: false),
                    ReclutadorUsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReclutadorNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Calificacion = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComentariosVideoCv", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComentariosVideoCv_Postulaciones_PostulacionId",
                        column: x => x.PostulacionId,
                        principalTable: "Postulaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluacionesPsicometricas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostulacionId = table.Column<int>(type: "int", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaLimite = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCompletado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PuntajeResponsabilidad = table.Column<int>(type: "int", nullable: true),
                    PuntajeExtraversion = table.Column<int>(type: "int", nullable: true),
                    PuntajeAmabilidad = table.Column<int>(type: "int", nullable: true),
                    PuntajeApertura = table.Column<int>(type: "int", nullable: true),
                    PuntajeEstabilidadEmocional = table.Column<int>(type: "int", nullable: true),
                    PuntajeCompatibilidad = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluacionesPsicometricas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluacionesPsicometricas_Postulaciones_PostulacionId",
                        column: x => x.PostulacionId,
                        principalTable: "Postulaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RespuestasPsicometricas",
                columns: table => new
                {
                    EvaluacionId = table.Column<int>(type: "int", nullable: false),
                    PreguntaId = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespuestasPsicometricas", x => new { x.EvaluacionId, x.PreguntaId });
                    table.ForeignKey(
                        name: "FK_RespuestasPsicometricas_EvaluacionesPsicometricas_EvaluacionId",
                        column: x => x.EvaluacionId,
                        principalTable: "EvaluacionesPsicometricas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RespuestasPsicometricas_PreguntasPsicometricas_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "PreguntasPsicometricas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Desarrollo de software, TI, soporte técnico", "Tecnología" },
                    { 2, "Ventas, atención comercial", "Ventas" },
                    { 3, "Roles administrativos y de oficina", "Administración" },
                    { 4, "Personal médico y asistencial", "Salud" },
                    { 5, "Docencia y formación", "Educación" },
                    { 6, "Ingeniería civil y obra", "Construcción" },
                    { 7, "Call center, soporte al cliente", "Atención al Cliente" },
                    { 8, null, "Derecho" },
                    { 9, null, "Ciencias Agropecuarias y Veterinaria" },
                    { 10, null, "Idiomas" },
                    { 11, null, "Artes, Diseño y Comunicación" },
                    { 12, null, "Ciencias Sociales" },
                    { 13, null, "Turismo y Hotelería" },
                    { 14, null, "Industria y Oficios Técnicos" },
                    { 15, "Motorizados, repartidores, choferes, ayudantes de bodega", "Logística, Transporte y Mensajería" },
                    { 16, "Operarios, ayudantes de producción, empaque, ensamblaje", "Producción y Manufactura" },
                    { 17, "Limpieza, mantenimiento, jardinería, oficios varios", "Servicios Generales y Oficios" },
                    { 18, "Guardias, vigilantes, control de acceso", "Seguridad y Vigilancia" },
                    { 19, "Meseros, cocineros, ayudantes de cocina, baristas", "Gastronomía y Alimentos" },
                    { 20, "Cajeros, dependientes de tienda, bombas de gasolina", "Comercio y Ventas al Detalle" }
                });

            migrationBuilder.InsertData(
                table: "ConfiguracionesSistema",
                columns: new[] { "Id", "Clave", "Descripcion", "Valor" },
                values: new object[,]
                {
                    { 1, "PaqueteVacantesPrecio", "Precio en USD del paquete de publicación de vacantes", "20.00" },
                    { 2, "PaqueteVacantesCantidad", "Cantidad de vacantes incluidas por paquete", "10" },
                    { 3, "PaqueteVacantesDiasVigencia", "Días de vigencia del paquete desde la aprobación", "30" }
                });

            migrationBuilder.InsertData(
                table: "EstadosPostulacion",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Postulado" },
                    { 2, "En revisión" },
                    { 3, "Entrevista" },
                    { 4, "Contratado" },
                    { 5, "Rechazado" }
                });

            migrationBuilder.InsertData(
                table: "Idiomas",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Español" },
                    { 2, "Inglés" },
                    { 3, "Francés" },
                    { 4, "Portugués" },
                    { 5, "Italiano" }
                });

            migrationBuilder.InsertData(
                table: "Instituciones",
                columns: new[] { "Id", "Nombre", "Tipo" },
                values: new object[,]
                {
                    { 1, "UNAN-Managua", "Universidad" },
                    { 2, "UNAN-León", "Universidad" },
                    { 3, "UNI - Universidad Nacional de Ingeniería", "Universidad" },
                    { 4, "UNA - Universidad Nacional Agraria", "Universidad" },
                    { 5, "UCA - Universidad Centroamericana (cancelada en 2023 — ver UNCSM)", "Universidad" },
                    { 6, "UNP - Universidad Nacional Politécnica (antes UPOLI)", "Universidad" },
                    { 7, "UAM - Universidad Americana", "Universidad" },
                    { 8, "Keiser University", "Universidad" },
                    { 9, "UCC - Universidad de Ciencias Comerciales", "Universidad" },
                    { 10, "INATEC - Instituto Nacional Tecnológico", "Técnico" },
                    { 11, "Universidad Nacional Casimiro Sotelo Montenegro (UNCSM)", "Universidad" },
                    { 12, "Universidad Nacional Francisco Luis Espinoza Pineda (UNFLEP)", "Universidad" },
                    { 13, "URACCAN - Universidad de las Regiones Autónomas de la Costa Caribe Nicaragüense", "Universidad" },
                    { 14, "BICU - Bluefields Indian and Caribbean University", "Universidad" },
                    { 15, "UCN - Universidad Central de Nicaragua", "Universidad" },
                    { 16, "UNICIT - Universidad Iberoamericana de Ciencia y Tecnología", "Universidad" },
                    { 17, "UCYT - Universidad Nicaragüense de Ciencia y Tecnología", "Universidad" }
                });

            migrationBuilder.InsertData(
                table: "PlanesSuscripcion",
                columns: new[] { "Id", "Activo", "DiasVigencia", "IncluyePruebaPsicometrica", "IncluyeVideoCv", "Nombre", "PermiteVacantesDestacadas", "Precio", "VacantesIncluidas" },
                values: new object[,]
                {
                    { 1, true, 30, false, false, "Básico", false, 20.00m, 10 },
                    { 2, true, 30, true, true, "Empresarial", true, 50.00m, null }
                });

            migrationBuilder.InsertData(
                table: "PreguntasPsicometricas",
                columns: new[] { "Id", "EsInversa", "Orden", "Rasgo", "Texto" },
                values: new object[,]
                {
                    { 1, false, 1, "Responsabilidad", "Cumplo mis tareas y compromisos a tiempo." },
                    { 2, false, 2, "Responsabilidad", "Presto atención a los detalles en mi trabajo." },
                    { 3, true, 3, "Responsabilidad", "Suelo dejar las cosas para el último momento." },
                    { 4, false, 4, "Responsabilidad", "Mantengo mis pertenencias y espacio de trabajo organizados." },
                    { 5, true, 5, "Responsabilidad", "Me cuesta seguir un plan hasta el final." },
                    { 6, false, 6, "Extraversión", "Me siento cómodo iniciando conversaciones con personas nuevas." },
                    { 7, false, 7, "Extraversión", "Disfruto ser el centro de atención en reuniones o eventos." },
                    { 8, true, 8, "Extraversión", "Prefiero trabajar solo antes que en equipo." },
                    { 9, false, 9, "Extraversión", "Tengo energía de sobra para socializar después de una jornada larga." },
                    { 10, true, 10, "Extraversión", "Me cuesta expresar mis ideas frente a un grupo." },
                    { 11, false, 11, "Amabilidad", "Me preocupo genuinamente por el bienestar de mis compañeros." },
                    { 12, false, 12, "Amabilidad", "Estoy dispuesto a ceder en una discusión para mantener la armonía." },
                    { 13, true, 13, "Amabilidad", "Suelo desconfiar de las intenciones de los demás." },
                    { 14, false, 14, "Amabilidad", "Ayudo a otros incluso cuando no me beneficia directamente." },
                    { 15, true, 15, "Amabilidad", "Me resulta difícil ponerme en el lugar de otra persona." },
                    { 16, false, 16, "Apertura", "Disfruto aprender sobre temas que no conozco." },
                    { 17, false, 17, "Apertura", "Me gusta probar formas nuevas de resolver un problema." },
                    { 18, true, 18, "Apertura", "Prefiero métodos probados antes que experimentar con algo nuevo." },
                    { 19, false, 19, "Apertura", "Me interesa el arte, la cultura o las ideas abstractas." },
                    { 20, true, 20, "Apertura", "Me incomoda salir de mi rutina habitual." },
                    { 21, false, 21, "Estabilidad Emocional", "Mantengo la calma incluso bajo presión." },
                    { 22, false, 22, "Estabilidad Emocional", "Me recupero rápido después de un contratiempo." },
                    { 23, true, 23, "Estabilidad Emocional", "Me preocupo con facilidad por cosas que podrían salir mal." },
                    { 24, false, 24, "Estabilidad Emocional", "Rara vez pierdo la paciencia con los demás." },
                    { 25, true, 25, "Estabilidad Emocional", "Los cambios inesperados me generan mucha ansiedad." }
                });

            migrationBuilder.InsertData(
                table: "Ubicaciones",
                columns: new[] { "Id", "Ciudad", "Departamento" },
                values: new object[,]
                {
                    { 1, "Managua", "Managua" },
                    { 2, "León", "León" },
                    { 3, "Granada", "Granada" },
                    { 4, "Masaya", "Masaya" },
                    { 5, "Chinandega", "Chinandega" }
                });

            migrationBuilder.InsertData(
                table: "Facultades",
                columns: new[] { "Id", "InstitucionId", "Nombre" },
                values: new object[,]
                {
                    { 1, 1, "Facultad de Ciencias e Ingeniería" },
                    { 2, 1, "Facultad de Educación e Idiomas" },
                    { 3, 1, "Facultad de Humanidades y Ciencias Jurídicas" },
                    { 4, 1, "Facultad de Ciencias Económicas" },
                    { 5, 1, "Facultad de Ciencias Médicas / Instituto Politécnico de la Salud (POLISAL)" },
                    { 6, 2, "Ciencias Jurídicas y Sociales" },
                    { 7, 2, "Ciencias de la Salud" },
                    { 8, 2, "Ciencias Químicas" },
                    { 9, 2, "Odontología" },
                    { 10, 2, "Ciencias y Tecnología" },
                    { 11, 2, "Ciencias de la Educación y Humanidades" },
                    { 12, 2, "Ciencias Económicas y Empresariales" },
                    { 13, 2, "Ciencias Agrarias y Veterinaria" },
                    { 14, 3, "Oferta académica 2025-2026" },
                    { 15, 4, "Facultad de Agronomía" },
                    { 16, 4, "Facultad de Ciencia Animal" },
                    { 17, 4, "Facultad de Desarrollo Rural" },
                    { 18, 6, "Administración, Comercio y Finanzas" },
                    { 19, 6, "Derecho" },
                    { 20, 6, "Diseño y Comunicación" },
                    { 21, 6, "Enfermería" },
                    { 22, 6, "Ingeniería" },
                    { 23, 6, "Educación, Arte y Humanidades" },
                    { 24, 7, "Facultad de Medicina" },
                    { 25, 7, "Facultad de Odontología" },
                    { 26, 7, "Facultad de Ciencias Médicas" },
                    { 27, 7, "Facultad de Ciencias Jurídicas y Relaciones Internacionales" },
                    { 28, 7, "Facultad de Diseño, Arquitectura y Ciencias de la Comunicación" },
                    { 29, 7, "Facultad de Ciencias Administrativas y Económicas" },
                    { 30, 7, "Facultad de Ingeniería y Arquitectura" },
                    { 31, 7, "UAM College (programas internacionales en inglés)" },
                    { 32, 11, "Carreras reportadas" },
                    { 33, 12, "Carreras" },
                    { 34, 13, "Área de Salud y Servicio Social" },
                    { 35, 13, "Área de Ciencias Económicas y Jurídicas" },
                    { 36, 13, "Área de Educación" },
                    { 37, 13, "Área de Agricultura" },
                    { 38, 13, "Área de Ciencias, Tecnologías e Ingenierías" },
                    { 39, 13, "Área de Ciencias Sociales, Humanidades y Artes" },
                    { 40, 14, "Carreras reportadas" },
                    { 41, 15, "Ciencias Administrativas" },
                    { 42, 15, "Ciencias Jurídicas y Sociales" },
                    { 43, 15, "Ciencias Médicas" },
                    { 44, 15, "Ingeniería" },
                    { 45, 15, "Medicina Veterinaria" },
                    { 46, 16, "Facultad de Ingeniería y Arquitectura" },
                    { 47, 16, "Otras facultades" },
                    { 48, 17, "Carreras reportadas" },
                    { 49, 10, "Comercio y Servicios — Hotelería y Turismo" },
                    { 50, 10, "Comercio y Servicios — Administración" },
                    { 51, 10, "Comercio y Servicios — Actividades Físicas y Deportivas" },
                    { 52, 10, "Comercio y Servicios — Docencia" },
                    { 53, 10, "Comercio y Servicios — Finanzas" },
                    { 54, 10, "Comercio y Servicios — Informática" },
                    { 55, 10, "Industria y Construcción — Automotriz" },
                    { 56, 10, "Industria y Construcción — Construcción" },
                    { 57, 10, "Industria y Construcción — Cuero y Calzado" },
                    { 58, 10, "Industria y Construcción — Electricidad y Electrónica" },
                    { 59, 10, "Industria y Construcción — Energías Renovables" },
                    { 60, 10, "Industria y Construcción — Madera Mueble" },
                    { 61, 10, "Industria y Construcción — Metal Mecánica" },
                    { 62, 10, "Industria y Construcción — Pesca" },
                    { 63, 10, "Industria y Construcción — Química" },
                    { 64, 10, "Industria y Construcción — Refrigeración" },
                    { 65, 10, "Industria y Construcción — Textil-Vestuario" },
                    { 66, 10, "Industria y Construcción — Producción de Palma" },
                    { 67, 10, "Agropecuario y Forestal — Agroindustria de los Alimentos" },
                    { 68, 10, "Agropecuario y Forestal — Agropecuaria" },
                    { 69, 10, "Agropecuario y Forestal — Forestal" },
                    { 70, 10, "Agropecuario y Forestal — Veterinaria" }
                });

            migrationBuilder.InsertData(
                table: "Carreras",
                columns: new[] { "Id", "CategoriaId", "FacultadId", "Nombre" },
                values: new object[,]
                {
                    { 1, 6, 1, "Arquitectura" },
                    { 2, 1, 1, "Licenciatura en Biología" },
                    { 3, 1, 1, "Licenciatura en Ciencias Naturales" },
                    { 4, 1, 1, "Ingeniería Ambiental" },
                    { 5, 1, 1, "Licenciatura en Química Ambiental" },
                    { 6, 1, 1, "Licenciatura en Química Farmacéutica" },
                    { 7, 1, 1, "Licenciatura en Química Industrial" },
                    { 8, 1, 1, "Ingeniería en Ciencias de la Computación" },
                    { 9, 6, 1, "Ingeniería Civil" },
                    { 10, 1, 1, "Ingeniería Electrónica" },
                    { 11, 1, 1, "Ingeniería en Sistemas de la Información" },
                    { 12, 1, 1, "Ingeniería Industrial" },
                    { 13, 1, 1, "Ingeniería Estadística" },
                    { 14, 1, 1, "Ingeniería en Geofísica" },
                    { 15, 1, 1, "Ingeniería Geológica" },
                    { 16, 1, 1, "Ingeniería en Energías Renovables" },
                    { 17, 1, 1, "Ingeniería en Telemática" },
                    { 18, 1, 1, "Física" },
                    { 19, 1, 1, "Física Médica" },
                    { 20, 1, 1, "Física con mención en Geofísica" },
                    { 21, 6, 1, "Técnico Superior en Construcción" },
                    { 22, 6, 1, "Técnico Superior en Topografía" },
                    { 23, 1, 2, "Licenciatura en Matemática" },
                    { 24, 10, 2, "Francés" },
                    { 25, 10, 2, "Inglés" },
                    { 26, 1, 2, "Lengua y Literatura Hispánicas" },
                    { 27, 5, 2, "Licenciatura en Informática Educativa" },
                    { 28, 1, 2, "Licenciatura en Innovación y Emprendimiento" },
                    { 29, 11, 2, "Licenciatura en Cultura y Artes" },
                    { 30, 11, 2, "Licenciatura en Danza" },
                    { 31, 11, 2, "Licenciatura en Diseño Gráfico y Multimedia" },
                    { 32, 5, 2, "Licenciatura en Educación Física y Deportes" },
                    { 33, 1, 2, "Licenciatura en Traducción e Interpretación de Lenguas Extranjeras" },
                    { 34, 13, 2, "Licenciatura en Turismo Sostenible" },
                    { 35, 3, 2, "Licenciatura en Administración de la Educación" },
                    { 36, 5, 2, "Pedagogía con mención en Educación para la Diversidad" },
                    { 37, 5, 2, "Licenciatura en Educación Infantil" },
                    { 38, 5, 2, "Licenciatura en Educación Primaria" },
                    { 39, 11, 2, "Licenciatura en Educación Musical" },
                    { 40, 5, 2, "Licenciatura en Educación Especial" },
                    { 41, 5, 2, "Licenciatura en Educación Comercial" },
                    { 42, 5, 2, "Ciencias de la Educación con mención en Ciencias Naturales" },
                    { 43, 5, 2, "Ciencias de la Educación con mención en Ciencias Sociales" },
                    { 44, 5, 2, "Ciencias de la Educación con mención en Física-Matemática" },
                    { 45, 10, 2, "Ciencias de la Educación con mención en Inglés" },
                    { 46, 5, 2, "Ciencias de la Educación con mención en Lengua y Literatura Hispánica" },
                    { 47, 12, 3, "Antropología Social" },
                    { 48, 12, 3, "Ciencia Política y Relaciones Internacionales" },
                    { 49, 11, 3, "Comunicación para el Desarrollo" },
                    { 50, 8, 3, "Derecho" },
                    { 51, 12, 3, "Geografía" },
                    { 52, 12, 3, "Gestión de la Información" },
                    { 53, 12, 3, "Historia" },
                    { 54, 12, 3, "Psicología" },
                    { 55, 12, 3, "Trabajo Social" },
                    { 56, 1, 3, "Ciencias Sociales" },
                    { 57, 3, 4, "Administración de Empresas" },
                    { 58, 3, 4, "Contaduría Pública y Finanzas" },
                    { 59, 3, 4, "Banca y Finanzas" },
                    { 60, 3, 4, "Economía" },
                    { 61, 9, 4, "Economía Agrícola" },
                    { 62, 3, 4, "Mercadotecnia" },
                    { 63, 4, 5, "Medicina y Cirugía" },
                    { 64, 4, 5, "Odontología" },
                    { 65, 4, 5, "Optometría Médica" },
                    { 66, 4, 5, "Licenciatura en Anestesia y Reanimación" },
                    { 67, 4, 5, "Licenciatura en Bioanálisis Clínico" },
                    { 68, 4, 5, "Fisioterapia" },
                    { 69, 4, 5, "Enfermería (Licenciatura general)" },
                    { 70, 4, 5, "Licenciatura en Enfermería Obstétrica y Perinatal" },
                    { 71, 4, 5, "Licenciatura en Enfermería en Cuidados Críticos" },
                    { 72, 4, 5, "Licenciatura en Enfermería en Salud Pública" },
                    { 73, 4, 5, "Licenciatura en Enfermería en Materno Infantil" },
                    { 74, 4, 5, "Microbiología" },
                    { 75, 4, 5, "Nutrición" },
                    { 76, 4, 5, "Técnico Superior en Citología Cervical" },
                    { 77, 4, 5, "Técnico Superior en Enfermería General" },
                    { 78, 8, 6, "Derecho" },
                    { 79, 8, 6, "Administración y Políticas Públicas" },
                    { 80, 4, 7, "Medicina" },
                    { 81, 12, 7, "Psicología" },
                    { 82, 4, 7, "Bioanálisis Clínico" },
                    { 83, 4, 7, "Ciencias de Enfermería" },
                    { 84, 4, 7, "Técnico Superior en Radiología" },
                    { 85, 4, 7, "Técnico Superior en Enfermería en Técnicas Quirúrgicas" },
                    { 86, 1, 7, "Técnico Superior en Anestesiología" },
                    { 87, 4, 7, "Técnico Superior en Citotecnología" },
                    { 88, 4, 7, "Técnico Superior en Higiene y Seguridad Laboral" },
                    { 89, 1, 8, "Ingeniería en Alimentos" },
                    { 90, 4, 8, "Farmacia" },
                    { 91, 4, 9, "Odontología" },
                    { 92, 1, 10, "Matemática" },
                    { 93, 1, 10, "Biología" },
                    { 94, 1, 10, "Ciencias Actuariales y Financieras" },
                    { 95, 1, 10, "Ingeniería en Telemática" },
                    { 96, 1, 10, "Ingeniería en Estadística" },
                    { 97, 1, 10, "Ingeniería en Sistemas" },
                    { 98, 1, 10, "Ingeniería en Tecnología de la Información con énfasis en Desarrollo Web y Marketing Digital" },
                    { 99, 11, 11, "Comunicación Social" },
                    { 100, 12, 11, "Trabajo Social" },
                    { 101, 10, 11, "Lengua Inglesa" },
                    { 102, 5, 11, "Ciencias de la Educación con mención en Lengua y Literatura" },
                    { 103, 5, 11, "Ciencias de la Educación con mención en Educación Especial Incluyente" },
                    { 104, 10, 11, "Ciencias de la Educación con mención en Inglés" },
                    { 105, 5, 11, "Ciencias de la Educación con mención en Ciencias Sociales" },
                    { 106, 5, 11, "Educación Física y Deportes" },
                    { 107, 5, 11, "Matemática Educativa y Computación" },
                    { 108, 1, 11, "Ciencias Naturales" },
                    { 109, 3, 12, "Contaduría Pública y Finanzas" },
                    { 110, 3, 12, "Administración de Empresas" },
                    { 111, 3, 12, "Economía" },
                    { 112, 1, 12, "Gestión de Empresas Turísticas" },
                    { 113, 3, 12, "Mercadotecnia" },
                    { 114, 10, 12, "Administración en Lengua Extranjera" },
                    { 115, 3, 12, "Técnico en Comercio Internacional" },
                    { 116, 3, 12, "Técnico Superior en Finanzas" },
                    { 117, 9, 13, "Ingeniería en Agroecología Tropical" },
                    { 118, 9, 13, "Ingeniería en Agronegocios" },
                    { 119, 9, 13, "Ingeniería Acuícola" },
                    { 120, 9, 13, "Medicina Veterinaria" },
                    { 121, 9, 13, "Ingeniería en Zootecnia" },
                    { 122, 9, 13, "Ingeniería Agropecuaria" },
                    { 123, 6, 14, "Arquitectura" },
                    { 124, 6, 14, "Ingeniería Civil (diurno y nocturno)" },
                    { 125, 9, 14, "Ingeniería Agrícola" },
                    { 126, 1, 14, "Ingeniería Industrial (diurno y nocturno)" },
                    { 127, 1, 14, "Ingeniería Mecánica (diurno y nocturno)" },
                    { 128, 1, 14, "Ingeniería de Sistemas" },
                    { 129, 1, 14, "Ingeniería Química" },
                    { 130, 1, 14, "Ingeniería Electrónica (diurno y nocturno)" },
                    { 131, 1, 14, "Ingeniería Eléctrica (diurno y nocturno)" },
                    { 132, 1, 14, "Ingeniería en Computación" },
                    { 133, 9, 15, "Ingeniería Agronómica" },
                    { 134, 9, 15, "Ingeniería en Sanidad Vegetal" },
                    { 135, 9, 15, "Ingeniería Agrícola" },
                    { 136, 9, 15, "Ingeniería en Agroindustria de los Alimentos" },
                    { 137, 9, 15, "Ingeniería Forestal" },
                    { 138, 9, 15, "Ingeniería en Recursos Naturales Renovables" },
                    { 139, 9, 16, "Ingeniería en Zootecnia" },
                    { 140, 9, 16, "Medicina Veterinaria" },
                    { 141, 9, 17, "Licenciatura en Agronegocios" },
                    { 142, 9, 17, "Licenciatura en Desarrollo Rural" },
                    { 143, 13, 17, "Licenciatura en Turismo Rural y Comunitario" },
                    { 144, 3, 18, "Administración de Empresas" },
                    { 145, 3, 18, "Banca y Finanzas" },
                    { 146, 8, 19, "Derecho" },
                    { 147, 11, 20, "Diseño Gráfico" },
                    { 148, 11, 20, "Diseño de Producto" },
                    { 149, 11, 20, "Diseño Integral de Comunicaciones" },
                    { 150, 11, 20, "Comunicación Social" },
                    { 151, 4, 21, "Enfermería" },
                    { 152, 1, 22, "Ingeniería Industrial" },
                    { 153, 11, 23, "Producción de Espectáculos" },
                    { 154, 11, 23, "Enseñanza Artística Musical (Conservatorio)" },
                    { 155, 10, 23, "Idiomas" },
                    { 156, 4, 24, "Medicina y Cirugía" },
                    { 157, 4, 25, "Odontología" },
                    { 158, 4, 26, "Enfermería" },
                    { 159, 8, 27, "Derecho" },
                    { 160, 12, 27, "Diplomacia y Relaciones Internacionales" },
                    { 161, 6, 28, "Arquitectura" },
                    { 162, 11, 28, "Diseño y Comunicación Visual" },
                    { 163, 11, 28, "Comunicación y Relaciones Públicas" },
                    { 164, 3, 29, "Administración de Empresas" },
                    { 165, 3, 29, "Contabilidad y Finanzas" },
                    { 166, 3, 29, "Negocios Internacionales" },
                    { 167, 11, 29, "Marketing y Publicidad" },
                    { 168, 3, 29, "Economía Empresarial" },
                    { 169, 1, 30, "Ingeniería Industrial" },
                    { 170, 6, 30, "Ingeniería Civil" },
                    { 171, 1, 30, "Ingeniería en Sistemas de Información" },
                    { 172, 1, 31, "Strategic Marketing" },
                    { 173, 1, 31, "Global Finance" },
                    { 174, 1, 31, "Global Management" },
                    { 175, 1, 31, "Global Business" },
                    { 176, 1, 31, "Natural Resource Management" },
                    { 177, 8, 32, "Derecho" },
                    { 178, 3, 32, "Administración de Empresas" },
                    { 179, 3, 32, "Economía" },
                    { 180, 12, 32, "Psicología" },
                    { 181, 11, 32, "Comunicación Social" },
                    { 182, 6, 32, "Arquitectura" },
                    { 183, 6, 32, "Ingeniería Civil" },
                    { 184, 1, 32, "Ingeniería Industrial" },
                    { 185, 1, 32, "Ingeniería en Sistemas" },
                    { 186, 9, 33, "Ingeniería Agronómica" },
                    { 187, 9, 33, "Ingeniería Agroindustrial" },
                    { 188, 9, 33, "Ingeniería en Zootecnia" },
                    { 189, 9, 33, "Medicina Veterinaria" },
                    { 190, 4, 34, "Medicina Intercultural" },
                    { 191, 9, 34, "Medicina Veterinaria" },
                    { 192, 4, 34, "Licenciatura en Farmacia" },
                    { 193, 4, 34, "Licenciatura en Bioanálisis Clínico" },
                    { 194, 4, 34, "Licenciatura en Enfermería" },
                    { 195, 4, 34, "Técnico Superior en Enfermería Intercultural" },
                    { 196, 3, 35, "Licenciatura en Contabilidad Pública y Auditoría" },
                    { 197, 13, 35, "Licenciatura en Administración de Empresas con mención en Turismo y Hotelería" },
                    { 198, 3, 35, "Licenciatura en Administración de Empresas con mención en Comercio Internacional" },
                    { 199, 8, 35, "Licenciatura en Derecho" },
                    { 200, 10, 36, "Ciencias de la Educación con mención en Inglés" },
                    { 201, 5, 36, "Ciencias de la Educación con mención en Ciencias Sociales" },
                    { 202, 5, 36, "Ciencias de la Educación con mención en Ciencias Naturales" },
                    { 203, 5, 36, "Ciencias de la Educación con mención en Física-Matemática" },
                    { 204, 5, 36, "Licenciatura en Educación Intercultural Bilingüe" },
                    { 205, 12, 36, "Licenciatura en Lingüística Intercultural" },
                    { 206, 9, 37, "Ingeniería Agronómica" },
                    { 207, 9, 37, "Ingeniería Agroforestal" },
                    { 208, 9, 37, "Ingeniería en Zootecnia" },
                    { 209, 9, 37, "Ingeniería en Pesca" },
                    { 210, 9, 37, "Técnico Superior en Pesca" },
                    { 211, 13, 37, "Técnico Superior en Ecoturismo" },
                    { 212, 1, 38, "Ingeniería de Sistemas" },
                    { 213, 6, 38, "Ingeniería Civil" },
                    { 214, 1, 38, "Ingeniería Industrial" },
                    { 215, 1, 38, "Ingeniería en Computación" },
                    { 216, 12, 39, "Licenciatura en Psicología en Contextos Multiculturales" },
                    { 217, 12, 39, "Licenciatura en Sociología" },
                    { 218, 11, 39, "Licenciatura en Comunicación Intercultural" },
                    { 219, 12, 39, "Técnico Superior en Gestión Cultural" },
                    { 220, 9, 40, "Ingeniería Agroforestal" },
                    { 221, 4, 40, "Licenciatura en Enfermería Profesional" },
                    { 222, 3, 40, "Licenciatura en Contaduría Pública y Finanzas" },
                    { 223, 8, 40, "Licenciatura en Derecho" },
                    { 224, 5, 40, "Licenciatura en Ciencias de la Educación" },
                    { 225, 6, 40, "Técnico Superior en Construcción Civil" },
                    { 226, 1, 40, "Técnico Superior en Geología" },
                    { 227, 3, 41, "Administración de Empresas" },
                    { 228, 8, 42, "Derecho" },
                    { 229, 12, 42, "Psicología" },
                    { 230, 4, 43, "Medicina" },
                    { 231, 4, 43, "Enfermería" },
                    { 232, 6, 44, "Ingeniería Civil" },
                    { 233, 9, 45, "Medicina Veterinaria" },
                    { 234, 6, 46, "Arquitectura" },
                    { 235, 11, 46, "Diseño Gráfico" },
                    { 236, 11, 46, "Diseño de Interiores" },
                    { 237, 6, 46, "Ingeniería Civil" },
                    { 238, 3, 47, "Administración de Empresas" },
                    { 239, 3, 47, "Contaduría Pública" },
                    { 240, 8, 47, "Derecho" },
                    { 241, 1, 47, "Informática" },
                    { 242, 3, 47, "Mercadotecnia" },
                    { 243, 12, 47, "Psicología" },
                    { 244, 13, 47, "Turismo" },
                    { 245, 3, 48, "Administración de Empresas" },
                    { 246, 1, 48, "Informática" },
                    { 247, 8, 48, "Derecho" },
                    { 248, 3, 48, "Contaduría Pública" },
                    { 249, 3, 48, "Mercadotecnia" },
                    { 250, 13, 49, "Técnico General en Cocina y Gastronomía" },
                    { 251, 13, 49, "Técnico General en Pastelería y Panadería" },
                    { 252, 13, 49, "Técnico General en Servicio de Restaurante, Bar y Cafetería" },
                    { 253, 13, 49, "Técnico Especialista en Dirección de Alimentos y Bebidas en Hotelería" },
                    { 254, 13, 49, "Técnico Especialista en Atención al Cliente y Recepción Hotelera" },
                    { 255, 1, 49, "Técnico Especialista en Guía de Turista Nacional" },
                    { 256, 13, 49, "Técnico Especialista en Hotelería Rural" },
                    { 257, 13, 49, "Técnico General en Gestión del Departamento de Habitaciones en Hotelería" },
                    { 258, 1, 49, "Técnico Especialista en Guía de Turista por Espacios Naturales" },
                    { 259, 1, 49, "Bachillerato Técnico en Guía de Turista" },
                    { 260, 13, 49, "Técnico General en Administración de Empresas Turísticas y Hoteleras" },
                    { 261, 3, 50, "Técnico General en Asistente Ejecutivo" },
                    { 262, 3, 50, "Técnico General en Administración" },
                    { 263, 3, 50, "Técnico General en Gestión Aduanera" },
                    { 264, 3, 50, "Técnico General en Gestión de Recursos Humanos" },
                    { 265, 3, 50, "Bachillerato Técnico en Administración" },
                    { 266, 3, 50, "Bachillerato Técnico en Asistente Ejecutivo" },
                    { 267, 11, 50, "Técnico Especialista en Marketing y Publicidad" },
                    { 268, 5, 51, "Técnico Especialista en Entrenamiento Físico y Deportivo" },
                    { 269, 5, 51, "Técnico General en Entrenamiento de Deporte en Combate" },
                    { 270, 5, 51, "Técnico General en Entrenamiento de Deporte en Balón y Pelota" },
                    { 271, 5, 52, "Técnico Especialista en Docencia de Educación Técnica y Formación Profesional" },
                    { 272, 10, 52, "Técnico Especialista en Didáctica del Idioma Inglés" },
                    { 273, 3, 53, "Técnico Especialista en Banca y Finanzas" },
                    { 274, 3, 53, "Técnico General en Contabilidad" },
                    { 275, 3, 53, "Bachillerato Técnico en Contabilidad" },
                    { 276, 1, 54, "Técnico General en Computación" },
                    { 277, 1, 54, "Técnico Especialista en Programación" },
                    { 278, 1, 54, "Técnico Especialista en Seguridad Informática" },
                    { 279, 11, 54, "Técnico Especialista en Diseño Gráfico" },
                    { 280, 1, 54, "Bachillerato Técnico en Computación" },
                    { 281, 5, 54, "Técnico Especialista en Tecnología Educativa" },
                    { 282, 10, 54, "Técnico Especialista en Inglés" },
                    { 283, 14, 55, "Técnico General en Mecánica Automotriz de Vehículos Livianos Diésel y Gasolina" },
                    { 284, 1, 55, "Técnico General en Enderezado y Pintura" },
                    { 285, 14, 55, "Técnico Especialista en Administración de Flota Vehicular" },
                    { 286, 14, 55, "Técnico General en Mecánica de Motocicleta" },
                    { 287, 14, 55, "Técnico General en Mecánica de Vehículos Pesados" },
                    { 288, 6, 56, "Técnico General en Topografía" },
                    { 289, 6, 56, "Técnico General en Construcción Civil" },
                    { 290, 6, 56, "Técnico General en Maestro de Obras en Construcciones Verticales" },
                    { 291, 6, 56, "Técnico Especialista en Dibujo Arquitectónico" },
                    { 292, 6, 56, "Bachillerato Técnico en Maestro de Obras en Construcciones Verticales" },
                    { 293, 11, 57, "Técnico General en Diseño y Elaboración de Productos de Cuero" },
                    { 294, 14, 58, "Técnico General en Electricidad Industrial" },
                    { 295, 14, 58, "Técnico General en Instalaciones Eléctricas Automatizadas" },
                    { 296, 1, 58, "Técnico General en Instalación y Mantenimiento de Equipos Electrónicos Médicos" },
                    { 297, 14, 58, "Técnico General en Instalaciones de Telecomunicaciones" },
                    { 298, 14, 58, "Técnico General en Electrónica" },
                    { 299, 14, 58, "Bachillerato Técnico en Electrónica" },
                    { 300, 14, 58, "Bachillerato Técnico en Electricidad Industrial" },
                    { 301, 1, 59, "Técnico General en Energías Renovables" },
                    { 302, 14, 60, "Técnico General en Fabricación de Productos de Madera" },
                    { 303, 14, 61, "Técnico General en Corte y Soldadura" },
                    { 304, 14, 61, "Técnico Especialista en Mantenimiento Industrial" },
                    { 305, 14, 61, "Técnico General en Operación de Máquinas Herramientas" },
                    { 306, 14, 61, "Técnico Especialista en Supervisión de Procesos Industriales" },
                    { 307, 14, 61, "Técnico Especialista en Higiene y Seguridad Industrial" },
                    { 308, 9, 62, "Técnico General en Pesca" },
                    { 309, 14, 62, "Técnico General en Mecánica Naval" },
                    { 310, 14, 63, "Técnico Especialista en Análisis Químico Industrial" },
                    { 311, 14, 64, "Técnico General en Refrigeración y Aire Acondicionado Comercial" },
                    { 312, 11, 65, "Técnico General en Diseño, Corte y Confección" },
                    { 313, 1, 66, "Técnico General en Procesamiento de Aceite de Palma" },
                    { 314, 1, 66, "Técnico General en Producción de Palma Aceitera" },
                    { 315, 9, 67, "Técnico General en Agroindustria de los Alimentos" },
                    { 316, 9, 67, "Técnico General en Agroindustria del Café" },
                    { 317, 9, 67, "Técnico General en Transformación de Productos Apícolas" },
                    { 318, 1, 67, "Técnico General en Procesamiento de Productos Lácteos" },
                    { 319, 9, 67, "Técnico General en Procesamiento de Productos Cárnicos, Pescados y Mariscos" },
                    { 320, 1, 67, "Técnico General en Procesamiento de Productos de Granos, Frutas y Hortalizas" },
                    { 321, 9, 68, "Técnico General Agropecuario" },
                    { 322, 1, 68, "Técnico General en Agronomía" },
                    { 323, 9, 68, "Técnico General en Zootecnia" },
                    { 324, 9, 68, "Técnico General en Acuicultura" },
                    { 325, 9, 68, "Técnico Especialista en Gestión de Fincas Ganaderas" },
                    { 326, 9, 68, "Técnico General en Producción Apícola" },
                    { 327, 9, 68, "Técnico General en Riego Agrícola" },
                    { 328, 9, 68, "Bachillerato Técnico Agropecuario" },
                    { 329, 9, 68, "Técnico General Agrícola en Café" },
                    { 330, 9, 69, "Técnico Especialista Forestal" },
                    { 331, 9, 70, "Técnico General en Veterinaria" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_FechaHora",
                table: "Auditorias",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_CandidatoHabilidades_HabilidadId",
                table: "CandidatoHabilidades",
                column: "HabilidadId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidatoIdiomas_IdiomaId",
                table: "CandidatoIdiomas",
                column: "IdiomaId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidatos_CvArchivoId",
                table: "Candidatos",
                column: "CvArchivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidatos_UsuarioId",
                table: "Candidatos",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Candidatos_VideoCvArchivoId",
                table: "Candidatos",
                column: "VideoCvArchivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Carreras_CategoriaId",
                table: "Carreras",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Carreras_FacultadId_Nombre",
                table: "Carreras",
                columns: new[] { "FacultadId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nombre",
                table: "Categorias",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_ArchivoId",
                table: "Certificados",
                column: "ArchivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_CandidatoId",
                table: "Certificados",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosVideoCv_PostulacionId",
                table: "ComentariosVideoCv",
                column: "PostulacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesSistema_Clave",
                table: "ConfiguracionesSistema",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_ArchivoId",
                table: "Cursos",
                column: "ArchivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_CandidatoId",
                table: "Cursos",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Educaciones_CandidatoId",
                table: "Educaciones",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Educaciones_CarreraId",
                table: "Educaciones",
                column: "CarreraId");

            migrationBuilder.CreateIndex(
                name: "IX_Educaciones_CategoriaId",
                table: "Educaciones",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Educaciones_InstitucionId",
                table: "Educaciones",
                column: "InstitucionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaGalerias_ArchivoId",
                table: "EmpresaGalerias",
                column: "ArchivoId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaGalerias_EmpresaId_Orden",
                table: "EmpresaGalerias",
                columns: new[] { "EmpresaId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_LogoArchivoId",
                table: "Empresas",
                column: "LogoArchivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_RUC",
                table: "Empresas",
                column: "RUC",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_UbicacionId",
                table: "Empresas",
                column: "UbicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_UsuarioId",
                table: "Empresas",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadosPostulacion_Nombre",
                table: "EstadosPostulacion",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionesPsicometricas_PostulacionId",
                table: "EvaluacionesPsicometricas",
                column: "PostulacionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExperienciasLaborales_CandidatoId",
                table: "ExperienciasLaborales",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Facultades_InstitucionId_Nombre",
                table: "Facultades",
                columns: new[] { "InstitucionId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_VacanteId",
                table: "Favoritos",
                column: "VacanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Habilidades_Nombre",
                table: "Habilidades",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Idiomas_Nombre",
                table: "Idiomas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instituciones_Nombre",
                table: "Instituciones",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MensajesSoporte_TicketSoporteId_FechaEnvio",
                table: "MensajesSoporte",
                columns: new[] { "TicketSoporteId", "FechaEnvio" });

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioId_Leida_FechaCreacion",
                table: "Notificaciones",
                columns: new[] { "UsuarioId", "Leida", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_PaqueteEmpresaId",
                table: "Pagos",
                column: "PaqueteEmpresaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaquetesEmpresa_EmpresaId",
                table: "PaquetesEmpresa",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PaquetesEmpresa_PlanSuscripcionId",
                table: "PaquetesEmpresa",
                column: "PlanSuscripcionId");

            migrationBuilder.CreateIndex(
                name: "IX_Pasantias_CategoriaId_UbicacionId_Estado",
                table: "Pasantias",
                columns: new[] { "CategoriaId", "UbicacionId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Pasantias_EmpresaId",
                table: "Pasantias",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pasantias_UbicacionId",
                table: "Pasantias",
                column: "UbicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Postulaciones_CandidatoId_VacanteId",
                table: "Postulaciones",
                columns: new[] { "CandidatoId", "VacanteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Postulaciones_EstadoPostulacionId",
                table: "Postulaciones",
                column: "EstadoPostulacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Postulaciones_VacanteId_PuntajeCompatibilidad",
                table: "Postulaciones",
                columns: new[] { "VacanteId", "PuntajeCompatibilidad" });

            migrationBuilder.CreateIndex(
                name: "IX_PostulacionesPasantia_CandidatoId_PasantiaId",
                table: "PostulacionesPasantia",
                columns: new[] { "CandidatoId", "PasantiaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostulacionesPasantia_PasantiaId",
                table: "PostulacionesPasantia",
                column: "PasantiaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasPsicometricas_PreguntaId",
                table: "RespuestasPsicometricas",
                column: "PreguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsSoporte_Estado_FechaCreacion",
                table: "TicketsSoporte",
                columns: new[] { "Estado", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Vacantes_CarreraId",
                table: "Vacantes",
                column: "CarreraId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacantes_CategoriaId_UbicacionId_Estado",
                table: "Vacantes",
                columns: new[] { "CategoriaId", "UbicacionId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Vacantes_EmpresaId",
                table: "Vacantes",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacantes_PaqueteEmpresaId",
                table: "Vacantes",
                column: "PaqueteEmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacantes_UbicacionId",
                table: "Vacantes",
                column: "UbicacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Auditorias");

            migrationBuilder.DropTable(
                name: "CandidatoHabilidades");

            migrationBuilder.DropTable(
                name: "CandidatoIdiomas");

            migrationBuilder.DropTable(
                name: "Certificados");

            migrationBuilder.DropTable(
                name: "ComentariosVideoCv");

            migrationBuilder.DropTable(
                name: "ConfiguracionesSistema");

            migrationBuilder.DropTable(
                name: "Cursos");

            migrationBuilder.DropTable(
                name: "Educaciones");

            migrationBuilder.DropTable(
                name: "EmpresaGalerias");

            migrationBuilder.DropTable(
                name: "ExperienciasLaborales");

            migrationBuilder.DropTable(
                name: "Favoritos");

            migrationBuilder.DropTable(
                name: "MensajesSoporte");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "PostulacionesPasantia");

            migrationBuilder.DropTable(
                name: "RespuestasPsicometricas");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Habilidades");

            migrationBuilder.DropTable(
                name: "Idiomas");

            migrationBuilder.DropTable(
                name: "TicketsSoporte");

            migrationBuilder.DropTable(
                name: "Pasantias");

            migrationBuilder.DropTable(
                name: "EvaluacionesPsicometricas");

            migrationBuilder.DropTable(
                name: "PreguntasPsicometricas");

            migrationBuilder.DropTable(
                name: "Postulaciones");

            migrationBuilder.DropTable(
                name: "Candidatos");

            migrationBuilder.DropTable(
                name: "EstadosPostulacion");

            migrationBuilder.DropTable(
                name: "Vacantes");

            migrationBuilder.DropTable(
                name: "Carreras");

            migrationBuilder.DropTable(
                name: "PaquetesEmpresa");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Facultades");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropTable(
                name: "PlanesSuscripcion");

            migrationBuilder.DropTable(
                name: "Instituciones");

            migrationBuilder.DropTable(
                name: "Archivos");

            migrationBuilder.DropTable(
                name: "Ubicaciones");
        }
    }
}
