namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Facultad o área de conocimiento dentro de una Institución (ej. "Facultad
/// de Ciencias e Ingeniería" dentro de UNAN-Managua). Nivel intermedio del
/// catálogo de 3 pasos: Institución → Facultad → Carrera — cada Carrera
/// pertenece a exactamente una Facultad, y cada Facultad a exactamente una
/// Institución, así que al elegir la universidad primero se filtran sus
/// facultades, y al elegir la facultad se filtran sus carreras reales.
/// </summary>
public class Facultad : BaseEntity
{
    public int InstitucionId { get; set; }
    public Institucion? Institucion { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public List<Carrera> Carreras { get; set; } = new();
}
