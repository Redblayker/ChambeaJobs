namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Entidad genérica para cualquier archivo subido al sistema
/// (CV en PDF, logo de empresa, foto de perfil)./// </summary>
public class Archivo : BaseEntity
{
    public string UsuarioId { get; set; } = string.Empty;
    public string TipoArchivo { get; set; } = string.Empty; // "CV", "Logo", "FotoPerfil"
    public string RutaArchivo { get; set; } = string.Empty;
    public string NombreOriginal { get; set; } = string.Empty;
    public int PesoBytes { get; set; }
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
}
