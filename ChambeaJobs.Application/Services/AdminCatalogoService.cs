using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="IAdminCatalogoService"/>
public class AdminCatalogoService : IAdminCatalogoService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminCatalogoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ---------- Categorías ----------

    public async Task<List<CategoriaFormDto>> ObtenerCategoriasAsync()
    {
        var categorias = await _unitOfWork.Categorias.ObtenerTodosAsync();
        var vacantes = await _unitOfWork.Vacantes.ObtenerTodosAsync();

        return categorias
            .Select(c => new CategoriaFormDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                VacantesAsociadas = vacantes.Count(v => v.CategoriaId == c.Id)
            })
            .OrderBy(c => c.Nombre)
            .ToList();
    }

    public async Task CrearCategoriaAsync(string nombre, string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre de la categoría es obligatorio.");
        }

        var categorias = await _unitOfWork.Categorias.ObtenerTodosAsync();
        if (categorias.Any(c => c.Nombre.Equals(nombre.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Ya existe una categoría con ese nombre.");
        }

        await _unitOfWork.Categorias.AgregarAsync(new Categoria { Nombre = nombre.Trim(), Descripcion = descripcion?.Trim() });
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task EditarCategoriaAsync(int id, string nombre, string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre de la categoría es obligatorio.");
        }

        var categoria = await _unitOfWork.Categorias.ObtenerPorIdAsync(id)
            ?? throw new InvalidOperationException("Esta categoría no existe.");

        var categorias = await _unitOfWork.Categorias.ObtenerTodosAsync();
        if (categorias.Any(c => c.Id != id && c.Nombre.Equals(nombre.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Ya existe otra categoría con ese nombre.");
        }

        categoria.Nombre = nombre.Trim();
        categoria.Descripcion = descripcion?.Trim();

        _unitOfWork.Categorias.Actualizar(categoria);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task EliminarCategoriaAsync(int id)
    {
        var vacantes = await _unitOfWork.Vacantes.ObtenerTodosAsync();
        if (vacantes.Any(v => v.CategoriaId == id))
        {
            throw new InvalidOperationException("No se puede eliminar esta categoría porque tiene vacantes asociadas.");
        }

        var categoria = await _unitOfWork.Categorias.ObtenerPorIdAsync(id)
            ?? throw new InvalidOperationException("Esta categoría no existe.");

        _unitOfWork.Categorias.Eliminar(categoria);
        await _unitOfWork.GuardarCambiosAsync();
    }

    // ---------- Ubicaciones ----------

    public async Task<List<UbicacionFormDto>> ObtenerUbicacionesAsync()
    {
        var ubicaciones = await _unitOfWork.Ubicaciones.ObtenerTodosAsync();
        var vacantes = await _unitOfWork.Vacantes.ObtenerTodosAsync();

        return ubicaciones
            .Select(u => new UbicacionFormDto
            {
                Id = u.Id,
                Departamento = u.Departamento,
                Ciudad = u.Ciudad,
                VacantesAsociadas = vacantes.Count(v => v.UbicacionId == u.Id)
            })
            .OrderBy(u => u.Departamento)
            .ToList();
    }

    public async Task CrearUbicacionAsync(string departamento, string ciudad)
    {
        if (string.IsNullOrWhiteSpace(departamento) || string.IsNullOrWhiteSpace(ciudad))
        {
            throw new ArgumentException("El departamento y la ciudad son obligatorios.");
        }

        var ubicaciones = await _unitOfWork.Ubicaciones.ObtenerTodosAsync();
        if (ubicaciones.Any(u =>
                u.Departamento.Equals(departamento.Trim(), StringComparison.OrdinalIgnoreCase) &&
                u.Ciudad.Equals(ciudad.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Ya existe esa combinación de departamento y ciudad.");
        }

        await _unitOfWork.Ubicaciones.AgregarAsync(new Ubicacion { Departamento = departamento.Trim(), Ciudad = ciudad.Trim() });
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task EditarUbicacionAsync(int id, string departamento, string ciudad)
    {
        if (string.IsNullOrWhiteSpace(departamento) || string.IsNullOrWhiteSpace(ciudad))
        {
            throw new ArgumentException("El departamento y la ciudad son obligatorios.");
        }

        var ubicacion = await _unitOfWork.Ubicaciones.ObtenerPorIdAsync(id)
            ?? throw new InvalidOperationException("Esta ubicación no existe.");

        var ubicaciones = await _unitOfWork.Ubicaciones.ObtenerTodosAsync();
        if (ubicaciones.Any(u => u.Id != id &&
                u.Departamento.Equals(departamento.Trim(), StringComparison.OrdinalIgnoreCase) &&
                u.Ciudad.Equals(ciudad.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Ya existe otra ubicación con esa combinación de departamento y ciudad.");
        }

        ubicacion.Departamento = departamento.Trim();
        ubicacion.Ciudad = ciudad.Trim();

        _unitOfWork.Ubicaciones.Actualizar(ubicacion);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task EliminarUbicacionAsync(int id)
    {
        var vacantes = await _unitOfWork.Vacantes.ObtenerTodosAsync();
        if (vacantes.Any(v => v.UbicacionId == id))
        {
            throw new InvalidOperationException("No se puede eliminar esta ubicación porque tiene vacantes asociadas.");
        }

        var ubicacion = await _unitOfWork.Ubicaciones.ObtenerPorIdAsync(id)
            ?? throw new InvalidOperationException("Esta ubicación no existe.");

        _unitOfWork.Ubicaciones.Eliminar(ubicacion);
        await _unitOfWork.GuardarCambiosAsync();
    }
}
