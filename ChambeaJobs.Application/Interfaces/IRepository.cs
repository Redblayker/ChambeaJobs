namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Repositorio genérico (Repository Pattern). Cada entidad puede
/// tener además un repositorio específico (ej. ICandidatoRepository)
/// para consultas propias que no aplican a todas las entidades.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<T>> ObtenerTodosAsync();
    Task AgregarAsync(T entidad);
    void Actualizar(T entidad);
    void Eliminar(T entidad);
    Task<int> GuardarCambiosAsync();
}
