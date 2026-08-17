using ChambeaJobs.Application.DTOs;

namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Lógica de negocio del perfil de candidato: obtener el perfil para
/// mostrarlo, editarlo, y gestionar experiencias/educaciones/habilidades.
/// El controlador (ChambeaJobs.Web) solo orquesta HTTP; toda la regla
/// de negocio vive aquí (Service Layer).
/// </summary>
public interface ICandidatoService
{
    Task<CandidatoPerfilDto?> ObtenerPerfilPorUsuarioIdAsync(string usuarioId);

    /// <summary>Usado por Empresa para ver el perfil de un candidato postulado.</summary>
    Task<CandidatoPerfilDto?> ObtenerPerfilPorIdAsync(int candidatoId);

    /// <summary>
    /// Crea el registro de Candidato al momento del registro de cuenta
    /// (conecta con el TODO dejado en AccountController.RegistroCandidato).
    /// </summary>
    Task CrearPerfilInicialAsync(string usuarioId, string nombres, string apellidos);

    Task ActualizarDatosBasicosAsync(string usuarioId, EditarPerfilCandidatoDto datos);

    Task<int> AgregarExperienciaAsync(string usuarioId, ExperienciaDto experiencia);
    Task EliminarExperienciaAsync(string usuarioId, int experienciaId);

    Task<int> AgregarEducacionAsync(string usuarioId, EducacionDto educacion);
    Task EliminarEducacionAsync(string usuarioId, int educacionId);

    Task AgregarHabilidadAsync(string usuarioId, string nombreHabilidad, string? nivelDominio);
    Task EliminarHabilidadAsync(string usuarioId, int habilidadId);

    // ---------- Idiomas ----------
    Task<List<IdiomaOptionDto>> ObtenerIdiomasDisponiblesAsync();
    Task AgregarIdiomaAsync(string usuarioId, int idiomaId, string nivel);
    Task EliminarIdiomaAsync(string usuarioId, int idiomaId);

    // ---------- Certificados (PDF) ----------
    Task AgregarCertificadoAsync(string usuarioId, string nombre, string? institucionEmisora, DateTime? fechaObtencion, Microsoft.AspNetCore.Http.IFormFile archivoPdf, string tipoDocumento);
    Task EliminarCertificadoAsync(string usuarioId, int certificadoId);

    // ---------- Cursos ----------
    Task AgregarCursoAsync(string usuarioId, string nombre, string? institucion, int? horasDuracion, DateTime? fechaFinalizacion, Microsoft.AspNetCore.Http.IFormFile? archivoPdf);
    Task EliminarCursoAsync(string usuarioId, int cursoId);
}
