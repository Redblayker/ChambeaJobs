namespace ChambeaJobs.Domain.Entities;

/// <summary>Catálogo de estados del ciclo de vida de una postulación.</summary>
public class EstadoPostulacion : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;

    public static class Nombres
    {
        public const string Postulado = "Postulado";
        public const string EnRevision = "En revisión";
        public const string Entrevista = "Entrevista";
        public const string Contratado = "Contratado";
        public const string Rechazado = "Rechazado";
    }
}
