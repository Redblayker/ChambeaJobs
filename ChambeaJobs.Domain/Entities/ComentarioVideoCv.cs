namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Comentario que un reclutador (usuario Empresa) deja sobre el video
/// currículum de un candidato, en el contexto de una postulación puntual.
/// Se ancla a la Postulación (y no solo al Candidato) para que el comentario
/// tenga contexto de "para qué vacante" fue evaluado el video.
/// </summary>
public class ComentarioVideoCv : BaseEntity
{
    public int PostulacionId { get; set; }
    public Postulacion? Postulacion { get; set; }

    /// <summary>UsuarioId (Identity) del reclutador que escribió el comentario.</summary>
    public string ReclutadorUsuarioId { get; set; } = string.Empty;
    public string ReclutadorNombre { get; set; } = string.Empty;

    public string Comentario { get; set; } = string.Empty;

    /// <summary>Calificación opcional en estrellas (1-5) que el reclutador da al video.</summary>
    public int? Calificacion { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
