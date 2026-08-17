namespace ChambeaJobs.Domain.Entities;

/// <summary>
/// Subcategoría dentro de una Categoría (ej. Categoría "Tecnología" → Carreras
/// "Ingeniería en Ciencias de la Computación", "Ingeniería en Sistemas"...).
/// Antes solo existía el catálogo de Categorías, demasiado amplio para un
/// candidato que quiere indicar específicamente qué estudió — sobre todo en
/// categorías con muchas carreras distintas como Tecnología o Salud.
///
/// A propósito NO reemplaza a Categoria en la validación de "solo puedes
/// postularte a vacantes de tu misma área": esa sigue basándose en
/// Educacion.CategoriaId (el área amplia), para no romper esa regla ya
/// existente. Carrera es información adicional, más específica, que se
/// muestra en el perfil/CV pero no participa en el filtro de postulación.
/// </summary>
/// <summary>
/// Subcategoría dentro de una Categoría (ej. Categoría "Tecnología" → Carreras
/// "Ingeniería en Ciencias de la Computación", "Ingeniería en Sistemas"...).
/// Antes solo existía el catálogo de Categorías, demasiado amplio para un
/// candidato que quiere indicar específicamente qué estudió — sobre todo en
/// categorías con muchas carreras distintas como Tecnología o Salud.
///
/// A propósito NO reemplaza a Categoria en la validación de "solo puedes
/// postularte a vacantes de tu misma área": esa sigue basándose en
/// Educacion.CategoriaId (el área amplia), para no romper esa regla ya
/// existente. Carrera es información adicional, más específica, que se
/// muestra en el perfil/CV pero no participa en el filtro de postulación.
///
/// Cada Carrera pertenece a UNA Facultad de UNA Institución (ya no es M:N
/// como antes) — si dos universidades ofrecen "Ingeniería Civil", son dos
/// registros de Carrera distintos, cada uno bajo la Facultad real de su
/// propia universidad. Esto refleja con más precisión el catálogo real.
/// </summary>
public class Carrera : BaseEntity
{
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public int FacultadId { get; set; }
    public Facultad? Facultad { get; set; }
}
