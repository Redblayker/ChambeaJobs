using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChambeaJobs.Infrastructure.Data;

/// <summary>
/// Siembra los datos mínimos necesarios para que el sistema funcione desde
/// el primer arranque: los roles fijos (Candidato, Empresa, Administrador,
/// SuperAdministrador) y la cuenta de Super Administrador inicial.
///
/// IMPORTANTE (seguridad): la contraseña del Super Administrador NUNCA vive
/// en el código, en appsettings.json ni en ningún script — se lee de las
/// variables de entorno SUPERADMIN_EMAIL / SUPERADMIN_PASSWORD, que se
/// configuran directamente en el servidor (panel de MonsterASP, o
/// "Variables de entorno" del hosting). Si esas variables no están
/// configuradas, simplemente no se crea ninguna cuenta nueva — el sistema
/// arranca igual, sin tronar.
///
/// Se ejecuta en cada arranque, y es seguro llamarlo repetidas veces porque
/// verifica existencia antes de crear cualquier registro (idempotente).
/// </summary>
public static class DbInitializer
{
    public static async Task SeedRolesYAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var entorno = serviceProvider.GetRequiredService<IHostEnvironment>();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        foreach (var nombreRol in RolesSistema.Todos)
        {
            if (!await roleManager.RoleExistsAsync(nombreRol))
            {
                var resultado = await roleManager.CreateAsync(new IdentityRole(nombreRol));
                if (!resultado.Succeeded)
                {
                    var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"No se pudo crear el rol '{nombreRol}': {errores}");
                }
            }
        }

        await SembrarSuperAdministradorAsync(userManager, logger);
        await SembrarAdminDeDesarrolloAsync(userManager, entorno, logger);
    }

    /// <summary>Crea el Super Administrador SOLO si SUPERADMIN_EMAIL y
    /// SUPERADMIN_PASSWORD están configuradas como variables de entorno.
    /// La contraseña nunca se escribe en ningún log.</summary>
    private static async Task SembrarSuperAdministradorAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var correo = Environment.GetEnvironmentVariable("SUPERADMIN_EMAIL");
        var contrasena = Environment.GetEnvironmentVariable("SUPERADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
        {
            logger.LogInformation(
                "SUPERADMIN_EMAIL / SUPERADMIN_PASSWORD no están configuradas — no se creó ningún Super Administrador nuevo. " +
                "Configúralas como variables de entorno del servidor para crear esa cuenta.");
            return;
        }

        var existente = await userManager.FindByEmailAsync(correo);
        if (existente is not null)
        {
            // Ya existe (de un arranque anterior) — no se toca la contraseña
            // aquí para no pisar un cambio de contraseña que ya haya hecho
            // el propio Super Administrador desde la app.
            if (!await userManager.IsInRoleAsync(existente, RolesSistema.SuperAdministrador))
            {
                await userManager.AddToRoleAsync(existente, RolesSistema.SuperAdministrador);
            }
            return;
        }

        var superAdmin = new ApplicationUser
        {
            UserName = correo,
            Email = correo,
            EmailConfirmed = true,
            EstadoCuenta = true,
            FechaRegistro = DateTime.UtcNow
        };

        var resultadoCreacion = await userManager.CreateAsync(superAdmin, contrasena);

        if (!resultadoCreacion.Succeeded)
        {
            // Deliberadamente no se incluye la contraseña ni ningún dato
            // sensible en este mensaje de error.
            var errores = string.Join(", ", resultadoCreacion.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"No se pudo crear el Super Administrador inicial: {errores}");
        }

        await userManager.AddToRoleAsync(superAdmin, RolesSistema.SuperAdministrador);

        logger.LogInformation(
            "Super Administrador creado correctamente ({Correo}). Por seguridad, cambia la contraseña inicial " +
            "desde 'Mi cuenta' en tu primer inicio de sesión.", correo);
    }

    /// <summary>Solo para entornos de Desarrollo local: si no hay ningún
    /// Super Administrador configurado por variables de entorno, siembra una
    /// cuenta de Administrador de prueba con contraseña fija — para que un
    /// desarrollador pueda levantar el proyecto localmente sin tener que
    /// configurar variables de entorno primero. Esto NUNCA corre en
    /// Production (ver el chequeo de IHostEnvironment abajo), así que esa
    /// contraseña fija jamás termina en un servidor real.</summary>
    private static async Task SembrarAdminDeDesarrolloAsync(UserManager<ApplicationUser> userManager, IHostEnvironment entorno, ILogger logger)
    {
        if (!entorno.IsDevelopment()) return;

        const string correoAdminDev = "admin@chambeajobs.com";
        var existente = await userManager.FindByEmailAsync(correoAdminDev);
        if (existente is not null) return;

        var adminDev = new ApplicationUser
        {
            UserName = correoAdminDev,
            Email = correoAdminDev,
            EmailConfirmed = true,
            EstadoCuenta = true,
            FechaRegistro = DateTime.UtcNow
        };

        var resultado = await userManager.CreateAsync(adminDev, "ChambeaJobs#2026");
        if (resultado.Succeeded)
        {
            await userManager.AddToRoleAsync(adminDev, RolesSistema.Administrador);
            logger.LogInformation("Cuenta de Administrador de desarrollo sembrada (solo entorno Development).");
        }
    }
}
