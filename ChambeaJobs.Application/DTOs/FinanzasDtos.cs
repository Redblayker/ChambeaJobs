using System.ComponentModel.DataAnnotations;
using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Application.DTOs;

// ---------- Categorías ----------
public class CategoriaFinancieraDto
{
    public int Id { get; set; }
    public TipoCategoriaFinanciera Tipo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activa { get; set; }
}

public class CategoriaFinancieraFormDto
{
    [Required] public TipoCategoriaFinanciera Tipo { get; set; }
    [Required, StringLength(150)] public string Nombre { get; set; } = string.Empty;
    [StringLength(400)] public string? Descripcion { get; set; }
}

// ---------- Ingresos ----------
public class IngresoFinancieroDto
{
    public int Id { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string? NombrePlan { get; set; }
    public string CategoriaFinanciera { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public string PeriodoNombre { get; set; } = string.Empty;
    public string? MotivoAnulacion { get; set; }
}

public class AnularMovimientoDto
{
    [Required(ErrorMessage = "El motivo es obligatorio")]
    [StringLength(500)]
    public string Motivo { get; set; } = string.Empty;
}

// ---------- Gastos ----------
public class GastoFinancieroDto
{
    public int Id { get; set; }
    public string CategoriaFinanciera { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? Proveedor { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public string? NumeroReferencia { get; set; }
    public string? RutaComprobante { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string PeriodoNombre { get; set; } = string.Empty;
    public string RegistradoPor { get; set; } = string.Empty;
    public string? MotivoAnulacion { get; set; }
}

public class GastoFinancieroFormDto
{
    [Required] public int CategoriaFinancieraId { get; set; }
    [Required, StringLength(300)] public string Descripcion { get; set; } = string.Empty;
    [StringLength(200)] public string? Proveedor { get; set; }
    [Required] public DateTime Fecha { get; set; } = DateTime.Today;
    [Required, Range(0.01, 9999999)] public decimal Monto { get; set; }
    [Required] public MonedaFinanciera Moneda { get; set; } = MonedaFinanciera.USD;
    [Required, StringLength(80)] public string MetodoPago { get; set; } = string.Empty;
    [StringLength(150)] public string? NumeroReferencia { get; set; }
}

// ---------- Períodos ----------
public class PeriodoFinancieroDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool Cerrado { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalGastos { get; set; }
}

// ---------- Dashboard ----------
public class FinanzasDashboardDto
{
    public decimal IngresosDia { get; set; }
    public decimal IngresosMes { get; set; }
    public decimal IngresosAnio { get; set; }
    public decimal GastosMes { get; set; }
    public decimal GastosAnio { get; set; }
    public decimal UtilidadEstimadaMes { get; set; }
    public decimal UtilidadEstimadaAnio { get; set; }
    public int PagosAprobados { get; set; }
    public int PagosPendientes { get; set; }
    public int PagosRechazados { get; set; }
    public decimal CuentasPorCobrarEstimadas { get; set; } // pagos pendientes en monto
    public int EmpresasClientes { get; set; }
    public int RenovacionesDelMes { get; set; }
    public List<CrmReporteConteoDto> IngresosPorPlan { get; set; } = new();
}

// ---------- Estado de resultados ----------
public class EstadoResultadosDto
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal ResultadoDelPeriodo => TotalIngresos - TotalGastos;
    public List<FinanzasCategoriaMontoDto> IngresosPorCategoria { get; set; } = new();
    public List<FinanzasCategoriaMontoDto> GastosPorCategoria { get; set; } = new();
}

/// <summary>A diferencia de CrmReporteConteoDto (que cuenta registros como
/// entero), este DTO guarda un MONTO en dinero con precisión decimal
/// completa — nunca se deben perder centavos en un reporte financiero.</summary>
public class FinanzasCategoriaMontoDto
{
    public string Etiqueta { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}

public class FinanzasExportOpcionesDto
{
    public bool IncluirIndicadores { get; set; } = true;
    public bool IncluirIngresos { get; set; } = true;
    public bool IncluirGastos { get; set; } = true;
    public bool IncluirEstadoResultados { get; set; } = true;
    public bool IncluirBalanceGeneral { get; set; } = true;
    public bool IncluirAuditoria { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}

// ---------- Balance general (estructura preparada) ----------
public class BalanceGeneralDto
{
    public decimal Efectivo { get; set; }
    public decimal CuentasPorCobrar { get; set; }
    public decimal TotalActivos => Efectivo + CuentasPorCobrar;

    public decimal CuentasPorPagar { get; set; }
    public decimal TotalPasivos => CuentasPorPagar;

    public decimal ResultadosAcumulados { get; set; }
    public decimal TotalPatrimonio => ResultadosAcumulados;
}

// ---------- Filtros comunes de reportes ----------
public class FiltroReporteFinancieroDto
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? EmpresaId { get; set; }
    public int? CategoriaFinancieraId { get; set; }
    public string? Estado { get; set; }
    public string? MetodoPago { get; set; }
    public int? PlanSuscripcionId { get; set; }
    public MonedaFinanciera? Moneda { get; set; }
}

public class AuditoriaFinancieraDto
{
    public string UsuarioNombre { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string RegistroAfectado { get; set; } = string.Empty;
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public string Resultado { get; set; } = string.Empty;
}
