using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Application.Interfaces;

public interface ICategoriaFinancieraService
{
    Task<List<CategoriaFinancieraDto>> ListarAsync(TipoCategoriaFinanciera? tipo = null);
    Task<int> CrearAsync(CategoriaFinancieraFormDto dto);
    Task ActualizarAsync(int id, CategoriaFinancieraFormDto dto);
    Task DesactivarAsync(int id);
}

public interface IIngresoFinancieroService
{
    /// <summary>Crea el ingreso a partir de un pago aprobado. Idempotente:
    /// si ya existe un ingreso para ese PagoId, no crea uno nuevo (evita
    /// duplicados aunque el evento se dispare dos veces).</summary>
    Task CrearDesdeIngresoPagoAsync(int pagoId);

    Task<List<IngresoFinancieroDto>> ListarAsync(FiltroReporteFinancieroDto filtro);
    Task AnularAsync(int ingresoId, string motivo, string usuarioId, string? ip);
}

public interface IGastoFinancieroService
{
    Task<int> RegistrarAsync(GastoFinancieroFormDto dto, string usuarioId, Microsoft.AspNetCore.Http.IFormFile? comprobante, string? ip);
    Task<List<GastoFinancieroDto>> ListarAsync(FiltroReporteFinancieroDto filtro);
    Task AnularAsync(int gastoId, string motivo, string usuarioId, string? ip);
}

public interface IPeriodoFinancieroService
{
    /// <summary>Devuelve el período mensual actual, creándolo si todavía no existe (idempotente).</summary>
    Task<int> ObtenerOCrearPeriodoMensualActualAsync();
    Task<int> ObtenerOCrearPeriodoAnualActualAsync();
    Task<List<PeriodoFinancieroDto>> ListarAsync();
    Task CerrarPeriodoAsync(int periodoId, string usuarioId, string? ip);
    Task<bool> EstaCerradoAsync(int periodoId);
}

public interface IFinanzasDashboardService
{
    Task<FinanzasDashboardDto> ObtenerDashboardAsync(DateTime? desde = null, DateTime? hasta = null);
    Task<EstadoResultadosDto> ObtenerEstadoResultadosAsync(DateTime desde, DateTime hasta);
    Task<BalanceGeneralDto> ObtenerBalanceGeneralAsync();
    Task<byte[]> GenerarReportePdfAsync(FinanzasExportOpcionesDto opciones);
}

public interface IAuditoriaFinancieraService
{
    Task RegistrarAsync(string usuarioId, string accion, string modulo, string registroAfectado,
        string? valorAnterior, string? valorNuevo, string? ip, string resultado = "Exito");
    Task<List<AuditoriaFinancieraDto>> ObtenerRecientesAsync(int cantidad = 200);
}
