using System.ComponentModel.DataAnnotations;

namespace ChambeaJobs.Application.DTOs;

/// <summary>Estado del paquete vigente, mostrado en el Dashboard Empresa.</summary>
public class PaqueteEstadoDto
{
    public bool TienePaquete { get; set; }
    public int? VacantesIncluidas { get; set; }
    public int VacantesConsumidas { get; set; }
    public int? VacantesDisponibles => VacantesIncluidas.HasValue ? VacantesIncluidas.Value - VacantesConsumidas : null;
    public DateTime? FechaVencimiento { get; set; }
    public int? DiasRestantes { get; set; }
    public string Estado { get; set; } = string.Empty; // Pendiente, Vigente, Agotado, Vencido, Rechazado
    public bool PuedePublicar { get; set; }
    public string? NombrePlan { get; set; }
    public bool IncluyePruebaPsicometrica { get; set; }
    public bool IncluyeVideoCv { get; set; }
    public bool PermiteVacantesDestacadas { get; set; }
}

/// <summary>DTO para registrar la solicitud de pago manual.</summary>
public class RegistrarPagoDto
{
    [Required(ErrorMessage = "Selecciona el plan que quieres pagar")]
    public int PlanSuscripcionId { get; set; }

    [Required(ErrorMessage = "Indica la referencia de tu transferencia/depósito")]
    [StringLength(150)]
    public string ReferenciaTransaccion { get; set; } = string.Empty;

    public decimal MontoInformativo { get; set; } = 20.00m; // solo se muestra en la vista, no editable
}

/// <summary>Fila del historial de pagos de la empresa.</summary>
/// <summary>Opción simple de plan, para el <select>/tarjetas de selección en formularios.</summary>
public class PlanSuscripcionOptionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int? VacantesIncluidas { get; set; }
    public int DiasVigencia { get; set; }
    public bool IncluyePruebaPsicometrica { get; set; }
    public bool IncluyeVideoCv { get; set; }
    public bool PermiteVacantesDestacadas { get; set; }

    public string Descripcion =>
        VacantesIncluidas.HasValue
            ? $"${Precio:0} / {VacantesIncluidas} vacantes / {DiasVigencia} días"
            : $"${Precio:0} / vacantes ilimitadas / {DiasVigencia} días";
}

/// <summary>DTO editable para que el Admin ajuste precio/cantidad de cada plan.</summary>
public class EditarPlanSuscripcionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int? VacantesIncluidas { get; set; }
    public int DiasVigencia { get; set; }
    public bool Activo { get; set; }
    public bool IncluyePruebaPsicometrica { get; set; }
    public bool IncluyeVideoCv { get; set; }
    public bool PermiteVacantesDestacadas { get; set; }
}

public class PagoHistorialDto
{
    public int PagoId { get; set; }
    public int PaqueteId { get; set; }
    public DateTime FechaCompra { get; set; }
    public decimal Monto { get; set; }
    public int? VacantesIncluidas { get; set; }
    public int VacantesConsumidas { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string EstadoPaquete { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;
    public string? ReferenciaTransaccion { get; set; }
}

/// <summary>Fila del historial completo de pagos que ve el Administrador (todas las empresas).</summary>
public class PagoAdminDto
{
    public int PagoId { get; set; }
    public string EmpresaNombre { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; }
    public decimal Monto { get; set; }
    public string EstadoPago { get; set; } = string.Empty;
    public string? ReferenciaTransaccion { get; set; }
    public DateTime? FechaRevision { get; set; }
    public string? ComentarioRevision { get; set; }
}

/// <summary>Fila de la cola de aprobación de pagos del Administrador.</summary>
public class PagoPendienteDto
{
    public int PagoId { get; set; }
    public int PaqueteId { get; set; }
    public string EmpresaNombre { get; set; } = string.Empty;
    public string EmpresaCorreo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? ReferenciaTransaccion { get; set; }
    public DateTime FechaSolicitud { get; set; }
}
