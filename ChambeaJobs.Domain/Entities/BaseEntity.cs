namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Clase base para entidades con clave primaria entera autoincremental.
/// Todas las entidades del dominio (Candidato, Empresa, Vacante, etc.)
/// heredan de aquí para mantener consistencia y evitar repetir "public int Id".
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}
