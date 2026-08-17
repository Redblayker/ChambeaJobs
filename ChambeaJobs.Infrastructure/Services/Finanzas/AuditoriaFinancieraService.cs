using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Data;
using ChambeaJobs.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Services.Finanzas;

public class AuditoriaFinancieraService : IAuditoriaFinancieraService
{
    private readonly ApplicationDbContext _db;

    public AuditoriaFinancieraService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task RegistrarAsync(string usuarioId, string accion, string modulo, string registroAfectado,
        string? valorAnterior, string? valorNuevo, string? ip, string resultado = "Exito")
    {
        _db.AuditoriasFinancieras.Add(new AuditoriaFinanciera
        {
            UsuarioId = usuarioId,
            Accion = accion,
            Modulo = modulo,
            RegistroAfectado = registroAfectado,
            ValorAnterior = valorAnterior,
            ValorNuevo = valorNuevo,
            DireccionIp = ip,
            Resultado = resultado,
            FechaHora = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditoriaFinancieraDto>> ObtenerRecientesAsync(int cantidad = 200)
    {
        var registros = await _db.AuditoriasFinancieras
            .OrderByDescending(a => a.FechaHora)
            .Take(cantidad)
            .ToListAsync();

        var usuarioIds = registros.Select(r => r.UsuarioId).Distinct().ToList();
        var usuarios = await _db.Users.Where(u => usuarioIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Email ?? u.Id);

        return registros.Select(r => new AuditoriaFinancieraDto
        {
            UsuarioNombre = usuarios.TryGetValue(r.UsuarioId, out var correo) ? correo : r.UsuarioId,
            FechaHora = r.FechaHora,
            Accion = r.Accion,
            Modulo = r.Modulo,
            RegistroAfectado = r.RegistroAfectado,
            ValorAnterior = r.ValorAnterior,
            ValorNuevo = r.ValorNuevo,
            Resultado = r.Resultado
        }).ToList();
    }
}
