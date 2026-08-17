using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Services.Finanzas;

public class GastoFinancieroService : IGastoFinancieroService
{
    private static readonly string[] ExtensionesPermitidas = { ".pdf", ".jpg", ".jpeg", ".png" };
    private const int TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly IPeriodoFinancieroService _periodoService;
    private readonly IAuditoriaFinancieraService _auditoria;

    public GastoFinancieroService(ApplicationDbContext db, IFileStorageService fileStorage, IPeriodoFinancieroService periodoService, IAuditoriaFinancieraService auditoria)
    {
        _db = db;
        _fileStorage = fileStorage;
        _periodoService = periodoService;
        _auditoria = auditoria;
    }

    public async Task<int> RegistrarAsync(GastoFinancieroFormDto dto, string usuarioId, IFormFile? comprobante, string? ip)
    {
        var categoria = await _db.CategoriasFinancieras.FindAsync(dto.CategoriaFinancieraId)
            ?? throw new InvalidOperationException("La categoría seleccionada no existe.");
        if (categoria.Tipo != TipoCategoriaFinanciera.Gasto)
        {
            throw new InvalidOperationException("Esa categoría no es de tipo Gasto.");
        }

        int? archivoComprobanteId = null;
        if (comprobante is not null)
        {
            archivoComprobanteId = await GuardarComprobanteValidadoAsync(comprobante, usuarioId);
        }

        var periodoId = await _periodoService.ObtenerOCrearPeriodoMensualActualAsync();

        var gasto = new GastoFinanciero
        {
            CategoriaFinancieraId = dto.CategoriaFinancieraId,
            Descripcion = dto.Descripcion.Trim(),
            Proveedor = dto.Proveedor,
            Fecha = dto.Fecha,
            Monto = dto.Monto,
            Moneda = dto.Moneda,
            MetodoPago = dto.MetodoPago,
            NumeroReferencia = dto.NumeroReferencia,
            ArchivoComprobanteId = archivoComprobanteId,
            Estado = EstadoMovimientoFinanciero.Activo,
            PeriodoFinancieroId = periodoId,
            RegistradoPorUsuarioId = usuarioId,
            FechaCreacion = DateTime.UtcNow
        };

        _db.GastosFinancieros.Add(gasto);
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(usuarioId, "Creacion", "Gastos", $"GastoFinanciero#{gasto.Id}",
            null, $"${gasto.Monto:0.00} — {gasto.Descripcion}", ip);

        return gasto.Id;
    }

    public async Task<List<GastoFinancieroDto>> ListarAsync(FiltroReporteFinancieroDto filtro)
    {
        var query = _db.GastosFinancieros
            .Include(g => g.CategoriaFinanciera)
            .Include(g => g.PeriodoFinanciero)
            .Include(g => g.ArchivoComprobante)
            .AsQueryable();

        if (filtro.Desde.HasValue) query = query.Where(g => g.Fecha >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) query = query.Where(g => g.Fecha <= filtro.Hasta.Value);
        if (filtro.CategoriaFinancieraId.HasValue) query = query.Where(g => g.CategoriaFinancieraId == filtro.CategoriaFinancieraId.Value);
        if (filtro.Moneda.HasValue) query = query.Where(g => g.Moneda == filtro.Moneda.Value);
        if (!string.IsNullOrWhiteSpace(filtro.MetodoPago)) query = query.Where(g => g.MetodoPago == filtro.MetodoPago);
        if (!string.IsNullOrWhiteSpace(filtro.Estado) && Enum.TryParse<EstadoMovimientoFinanciero>(filtro.Estado, out var estado))
            query = query.Where(g => g.Estado == estado);

        var registradoresIds = await query.Select(g => g.RegistradoPorUsuarioId).Distinct().ToListAsync();
        var usuarios = await _db.Users.Where(u => registradoresIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Email ?? u.Id);

        var gastos = await query.OrderByDescending(g => g.Fecha).ToListAsync();

        return gastos.Select(g => new GastoFinancieroDto
        {
            Id = g.Id,
            CategoriaFinanciera = g.CategoriaFinanciera!.Nombre,
            Descripcion = g.Descripcion,
            Proveedor = g.Proveedor,
            Fecha = g.Fecha,
            Monto = g.Monto,
            Moneda = g.Moneda.ToString(),
            MetodoPago = g.MetodoPago,
            NumeroReferencia = g.NumeroReferencia,
            RutaComprobante = g.ArchivoComprobante?.RutaArchivo,
            Estado = g.Estado.ToString(),
            PeriodoNombre = g.PeriodoFinanciero!.Nombre,
            RegistradoPor = usuarios.TryGetValue(g.RegistradoPorUsuarioId, out var correo) ? correo : g.RegistradoPorUsuarioId,
            MotivoAnulacion = g.MotivoAnulacion
        }).ToList();
    }

    public async Task AnularAsync(int gastoId, string motivo, string usuarioId, string? ip)
    {
        var gasto = await _db.GastosFinancieros.Include(g => g.PeriodoFinanciero).FirstOrDefaultAsync(g => g.Id == gastoId)
            ?? throw new InvalidOperationException("No se encontró el gasto.");

        if (gasto.Estado == EstadoMovimientoFinanciero.Anulado) return;

        if (gasto.PeriodoFinanciero?.Cerrado == true)
        {
            throw new InvalidOperationException(
                "Este gasto pertenece a un período ya cerrado. Cualquier corrección debe hacerse mediante un ajuste autorizado, no una anulación directa.");
        }

        var estadoAnterior = gasto.Estado.ToString();
        gasto.Estado = EstadoMovimientoFinanciero.Anulado;
        gasto.MotivoAnulacion = motivo;
        gasto.FechaAnulacion = DateTime.UtcNow;
        gasto.AnuladoPorUsuarioId = usuarioId;
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(usuarioId, "Anulacion", "Gastos", $"GastoFinanciero#{gastoId}",
            estadoAnterior, $"Anulado: {motivo}", ip);
    }

    /// <summary>Valida extensión, tamaño y evita que el archivo se pueda
    /// ejecutar como código — antes de guardarlo con el mismo servicio
    /// seguro de archivos que ya usa el resto del sistema (CVs, logos).</summary>
    private async Task<int> GuardarComprobanteValidadoAsync(IFormFile archivo, string usuarioId)
    {
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(extension))
        {
            throw new InvalidOperationException("El comprobante debe ser PDF, JPG o PNG.");
        }

        if (archivo.Length > TamanoMaximoBytes)
        {
            throw new InvalidOperationException("El comprobante no puede pesar más de 5 MB.");
        }

        if (archivo.Length == 0)
        {
            throw new InvalidOperationException("El archivo del comprobante está vacío.");
        }

        var ruta = await _fileStorage.GuardarArchivoAsync(archivo, "comprobantes-gastos");

        var registroArchivo = new Archivo
        {
            UsuarioId = usuarioId,
            TipoArchivo = "ComprobanteGasto",
            RutaArchivo = ruta,
            NombreOriginal = archivo.FileName,
            PesoBytes = (int)archivo.Length,
            FechaSubida = DateTime.UtcNow
        };
        _db.Archivos.Add(registroArchivo);
        await _db.SaveChangesAsync();

        return registroArchivo.Id;
    }
}
