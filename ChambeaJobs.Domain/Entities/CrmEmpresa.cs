using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Ficha comercial interna de una empresa (prospecto o cliente). Puede o no
/// estar ligada a una cuenta real registrada en la plataforma (EmpresaId nulo
/// = todavía es solo un prospecto que no se ha registrado).
/// </summary>
public class CrmEmpresa
{
    public int Id { get; set; }

    public int? EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public string NombreEmpresa { get; set; } = string.Empty;
    public EtapaPipelineCrm Etapa { get; set; } = EtapaPipelineCrm.Prospecto;

    public string? ContactoPrincipal { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? RUC { get; set; }
    public string? Direccion { get; set; }
    public string? SectorEmpresarial { get; set; }
    public string? TamanoEmpresa { get; set; }
    public string? SitioWeb { get; set; }
    public string? RedesSociales { get; set; }
    public string? Observaciones { get; set; }

    public string UsuarioCreadorId { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    public ICollection<CrmActividad> Actividades { get; set; } = new List<CrmActividad>();
    public ICollection<CrmArchivoAdjunto> ArchivosAdjuntos { get; set; } = new List<CrmArchivoAdjunto>();
}
