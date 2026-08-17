using ChambeaJobs.Application.DTOs;

namespace ChambeaJobs.Application.Interfaces;

public interface IPostulacionService
{
    /// <summary>Postula un candidato a una vacante. Lanza InvalidOperationException si ya se postuló o la vacante está cerrada.</summary>
    Task PostularAsync(int candidatoId, int vacanteId);

    Task<bool> YaPostuloAsync(int candidatoId, int vacanteId);

    Task<List<PostulacionCandidatoDto>> ObtenerMisPostulacionesAsync(int candidatoId);

    Task<List<CandidatoPostuladoDto>> ObtenerPostulantesDeVacanteAsync(int empresaId, int vacanteId);

    Task CambiarEstadoAsync(int empresaId, int postulacionId, int nuevoEstadoId);

    Task<List<EstadoPostulacionOptionDto>> ObtenerEstadosDisponiblesAsync();

    /// <summary>KPIs del Dashboard de Empresa: vacantes activas, candidatos nuevos, entrevistas hoy, pruebas pendientes, contrataciones del mes.</summary>
    Task<EmpresaDashboardResumenDto> ObtenerResumenDashboardAsync(int empresaId);

    /// <summary>Compara lado a lado 2 o más candidatos postulados a la misma vacante.</summary>
    Task<List<CandidatoComparacionDto>> ObtenerComparacionAsync(int empresaId, int vacanteId, List<int> postulacionIds);

    /// <summary>Comentarios del reclutador sobre el video CV de un candidato, para una postulación puntual.</summary>
    Task<List<ComentarioVideoCvDto>> ObtenerComentariosVideoCvAsync(int empresaId, int postulacionId);

    /// <summary>Datos del candidato y su video CV para la pantalla de comentarios del reclutador.</summary>
    Task<string?> MarcarCvRevisadoYObtenerUrlAsync(int empresaId, int postulacionId);
    Task<DetalleVideoCvDto> ObtenerDetalleVideoCvAsync(int empresaId, int postulacionId);

    /// <summary>Agrega un comentario del reclutador al video CV y marca la postulación como "video visto".</summary>
    Task ComentarVideoCvAsync(int empresaId, int postulacionId, string reclutadorUsuarioId, string reclutadorNombre, string comentario, int? calificacion);

    // ---------- Video entrevista (Jitsi Meet) ----------
    Task<string> ObtenerSalaVideollamadaEmpresaAsync(int empresaId, int postulacionId);
    Task<string> ObtenerSalaVideollamadaCandidatoAsync(int candidatoId, int postulacionId);

    /// <summary>Registra el resultado de la prueba psicométrica que el candidato acaba de responder.</summary>
    Task ResponderPruebaPsicometricaAsync(int candidatoId, int postulacionId, int puntaje);

    /// <summary>La empresa programa/reprograma la entrevista de una postulación.</summary>
    Task ProgramarEntrevistaAsync(int empresaId, int postulacionId, DateTime fechaEntrevista, string? nota);
}
