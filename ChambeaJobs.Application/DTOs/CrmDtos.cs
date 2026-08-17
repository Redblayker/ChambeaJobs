using System.ComponentModel.DataAnnotations;
using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Application.DTOs;

public class CrmEmpresaListItemDto
{
    public int Id { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public EtapaPipelineCrm Etapa { get; set; }
    public string? ContactoPrincipal { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public bool EstaRegistradaEnPlataforma { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaUltimaActividad { get; set; }
}

public class CrmEmpresaDetalleDto
{
    public int Id { get; set; }
    public int? EmpresaId { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public EtapaPipelineCrm Etapa { get; set; }
    public string? ContactoPrincipal { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? RUC { get; set; }
    public string? Direccion { get; set; }
    public string? SectorEmpresarial { get; set; }
    public string? TamanoEmpresa { get; set; }
    public string? SitioWeb { get; set; }
    public string? RedesSociales { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<CrmActividadDto> Actividades { get; set; } = new();
    public List<CrmArchivoAdjuntoDto> ArchivosAdjuntos { get; set; } = new();

    // Datos comerciales de la plataforma, si la empresa ya está registrada.
    public string? PlanActual { get; set; }
    public DateTime? PlanFechaVencimiento { get; set; }
    public string? PlanEstado { get; set; }
}

public class CrmEmpresaFormDto
{
    [Required(ErrorMessage = "El nombre de la empresa es obligatorio")]
    [StringLength(150)]
    public string NombreEmpresa { get; set; } = string.Empty;

    public int? EmpresaId { get; set; }
    public EtapaPipelineCrm Etapa { get; set; } = EtapaPipelineCrm.Prospecto;

    [StringLength(150)]
    public string? ContactoPrincipal { get; set; }
    [StringLength(30)]
    public string? Telefono { get; set; }
    [StringLength(200)]
    [EmailAddress(ErrorMessage = "Correo inválido")]
    public string? Correo { get; set; }
    [StringLength(30)]
    public string? RUC { get; set; }
    [StringLength(300)]
    public string? Direccion { get; set; }
    [StringLength(100)]
    public string? SectorEmpresarial { get; set; }
    [StringLength(30)]
    public string? TamanoEmpresa { get; set; }
    [StringLength(200)]
    public string? SitioWeb { get; set; }
    [StringLength(500)]
    public string? RedesSociales { get; set; }
    [StringLength(2000)]
    public string? Observaciones { get; set; }
}

public class CrmActividadDto
{
    public int Id { get; set; }
    public TipoActividadCrm Tipo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaActividad { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
}

public class CrmActividadFormDto
{
    [Required]
    public TipoActividadCrm Tipo { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(2000)]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha es obligatoria")]
    public DateTime FechaActividad { get; set; } = DateTime.Now;
}

public class CrmPipelineColumnaDto
{
    public EtapaPipelineCrm Etapa { get; set; }
    public string EtiquetaEtapa { get; set; } = string.Empty;
    public List<CrmEmpresaListItemDto> Empresas { get; set; } = new();
}

public class CrmMoverEtapaDto
{
    public int CrmEmpresaId { get; set; }
    public EtapaPipelineCrm NuevaEtapa { get; set; }
}

public class CrmReporteDto
{
    public int TotalEmpresas { get; set; }
    public int TotalClientesActivos { get; set; }
    public int TotalClientesInactivos { get; set; }
    public int TotalProspectos { get; set; }
    public decimal IngresosMesActual { get; set; }
    public decimal IngresosAnioActual { get; set; }
    public int VacantesPublicadasTotal { get; set; }
    public int CandidatosRegistrados { get; set; }
    public int PostulacionesTotal { get; set; }
    public double TasaConversionProspectoACliente { get; set; }
    public List<CrmReporteConteoDto> PorEtapa { get; set; } = new();
    public List<CrmReporteConteoDto> PorSector { get; set; } = new();
    public List<CrmReporteConteoDto> PlanesMasVendidos { get; set; } = new();
    public List<CrmAlertaDto> Alertas { get; set; } = new();
    public CrmResumenIngresosDto ResumenIngresos { get; set; } = new();
}

public class CrmReporteConteoDto
{
    public string Etiqueta { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class CrmArchivoAdjuntoDto
{
    public int Id { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaSubida { get; set; }
}

public class CrmAgendaItemDto
{
    public int CrmEmpresaId { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public int ActividadId { get; set; }
    public TipoActividadCrm Tipo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaActividad { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
}

/// <summary>Estadísticas calculadas en vivo (no se guardan como columnas
/// aparte, para que nunca queden desactualizadas): se recalculan cada vez
/// que se pide el detalle de la empresa en el CRM.</summary>
public class CrmEstadisticasEmpresaDto
{
    public int VacantesActivas { get; set; }
    public int VacantesCerradas { get; set; }
    public DateTime? UltimaVacantePublicada { get; set; }
    public int PostulacionesRecibidas { get; set; }
    public int CandidatosContratados { get; set; }
    public decimal TotalInvertido { get; set; }
    public DateTime? UltimaCompra { get; set; }
    public DateTime? UltimoInicioSesion { get; set; }
}

public class CrmAlertaDto
{
    public int CrmEmpresaId { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Severidad { get; set; } = "info"; // info | warning | danger
}

public class CrmReporteExportOpcionesDto
{
    public bool IncluirRendimiento { get; set; } = true;
    public bool IncluirIngresos { get; set; } = true;
    public bool IncluirSectores { get; set; } = true;
    public bool IncluirAlertas { get; set; } = true;
    public bool IncluirListaEmpresas { get; set; } = true;
}

/// <summary>Un pago aprobado real, con la empresa y el plan que lo generó —
/// el respaldo que se necesita para justificar ingresos ante la DGI.</summary>
public class CrmDetallePagoDto
{
    public string NombreEmpresa { get; set; } = string.Empty;
    public string? RUC { get; set; }
    public string NombrePlan { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
}

public class CrmResumenIngresosDto
{
    public decimal TotalMes { get; set; }
    public decimal TotalAnio { get; set; }
    public List<CrmDetallePagoDto> PagosDelMes { get; set; } = new();
    public List<CrmDetallePagoDto> PagosDelAnio { get; set; } = new();

    /// <summary>Cuántas empresas pagaron cada plan este mes, y cuánto sumó
    /// cada uno — para la frase-resumen tipo "2 empresas pagaron el plan
    /// Empresarial ($50 c/u) y 1 pagó el plan Básico".</summary>
    public List<CrmResumenPorPlanDto> ResumenPorPlanMes { get; set; } = new();
}

public class CrmResumenPorPlanDto
{
    public string NombrePlan { get; set; } = string.Empty;
    public int CantidadEmpresas { get; set; }
    public decimal MontoPorPago { get; set; }
    public decimal TotalPlan { get; set; }
}
