using Microsoft.AspNetCore.Identity;

namespace ChambeaJobs.Infrastructure.Identity;

/// <summary>
/// Extensión de IdentityUser con las columnas custom de la tabla
/// AspNetUsers: EstadoCuenta y FechaRegistro. Todos los usuarios del
/// sistema (Candidato, Empresa, Administrador) son instancias de esta
/// misma clase, diferenciados por su rol en AspNetUserRoles.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>true = cuenta activa, false = suspendida por un administrador.</summary>
    public bool EstadoCuenta { get; set; } = true;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    /// <summary>Se actualiza cada vez que el usuario inicia sesión (login normal o con Google). Usado por el CRM para detectar cuentas inactivas.</summary>
    public DateTime? UltimoInicioSesion { get; set; }
}
