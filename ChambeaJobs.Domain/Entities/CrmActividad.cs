using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Domain.Entities;

public class CrmActividad
{
    public int Id { get; set; }

    public int CrmEmpresaId { get; set; }
    public CrmEmpresa CrmEmpresa { get; set; } = null!;

    public TipoActividadCrm Tipo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaActividad { get; set; } = DateTime.UtcNow;

    public string UsuarioId { get; set; } = string.Empty;
    public string UsuarioNombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
