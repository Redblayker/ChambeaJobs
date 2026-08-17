namespace ChambeaJobs.Domain.Entities;

public class CrmArchivoAdjunto
{
    public int Id { get; set; }

    public int CrmEmpresaId { get; set; }
    public CrmEmpresa CrmEmpresa { get; set; } = null!;

    public string NombreOriginal { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public string UsuarioId { get; set; } = string.Empty;
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
}
