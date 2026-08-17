namespace ChambeaJobs.Domain.Enums;

/// <summary>
/// Roles fijos del sistema ChambeaJobs. Se usan tanto para sembrar
/// AspNetRoles como para las validaciones
/// [Authorize(Roles = "..")] en los controladores.
/// </summary>
public static class RolesSistema
{
    public const string Candidato = "Candidato";
    public const string Empresa = "Empresa";
    public const string Administrador = "Administrador";

    /// <summary>Todo lo que puede hacer Administrador, más Contabilidad y
    /// Finanzas, CRM completo, Auditoría, Configuración y administración de
    /// usuarios/permisos. Se crea únicamente por variables de entorno (ver
    /// DbInitializer) — nunca queda una contraseña de este rol en el código.</summary>
    public const string SuperAdministrador = "SuperAdministrador";

    /// <summary>Roles del módulo Contabilidad y Finanzas. AdministradorFinanciero
    /// puede registrar/editar; SupervisorFinanciero y AuditorFinanciero son
    /// de solo lectura (ver políticas en Program.cs).</summary>
    public const string AdministradorFinanciero = "AdministradorFinanciero";
    public const string SupervisorFinanciero = "SupervisorFinanciero";
    public const string AuditorFinanciero = "AuditorFinanciero";

    public const string CualquierAdmin = Administrador + "," + SuperAdministrador;

    /// <summary>Quién puede VER el módulo de Finanzas.</summary>
    public const string FinanzasVer = SuperAdministrador + "," + AdministradorFinanciero + "," + SupervisorFinanciero + "," + AuditorFinanciero + "," + Administrador;

    /// <summary>Quién puede REGISTRAR/EDITAR en el módulo de Finanzas (no solo ver).</summary>
    public const string FinanzasEditar = SuperAdministrador + "," + AdministradorFinanciero;

    public static readonly string[] Todos =
    {
        Candidato,
        Empresa,
        Administrador,
        SuperAdministrador,
        AdministradorFinanciero,
        SupervisorFinanciero,
        AuditorFinanciero
    };
}
