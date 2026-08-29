using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Application.Services;
using ChambeaJobs.Infrastructure.Data;
using ChambeaJobs.Infrastructure.Identity;
using ChambeaJobs.Infrastructure.Repositories;
using ChambeaJobs.Infrastructure.Storage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ---------- Data Protection: llaves persistentes ----------
// Sin esto, ASP.NET Core genera las llaves de cifrado de cookies/tokens en
// memoria — se pierden cada vez que el proceso se reinicia (deploy, IIS
// reciclando el pool, etc.), lo que cierra la sesión de TODOS los usuarios
// de golpe y también invalida enlaces de recuperación de contraseña que
// estuvieran en tránsito. Se guardan en App_Data/keys, una carpeta que
// IIS/Kestrel nunca sirve como contenido estático (a diferencia de
// wwwroot), así que no queda expuesta al público.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys")))
    .SetApplicationName("ChambeaJobs");

// ---------- Servicios ----------

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection' en appsettings.json.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Reglas de contraseña de seguridad:
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;

        // Bloqueo temporal tras intentos fallidos (mitiga fuerza bruta).
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 5;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessoDenegado";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ---------- Google Sign-In (solo para Candidatos) ----------
// El Client ID/Secret se leen de configuración (appsettings.json en local,
// o mejor aún, "dotnet user-secrets" para no dejarlos en texto plano — este
// proyecto ya tiene un UserSecretsId configurado en el .csproj para eso).
// Si están vacíos, Google simplemente no se agrega como esquema y el botón
// de "Continuar con Google" no debería mostrarse (ver ViewBag en Login/Registro).
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            // Ruta a la que Google redirige después de autenticar (debe
            // coincidir EXACTAMENTE con la registrada en Google Cloud Console).
            options.CallbackPath = "/signin-google";
        });
}

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    options.AddFixedWindowLimiter("autenticacion", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(10);
        limiterOptions.QueueLimit = 0;
    });
});

// ---------- Límite de tamaño de subida (para permitir Video CV hasta 50MB) ----------
// Por defecto, IIS/Kestrel limitan las peticiones a ~30MB; sin esto, subir un
// video CV grabado desde la cámara fallaría con un error genérico del servidor
// antes de llegar a nuestra propia validación de tamaño.
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(opciones =>
{
    opciones.MultipartBodyLengthLimit = 60 * 1024 * 1024; // 60MB (margen sobre el límite de 50MB del video)
});
builder.WebHost.ConfigureKestrel(opciones =>
{
    opciones.Limits.MaxRequestBodySize = 60 * 1024 * 1024;
});

// ---------- Repository Pattern / Service Layer (Módulo Candidato) ----------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<ICandidatoService, CandidatoService>();

// ---------- Service Layer (Módulo Empresa / Vacantes / Paquetes) ----------
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IPaqueteEmpresaService, PaqueteEmpresaService>();
builder.Services.AddScoped<IComprobantePagoService, ComprobantePagoService>();
builder.Services.AddHttpClient<ICaptchaService, ChambeaJobs.Infrastructure.Services.RecaptchaService>();
builder.Services.AddScoped<ICvGeneradorService, CvGeneradorService>();
builder.Services.AddScoped<IEmailSender, ChambeaJobs.Infrastructure.Services.SmtpEmailSender>();
builder.Services.AddScoped<IVacanteService, VacanteService>();
builder.Services.AddScoped<IPasantiaService, PasantiaService>();

// ---------- Service Layer (Módulo Postulaciones / Favoritos) ----------
builder.Services.AddScoped<IPostulacionService, PostulacionService>();
builder.Services.AddScoped<ICompatibilidadService, CompatibilidadService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<IRealTimeNotifier, ChambeaJobs.Web.Hubs.SignalRNotifier>();
builder.Services.AddScoped<IRealTimeChatNotifier, ChambeaJobs.Web.Hubs.SignalRChatNotifier>();
builder.Services.AddScoped<IRealTimeChatBotNotifier, ChambeaJobs.Web.Hubs.SignalRChatBotNotifier>();
builder.Services.AddScoped<IChatService, ChambeaJobs.Infrastructure.Services.ChatService>();
builder.Services.AddHostedService<ChambeaJobs.Web.BackgroundJobs.RecordatorioEntrevistaBackgroundService>();
builder.Services.AddHostedService<ChambeaJobs.Web.BackgroundJobs.RenovacionSuscripcionBackgroundService>();
builder.Services.AddHostedService<ChambeaJobs.Web.BackgroundJobs.CrmAutomatizacionBackgroundService>();
builder.Services.AddHostedService<ChambeaJobs.Web.BackgroundJobs.FinanzasAutomatizacionBackgroundService>();
builder.Services.AddScoped<IFavoritoService, FavoritoService>();

// ---------- Service Layer (Módulo Administrador) ----------
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IAdminUsuarioService, ChambeaJobs.Infrastructure.Services.AdminUsuarioService>();
builder.Services.AddScoped<IAdminEmpresaService, ChambeaJobs.Infrastructure.Services.AdminEmpresaService>();
builder.Services.AddScoped<IAdminVacanteService, AdminVacanteService>();
builder.Services.AddScoped<IAdminCatalogoService, AdminCatalogoService>();
builder.Services.AddScoped<IConfiguracionSistemaService, ConfiguracionSistemaService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<ISoporteService, SoporteService>();
builder.Services.AddScoped<ICrmService, ChambeaJobs.Infrastructure.Services.CrmService>();

// ---------- Módulo Contabilidad y Finanzas ----------
builder.Services.AddScoped<ICategoriaFinancieraService, ChambeaJobs.Infrastructure.Services.Finanzas.CategoriaFinancieraService>();
builder.Services.AddScoped<IPeriodoFinancieroService, ChambeaJobs.Infrastructure.Services.Finanzas.PeriodoFinancieroService>();
builder.Services.AddScoped<IIngresoFinancieroService, ChambeaJobs.Infrastructure.Services.Finanzas.IngresoFinancieroService>();
builder.Services.AddScoped<IGastoFinancieroService, ChambeaJobs.Infrastructure.Services.Finanzas.GastoFinancieroService>();
builder.Services.AddScoped<IFinanzasDashboardService, ChambeaJobs.Infrastructure.Services.Finanzas.FinanzasDashboardService>();
builder.Services.AddScoped<IAuditoriaFinancieraService, ChambeaJobs.Infrastructure.Services.Finanzas.AuditoriaFinancieraService>();

// ---------- Bus de eventos de dominio ----------
// Punto único de desacople: cuando algo importante pasa en el sistema
// (empresa registrada, vacante publicada, pago aprobado, plan vencido...)
// se publica un evento aquí, y cualquier módulo interesado (hoy, el CRM;
// mañana podría ser el chatbot, analítica, o un webhook externo) reacciona
// sin que el módulo que originó la acción tenga que conocerlo.
builder.Services.AddScoped<ChambeaJobs.Application.Eventos.IPublicadorEventos, ChambeaJobs.Infrastructure.Eventos.PublicadorEventos>();

builder.Services.AddScoped<
    ChambeaJobs.Application.Eventos.IManejadorEvento<ChambeaJobs.Application.Eventos.EmpresaRegistradaEvento>,
    ChambeaJobs.Infrastructure.Eventos.ManejadoresCrm.CrmManejadorEmpresaRegistrada>();
builder.Services.AddScoped<
    ChambeaJobs.Application.Eventos.IManejadorEvento<ChambeaJobs.Application.Eventos.PerfilActualizadoEvento>,
    ChambeaJobs.Infrastructure.Eventos.ManejadoresCrm.CrmManejadorPerfilActualizado>();
builder.Services.AddScoped<
    ChambeaJobs.Application.Eventos.IManejadorEvento<ChambeaJobs.Application.Eventos.VacantePublicadaEvento>,
    ChambeaJobs.Infrastructure.Eventos.ManejadoresCrm.CrmManejadorVacantePublicada>();
builder.Services.AddScoped<
    ChambeaJobs.Application.Eventos.IManejadorEvento<ChambeaJobs.Application.Eventos.PagoRealizadoEvento>,
    ChambeaJobs.Infrastructure.Eventos.ManejadoresCrm.CrmManejadorPagoRealizado>();
builder.Services.AddScoped<
    ChambeaJobs.Application.Eventos.IManejadorEvento<ChambeaJobs.Application.Eventos.PagoRealizadoEvento>,
    ChambeaJobs.Infrastructure.Eventos.ManejadoresCrm.FinanzasManejadorPagoAprobado>();
builder.Services.AddScoped<
    ChambeaJobs.Application.Eventos.IManejadorEvento<ChambeaJobs.Application.Eventos.PlanVencidoEvento>,
    ChambeaJobs.Infrastructure.Eventos.ManejadoresCrm.CrmManejadorPlanVencido>();

// ---------- Service Layer (Módulo Evaluaciones Psicométricas) ----------
builder.Services.AddScoped<IEvaluacionService, EvaluacionService>();

var app = builder.Build();

// ---------- Migraciones automáticas + Seed de roles y administrador inicial ----------
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Aplica automáticamente cualquier migración pendiente. Esto evita
        // que el desarrollador tenga que recordar correr Update-Database
        // manualmente cada vez, y es la causa más común de que la app
        // truene al arrancar (las tablas de Identity aún no existen).
        dbContext.Database.Migrate();

        await DbInitializer.SeedRolesYAdminAsync(scope.ServiceProvider);

        logger.LogInformation("Migraciones aplicadas y datos iniciales sembrados correctamente.");
    }
    catch (Exception ex)
    {
        // No relanzamos la excepción: preferimos que el servidor web SÍ
        // levante (para poder ver el error real en el navegador o en
        // /Home/Error500) en vez de que el proceso muera en seco, que es
        // lo que produce el mensaje "No se puede conectar al servidor web".
        logger.LogError(ex, "Error al aplicar migraciones o sembrar datos iniciales. Revisa la cadena de conexión en appsettings.json y que SQL Server esté accesible.");
    }
}

// ---------- Pipeline HTTP ----------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error500");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Home/Error404");

// ---------- Cabeceras de seguridad HTTP ----------
// Cada una mitiga un ataque distinto: X-Frame-Options y frame-ancestors
// evitan que el sitio se cargue dentro de un <iframe> ajeno (clickjacking);
// X-Content-Type-Options evita que el navegador "adivine" el tipo de un
// archivo subido y lo ejecute como algo que no es; Referrer-Policy evita
// filtrar la URL completa (con posibles tokens) a sitios externos; y CSP
// limita de qué dominios puede cargar scripts/estilos/imágenes — reduce
// drásticamente el impacto de un XSS aunque se cuele uno.
//
// La lista de dominios permitidos en CSP es exactamente la que el sitio ya
// usa hoy (Bootstrap/FontAwesome por CDN, Google Fonts, Jitsi Meet,
// Google Sign-In) — si en el futuro se agrega un nuevo script o CDN externo,
// hay que sumarlo aquí también, o el navegador lo bloqueará en silencio.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(self), microphone=(self), geolocation=()";
    headers["Cross-Origin-Opener-Policy"] = "same-origin-allow-popups";
    headers["Cross-Origin-Resource-Policy"] = "same-origin";
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "object-src 'none'; " +
        "form-action 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://accounts.google.com https://meet.jit.si https://www.google.com https://www.gstatic.com; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://fonts.googleapis.com; " +
        "font-src 'self' https://cdnjs.cloudflare.com https://fonts.gstatic.com; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self' wss: ws:; " +
        "frame-src 'self' https://meet.jit.si https://accounts.google.com https://www.google.com;";
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();

// Actualiza "último acceso" del usuario logueado. Se hace aquí (una sola
// vez, por request) en vez de en cada punto de login (normal, Google, 2FA,
// etc.) para no tener que tocar media docena de sitios distintos, y de paso
// sirve también como "última actividad" para el CRM (no solo el momento
// exacto del login). Se limita a una vez por hora por usuario para no
// escribir en la base de datos en cada clic.
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var usuarioId = context.User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(usuarioId))
        {
            var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();
            var ahora = DateTime.UtcNow;

            var usuario = await db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId);
            if (usuario is not null && (usuario.UltimoInicioSesion is null || (ahora - usuario.UltimoInicioSesion.Value).TotalHours >= 1))
            {
                usuario.UltimoInicioSesion = ahora;
                await db.SaveChangesAsync();
            }
        }
    }

    await next();
});

app.UseAuthorization();

// ⚠️ PAUSADO TEMPORALMENTE mientras se hacen pruebas locales — pon esto en
// "true" de nuevo cuando termines de probar, para que el 2FA vuelva a ser
// obligatorio para el Super Administrador.
const bool obligar2FASuperAdmin = false;

// Obliga a cualquier Super Administrador a configurar la verificación en
// dos pasos antes de poder usar el resto del sistema (requisito de
// seguridad de ese rol). No aplica a Administrador normal (ahí sigue siendo
// opcional, como ya estaba). Se deja pasar solo la propia pantalla de
// configuración de 2FA y el logout, para no dejar a la persona sin salida.
app.Use(async (context, next) =>
{
    if (obligar2FASuperAdmin && context.User.IsInRole("SuperAdministrador"))
    {
        var ruta = context.Request.Path.Value ?? "";
        var rutasPermitidas = ruta.StartsWith("/Account/ConfigurarDobleFactor", StringComparison.OrdinalIgnoreCase)
            || ruta.StartsWith("/Account/Logout", StringComparison.OrdinalIgnoreCase)
            || ruta.StartsWith("/css/") || ruta.StartsWith("/js/") || ruta.StartsWith("/images/") || ruta.StartsWith("/lib/");

        if (!rutasPermitidas)
        {
            var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var usuario = await userManager.GetUserAsync(context.User);
            if (usuario is not null && !await userManager.GetTwoFactorEnabledAsync(usuario))
            {
                context.Response.Redirect("/Account/ConfigurarDobleFactor");
                return;
            }
        }
    }

    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChambeaJobs.Web.Hubs.NotificacionesHub>("/hubs/notificaciones");
app.MapHub<ChambeaJobs.Web.Hubs.SoporteChatHub>("/hubs/soporte-chat");
app.MapHub<ChambeaJobs.Web.Hubs.ChatbotHub>("/hubs/chatbot");

app.Run();
