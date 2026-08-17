using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Services.Finanzas;

public class CategoriaFinancieraService : ICategoriaFinancieraService
{
    private readonly ApplicationDbContext _db;

    public CategoriaFinancieraService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoriaFinancieraDto>> ListarAsync(TipoCategoriaFinanciera? tipo = null)
    {
        var query = _db.CategoriasFinancieras.AsQueryable();
        if (tipo.HasValue) query = query.Where(c => c.Tipo == tipo.Value);

        return await query
            .OrderBy(c => c.Tipo).ThenBy(c => c.Nombre)
            .Select(c => new CategoriaFinancieraDto
            {
                Id = c.Id,
                Tipo = c.Tipo,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Activa = c.Activa
            })
            .ToListAsync();
    }

    public async Task<int> CrearAsync(CategoriaFinancieraFormDto dto)
    {
        var yaExiste = await _db.CategoriasFinancieras.AnyAsync(c => c.Tipo == dto.Tipo && c.Nombre == dto.Nombre);
        if (yaExiste) throw new InvalidOperationException("Ya existe una categoría con ese nombre en ese tipo.");

        var categoria = new CategoriaFinanciera { Tipo = dto.Tipo, Nombre = dto.Nombre, Descripcion = dto.Descripcion, Activa = true };
        _db.CategoriasFinancieras.Add(categoria);
        await _db.SaveChangesAsync();
        return categoria.Id;
    }

    public async Task ActualizarAsync(int id, CategoriaFinancieraFormDto dto)
    {
        var categoria = await _db.CategoriasFinancieras.FindAsync(id)
            ?? throw new InvalidOperationException("No se encontró la categoría.");

        categoria.Tipo = dto.Tipo;
        categoria.Nombre = dto.Nombre;
        categoria.Descripcion = dto.Descripcion;
        await _db.SaveChangesAsync();
    }

    public async Task DesactivarAsync(int id)
    {
        var categoria = await _db.CategoriasFinancieras.FindAsync(id);
        if (categoria is null) return;

        categoria.Activa = !categoria.Activa;
        await _db.SaveChangesAsync();
    }
}
