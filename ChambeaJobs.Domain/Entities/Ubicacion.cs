namespace ChambeaJobs.Domain.Entities;

/// <summary>Catálogo de ubicaciones.</summary>
public class Ubicacion : BaseEntity
{
    public string Departamento { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;

    public string NombreCompleto => $"{Ciudad}, {Departamento}";
}
