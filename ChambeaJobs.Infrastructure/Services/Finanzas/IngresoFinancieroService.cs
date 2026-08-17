using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Services.Finanzas;

public class IngresoFinancieroService : IIngresoFinancieroService
{
    private readonly ApplicationDbContext _db;
    private readonly IPeriodoFinancieroService _periodoService;
    private readonly IAuditoriaFinancieraService _auditoria;

    public IngresoFinancieroService(ApplicationDbContext db, IPeriodoFinancieroService periodoService, IAuditoriaFinancieraService auditoria)
    {
        _db = db;
        _periodoService = periodoService;
        _auditoria = auditoria;
    }

    public async Task CrearDesdeIngresoPagoAsync(int pagoId)
    {
        // Idempotencia: si ya existe un ingreso para este pago, no se crea
        // otro — así, si el evento se dispara dos veces por accidente
        // (reintentos, doble clic, etc.), nunca se duplica el ingreso.
        var yaExiste = await _db.IngresosFinancieros.AnyAsync(i => i.PagoId == pagoId);
        if (yaExiste) return;

        var pago = await _db.Pagos
            .Include(p => p.PaqueteEmpresa).ThenInclude(pe => pe!.Empresa)
            .Include(p => p.PaqueteEmpresa).ThenInclude(pe => pe!.PlanSuscripcion)
            .FirstOrDefaultAsync(p => p.Id == pagoId);

        // El pago no existe, o no está aprobado, o le falta la relación al
        // paquete/empresa: no se genera ingreso (regla: solo pagos
        // aprobados y con datos reales generan ingreso — nunca se confía en
        // nada que no venga de la propia base de datos).
        if (pago is null || pago.EstadoPago != EstadosPago.Aprobado || pago.PaqueteEmpresa?.Empresa is null) return;

        var categoriaId = await ObtenerOCrearCategoriaVentaPlanesAsync();
        var periodoId = await _periodoService.ObtenerOCrearPeriodoMensualActualAsync();
        await _periodoService.ObtenerOCrearPeriodoAnualActualAsync();

        var ingreso = new IngresoFinanciero
        {
            PagoId = pago.Id,
            EmpresaId = pago.PaqueteEmpresa.EmpresaId,
            PlanSuscripcionId = pago.PaqueteEmpresa.PlanSuscripcionId,
            CategoriaFinancieraId = categoriaId,
            PeriodoFinancieroId = periodoId,
            Monto = pago.Monto, // el monto real viene de la base de datos, nunca de lo que mande el navegador
            Moneda = MonedaFinanciera.USD,
            MetodoPago = pago.MetodoPago,
            Referencia = pago.ReferenciaTransaccion ?? pago.IdOrdenPayPal,
            Estado = EstadoMovimientoFinanciero.Activo,
            GeneradoPor = "Sistema",
            FechaIngreso = pago.FechaRevision ?? pago.FechaPago,
            FechaCreacion = DateTime.UtcNow
        };

        try
        {
            _db.IngresosFinancieros.Add(ingreso);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // El índice único de PagoId frenó una condición de carrera (dos
            // procesos casi simultáneos) — ya quedó creado por el otro, no
            // pasa nada.
            return;
        }

        await _auditoria.RegistrarAsync("Sistema", "Creacion", "Ingresos", $"IngresoFinanciero#{ingreso.Id}",
            null, $"${ingreso.Monto:0.00} — Pago#{pago.Id} — {pago.PaqueteEmpresa.Empresa.NombreEmpresa}", null);
    }

    public async Task<List<IngresoFinancieroDto>> ListarAsync(FiltroReporteFinancieroDto filtro)
    {
        var query = _db.IngresosFinancieros
            .Include(i => i.Empresa)
            .Include(i => i.PlanSuscripcion)
            .Include(i => i.CategoriaFinanciera)
            .Include(i => i.PeriodoFinanciero)
            .AsQueryable();

        if (filtro.Desde.HasValue) query = query.Where(i => i.FechaIngreso >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) query = query.Where(i => i.FechaIngreso <= filtro.Hasta.Value);
        if (filtro.EmpresaId.HasValue) query = query.Where(i => i.EmpresaId == filtro.EmpresaId.Value);
        if (filtro.CategoriaFinancieraId.HasValue) query = query.Where(i => i.CategoriaFinancieraId == filtro.CategoriaFinancieraId.Value);
        if (filtro.PlanSuscripcionId.HasValue) query = query.Where(i => i.PlanSuscripcionId == filtro.PlanSuscripcionId.Value);
        if (filtro.Moneda.HasValue) query = query.Where(i => i.Moneda == filtro.Moneda.Value);
        if (!string.IsNullOrWhiteSpace(filtro.MetodoPago)) query = query.Where(i => i.MetodoPago == filtro.MetodoPago);
        if (!string.IsNullOrWhiteSpace(filtro.Estado) && Enum.TryParse<EstadoMovimientoFinanciero>(filtro.Estado, out var estado))
            query = query.Where(i => i.Estado == estado);

        return await query
            .OrderByDescending(i => i.FechaIngreso)
            .Select(i => new IngresoFinancieroDto
            {
                Id = i.Id,
                NombreEmpresa = i.Empresa!.NombreEmpresa,
                NombrePlan = i.PlanSuscripcion != null ? i.PlanSuscripcion.Nombre : null,
                CategoriaFinanciera = i.CategoriaFinanciera!.Nombre,
                Monto = i.Monto,
                Moneda = i.Moneda.ToString(),
                MetodoPago = i.MetodoPago,
                Referencia = i.Referencia,
                Estado = i.Estado.ToString(),
                FechaIngreso = i.FechaIngreso,
                PeriodoNombre = i.PeriodoFinanciero!.Nombre,
                MotivoAnulacion = i.MotivoAnulacion
            })
            .ToListAsync();
    }

    public async Task AnularAsync(int ingresoId, string motivo, string usuarioId, string? ip)
    {
        var ingreso = await _db.IngresosFinancieros.Include(i => i.PeriodoFinanciero).FirstOrDefaultAsync(i => i.Id == ingresoId)
            ?? throw new InvalidOperationException("No se encontró el ingreso.");

        if (ingreso.Estado == EstadoMovimientoFinanciero.Anulado) return; // ya estaba anulado, idempotente

        if (ingreso.PeriodoFinanciero?.Cerrado == true)
        {
            throw new InvalidOperationException(
                "Este ingreso pertenece a un período ya cerrado. Cualquier corrección debe hacerse mediante un ajuste autorizado, no una anulación directa.");
        }

        var estadoAnterior = ingreso.Estado.ToString();
        ingreso.Estado = EstadoMovimientoFinanciero.Anulado;
        ingreso.MotivoAnulacion = motivo;
        ingreso.FechaAnulacion = DateTime.UtcNow;
        ingreso.AnuladoPorUsuarioId = usuarioId;
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(usuarioId, "Anulacion", "Ingresos", $"IngresoFinanciero#{ingresoId}",
            estadoAnterior, $"Anulado: {motivo}", ip);
    }

    /// <summary>La primera vez que se necesita, crea la categoría "Venta de planes de suscripción" — así el catálogo no queda vacío desde el día uno, pero sigue siendo 100% editable/administrable después.</summary>
    private async Task<int> ObtenerOCrearCategoriaVentaPlanesAsync()
    {
        const string nombre = "Venta de planes de suscripción";
        var existente = await _db.CategoriasFinancieras.FirstOrDefaultAsync(c => c.Tipo == TipoCategoriaFinanciera.Ingreso && c.Nombre == nombre);
        if (existente is not null) return existente.Id;

        var categoria = new CategoriaFinanciera
        {
            Tipo = TipoCategoriaFinanciera.Ingreso,
            Nombre = nombre,
            Descripcion = "Ingresos generados automáticamente por pagos aprobados de planes Básico/Empresarial.",
            Activa = true
        };
        _db.CategoriasFinancieras.Add(categoria);
        await _db.SaveChangesAsync();
        return categoria.Id;
    }
}
