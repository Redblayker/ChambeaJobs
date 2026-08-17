namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Plan de suscripción que una Empresa puede elegir:
///   - Básico: $20 / 10 vacantes / 30 días — sin evaluación psicométrica ni Video CV.
///     Pensado para empresas pequeñas que no contratan tan seguido.
///   - Empresarial: $50 / vacantes ilimitadas / 30 días — acceso completo
///     (evaluación psicométrica, Video CV, etc.)
/// VacantesIncluidas en null significa "sin límite" (plan Empresarial).
/// </summary>
public class PlanSuscripcion : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int? VacantesIncluidas { get; set; }
    public int DiasVigencia { get; set; } = 30;

    /// <summary>Si es false, ya no se ofrece para nuevas suscripciones (pero se conserva el historial de quienes ya lo tenían).</summary>
    public bool Activo { get; set; } = true;

    /// <summary>Si el plan permite enviar/ver evaluaciones psicométricas a los candidatos.</summary>
    public bool IncluyePruebaPsicometrica { get; set; }

    /// <summary>Si el plan permite ver/comentar el Video Currículum de los candidatos.</summary>
    public bool IncluyeVideoCv { get; set; }

    /// <summary>Si el plan permite marcar vacantes como destacadas (aparecen primero en las búsquedas, con estrella).</summary>
    public bool PermiteVacantesDestacadas { get; set; }

    public static class Nombres
    {
        public const string Basico = "Básico";
        public const string Empresarial = "Empresarial";
    }
}
