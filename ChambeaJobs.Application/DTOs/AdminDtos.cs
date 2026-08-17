namespace ChambeaJobs.Application.DTOs;

// ---------- Gestión de Usuarios ----------
public class UsuarioAdminDto
{
    public string Id { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool EstadoCuenta { get; set; }
    public DateTime FechaRegistro { get; set; }
}

// ---------- Gestión de Empresas ----------
public class EmpresaAdminDto
{
    public int Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreEmpresa { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public bool EstadoCuenta { get; set; }
    public string EstadoPaquete { get; set; } = string.Empty;
    public int VacantesActivas { get; set; }
}

// ---------- Gestión de Vacantes ----------
public class VacanteAdminDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string EmpresaNombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; }
}

// ---------- Categorías y Ubicaciones (CRUD) ----------
public class CategoriaFormDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int VacantesAsociadas { get; set; }
}

public class UbicacionFormDto
{
    public int Id { get; set; }
    public string Departamento { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public int VacantesAsociadas { get; set; }
}

// ---------- Configuración del sistema ----------
public class ConfiguracionPaqueteDto
{
    public decimal Precio { get; set; }
    public int CantidadVacantes { get; set; }
    public int DiasVigencia { get; set; }
}

// ---------- Reportes / Dashboard ----------
public class ReporteDashboardDto
{
    public int TotalCandidatos { get; set; }
    public int TotalEmpresas { get; set; }
    public int VacantesActivas { get; set; }
    public decimal IngresosTotales { get; set; }
    public decimal IngresosSemana { get; set; }
    public decimal IngresosQuincena { get; set; }
    public decimal IngresosMes { get; set; }
    public List<ReporteBarraDto> PostulacionesPorMes { get; set; } = new();
    public List<ReporteBarraDto> VacantesPorCategoria { get; set; } = new();
}

public class ReporteBarraDto
{
    public string Etiqueta { get; set; } = string.Empty;
    public int Valor { get; set; }
}

// ---------- Auditoría ----------
public class AuditoriaDto
{
    public int Id { get; set; }
    public string UsuarioCorreo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string EntidadAfectada { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
}

// ---------- Soporte ----------
public class TicketSoporteDto
{
    public int Id { get; set; }
    public string NombreContacto { get; set; } = string.Empty;
    public string CorreoContacto { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? RespuestaAdmin { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaRespuesta { get; set; }
    public List<MensajeSoporteDto> Mensajes { get; set; } = new();
}

/// <summary>Un mensaje individual del chat de soporte.</summary>
public class MensajeSoporteDto
{
    public int Id { get; set; }
    public string AutorNombre { get; set; } = string.Empty;
    public bool EsAdmin { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaEnvio { get; set; }
}

public class CrearTicketSoporteDto
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Escribe tu nombre")]
    [System.ComponentModel.DataAnnotations.StringLength(150)]
    public string NombreContacto { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Escribe tu correo")]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string CorreoContacto { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El asunto es obligatorio")]
    [System.ComponentModel.DataAnnotations.StringLength(200)]
    public string Asunto { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Cuéntanos tu problema o consulta")]
    [System.ComponentModel.DataAnnotations.StringLength(2000)]
    public string Mensaje { get; set; } = string.Empty;
}
