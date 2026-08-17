namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Parámetros globales del sistema, clave-valor. Usada principalmente
/// para las reglas del paquete de publicación, sin necesidad de
/// "hardcodearlas" en el código.
/// </summary>
public class ConfiguracionSistema : BaseEntity
{
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public static class Claves
    {
        public const string PaqueteVacantesPrecio = "PaqueteVacantesPrecio";
        public const string PaqueteVacantesCantidad = "PaqueteVacantesCantidad";
        public const string PaqueteVacantesDiasVigencia = "PaqueteVacantesDiasVigencia";
    }
}
