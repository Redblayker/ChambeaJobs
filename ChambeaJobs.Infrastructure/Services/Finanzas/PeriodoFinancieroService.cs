using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Services.Finanzas;

public class PeriodoFinancieroService : IPeriodoFinancieroService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditoriaFinancieraService _auditoria;

    public PeriodoFinancieroService(ApplicationDbContext db, IAuditoriaFinancieraService auditoria)
    {
        _db = db;
        _auditoria = auditoria;
    }

    public async Task<int> ObtenerOCrearPeriodoMensualActualAsync()
    {
        var hoy = DateTime.UtcNow;
        var existente = await _db.PeriodosFinancieros
            .FirstOrDefaultAsync(p => p.Tipo == TipoPeriodoFinanciero.Mensual && p.Anio == hoy.Year && p.Mes == hoy.Month);

        if (existente is not null) return existente.Id;

        var nombresMeses = new[] { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
        var inicio = new DateTime(hoy.Year, hoy.Month, 1);

        var periodo = new PeriodoFinanciero
        {
            Tipo = TipoPeriodoFinanciero.Mensual,
            Anio = hoy.Year,
            Mes = hoy.Month,
            Nombre = $"{nombresMeses[hoy.Month]} {hoy.Year}",
            FechaInicio = inicio,
            FechaFin = inicio.AddMonths(1).AddSeconds(-1)
        };

        // Por si dos procesos intentan crearlo al mismo tiempo (idempotencia
        // real): el índice único (Tipo, Anio, Mes) en la base de datos hace
        // que el segundo intento falle limpio en vez de duplicar.
        try
        {
            _db.PeriodosFinancieros.Add(periodo);
            await _db.SaveChangesAsync();
            return periodo.Id;
        }
        catch (DbUpdateException)
        {
            var yaCreado = await _db.PeriodosFinancieros
                .FirstAsync(p => p.Tipo == TipoPeriodoFinanciero.Mensual && p.Anio == hoy.Year && p.Mes == hoy.Month);
            return yaCreado.Id;
        }
    }

    public async Task<int> ObtenerOCrearPeriodoAnualActualAsync()
    {
        var anio = DateTime.UtcNow.Year;
        var existente = await _db.PeriodosFinancieros
            .FirstOrDefaultAsync(p => p.Tipo == TipoPeriodoFinanciero.Anual && p.Anio == anio);

        if (existente is not null) return existente.Id;

        var periodo = new PeriodoFinanciero
        {
            Tipo = TipoPeriodoFinanciero.Anual,
            Anio = anio,
            Mes = null,
            Nombre = anio.ToString(),
            FechaInicio = new DateTime(anio, 1, 1),
            FechaFin = new DateTime(anio, 12, 31, 23, 59, 59)
        };

        try
        {
            _db.PeriodosFinancieros.Add(periodo);
            await _db.SaveChangesAsync();
            return periodo.Id;
        }
        catch (DbUpdateException)
        {
            var yaCreado = await _db.PeriodosFinancieros.FirstAsync(p => p.Tipo == TipoPeriodoFinanciero.Anual && p.Anio == anio);
            return yaCreado.Id;
        }
    }

    public async Task<List<PeriodoFinancieroDto>> ListarAsync()
    {
        var periodos = await _db.PeriodosFinancieros.OrderByDescending(p => p.FechaInicio).ToListAsync();

        var resultado = new List<PeriodoFinancieroDto>();
        foreach (var p in periodos)
        {
            var ingresos = await _db.IngresosFinancieros
                .Where(i => i.PeriodoFinancieroId == p.Id && i.Estado == EstadoMovimientoFinanciero.Activo)
                .SumAsync(i => (decimal?)i.Monto) ?? 0m;
            var gastos = await _db.GastosFinancieros
                .Where(g => g.PeriodoFinancieroId == p.Id && g.Estado == EstadoMovimientoFinanciero.Activo)
                .SumAsync(g => (decimal?)g.Monto) ?? 0m;

            resultado.Add(new PeriodoFinancieroDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Tipo = p.Tipo.ToString(),
                Cerrado = p.Cerrado,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                TotalIngresos = ingresos,
                TotalGastos = gastos
            });
        }
        return resultado;
    }

    public async Task CerrarPeriodoAsync(int periodoId, string usuarioId, string? ip)
    {
        var periodo = await _db.PeriodosFinancieros.FindAsync(periodoId)
            ?? throw new InvalidOperationException("No se encontró el período.");

        if (periodo.Cerrado) return; // ya estaba cerrado, no hace nada (idempotente)

        periodo.Cerrado = true;
        periodo.FechaCierre = DateTime.UtcNow;
        periodo.CerradoPorUsuarioId = usuarioId;
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(usuarioId, "CierrePeriodo", "Periodos", $"PeriodoFinanciero#{periodoId}",
            "Abierto", "Cerrado", ip);
    }

    public async Task<bool> EstaCerradoAsync(int periodoId)
    {
        var periodo = await _db.PeriodosFinancieros.FindAsync(periodoId);
        return periodo?.Cerrado ?? false;
    }
}
