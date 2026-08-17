namespace ChambeaJobs.Domain.Entities;

/// <summary>Registro histórico de acciones administrativas críticas.</summary>
public class Auditoria : BaseEntity
{
    public string UsuarioId { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty; // Crear, Editar, Eliminar, Suspender, Activar..
    public string EntidadAfectada { get; set; } = string.Empty; // nombre de la entidad/tabla
    public string EntidadId { get; set; } = string.Empty;
    public string? DetalleJson { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
}
