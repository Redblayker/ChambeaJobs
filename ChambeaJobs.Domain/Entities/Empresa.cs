namespace ChambeaJobs.Domain.Entities;

/// <summary>Perfil de un usuario con rol Empresa.</summary>
public class Empresa : BaseEntity
{
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreEmpresa { get; set; } = string.Empty;
    public string RUC { get; set; } = string.Empty;
    public int? LogoArchivoId { get; set; }
    public Archivo? LogoArchivo { get; set; }
    public string? Descripcion { get; set; }
    public string? SitioWeb { get; set; }
    public int UbicacionId { get; set; }
    public Ubicacion? Ubicacion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // ---------- Perfil enriquecido de empresa ----------
    public string? Historia { get; set; }
    public string? Mision { get; set; }
    public string? Vision { get; set; }
    public string? CulturaOrganizacional { get; set; }

    /// <summary>Beneficios ofrecidos, uno por línea (se renderiza como lista con viñetas).</summary>
    public string? Beneficios { get; set; }

    /// <summary>Rango de colaboradores, ej. "1-10", "11-50", "51-200", "201-500", "500+".</summary>
    public string? NumeroColaboradores { get; set; }

    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TiktokUrl { get; set; }

    /// <summary>Rubro/industria de la empresa, ej. "Comercio", "Tecnología". Alimenta el CRM automáticamente.</summary>
    public string? SectorEmpresarial { get; set; }

    /// <summary>Nombre de la persona de contacto para temas comerciales (no necesariamente quien inició sesión).</summary>
    public string? NombreContacto { get; set; }

    public string? TelefonoContacto { get; set; }

    public ICollection<Vacante> Vacantes { get; set; } = new List<Vacante>();
    public ICollection<PaqueteEmpresa> Paquetes { get; set; } = new List<PaqueteEmpresa>();
    public ICollection<EmpresaGaleria> Galeria { get; set; } = new List<EmpresaGaleria>();

    /// <summary>
    /// El paquete vigente actual (si existe). Se usa en el Dashboard Empresa
    /// y para validar si puede publicar.
    /// </summary>
    public PaqueteEmpresa? ObtenerPaqueteVigente() =>
        Paquetes
            .Where(p => p.Estado == EstadosPaquete.Vigente)
            .OrderByDescending(p => p.FechaCompra)
            .FirstOrDefault();
}

/// <summary>Estados posibles de un PaqueteEmpresa.</summary>
public static class EstadosPaquete
{
    public const string Pendiente = "Pendiente"; // solicitud de pago registrada, aún no aprobada por admin
    public const string Vigente = "Vigente";
    public const string Agotado = "Agotado";
    public const string Vencido = "Vencido";
    public const string Rechazado = "Rechazado"; // el admin rechazó el pago
}
