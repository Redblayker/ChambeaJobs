namespace ChambeaJobs.Domain.Entities;

/// <summary>Formación académica registrada en el perfil de un Candidato.</summary>
public class Educacion : BaseEntity
{
    public int CandidatoId { get; set; }
    public Candidato? Candidato { get; set; }
    public string Institucion { get; set; } = string.Empty;

    /// <summary>
    /// Institución del catálogo (universidad/INATEC), si el candidato eligió
    /// una de la lista en vez de escribir el nombre a mano. Nulo si escribió
    /// "Otra institución" — en ese caso, el campo Institucion (texto) de
    /// arriba sigue siendo la única fuente del nombre. Cuando SÍ hay
    /// InstitucionId, el campo Institucion (texto) se llena automáticamente
    /// con el nombre de esta institución al guardar, para no duplicar lógica
    /// de visualización en el resto del sistema.
    /// </summary>
    public int? InstitucionId { get; set; }
    public Institucion? InstitucionCatalogo { get; set; }

    public string TituloObtenido { get; set; } = string.Empty;
    public string NivelEducativo { get; set; } = string.Empty; // Bachillerato/Técnico/Universitario/Posgrado
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }

    /// <summary>
    /// Categoría/área profesional a la que corresponde este título (mismo
    /// catálogo que usan las vacantes). Se usa para validar que un candidato
    /// solo pueda postularse a vacantes de la carrera que estudió.
    /// </summary>
    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    /// <summary>Carrera específica dentro de la Categoría (opcional — más detallada, ej. "Ingeniería en Sistemas" dentro de "Tecnología").</summary>
    public int? CarreraId { get; set; }
    public Carrera? Carrera { get; set; }
}
