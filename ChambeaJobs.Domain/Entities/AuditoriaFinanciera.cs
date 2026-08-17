namespace ChambeaJobs.Domain.Entities;

/// <summary>Auditoría dedicada y más estricta que la genérica del sistema
/// (ver Auditoria.cs) — específica del módulo financiero, porque es
/// información confidencial y sensible. Registra valor anterior/nuevo, IP
/// y resultado de la operación, además de lo que ya guarda la auditoría
/// general.</summary>
public class AuditoriaFinanciera : BaseEntity
{
    public string UsuarioId { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;

    /// <summary>Creacion | Modificacion | Anulacion | Ajuste | EliminacionLogica | CambioEstado | CierrePeriodo.</summary>
    public string Accion { get; set; } = string.Empty;

    public string Modulo { get; set; } = string.Empty; // Ingresos | Gastos | Categorias | Periodos | Reportes
    public string RegistroAfectado { get; set; } = string.Empty; // ej. "GastoFinanciero#42"

    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }

    public string? DireccionIp { get; set; }

    /// <summary>Exito | Fallido | Denegado.</summary>
    public string Resultado { get; set; } = "Exito";
}
