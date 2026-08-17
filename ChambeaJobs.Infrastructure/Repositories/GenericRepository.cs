using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Repositories;

/// <inheritdoc cref="IRepository{T}"/>
public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext Contexto;
    protected readonly DbSet<T> ConjuntoDatos;

    public GenericRepository(ApplicationDbContext contexto)
    {
        Contexto = contexto;
        ConjuntoDatos = contexto.Set<T>();
    }

    public async Task<T?> ObtenerPorIdAsync(int id) => await ConjuntoDatos.FindAsync(id);

    public async Task<IEnumerable<T>> ObtenerTodosAsync() => await ConjuntoDatos.ToListAsync();

    public async Task AgregarAsync(T entidad) => await ConjuntoDatos.AddAsync(entidad);

    public void Actualizar(T entidad) => ConjuntoDatos.Update(entidad);

    public void Eliminar(T entidad) => ConjuntoDatos.Remove(entidad);

    public async Task<int> GuardarCambiosAsync() => await Contexto.SaveChangesAsync();
}
