using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="IPostulacionService"/>
public class PostulacionService : IPostulacionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompatibilidadService _compatibilidadService;
    private readonly INotificacionService _notificacionService;

    public PostulacionService(IUnitOfWork unitOfWork, ICompatibilidadService compatibilidadService, INotificacionService notificacionService)
    {
        _unitOfWork = unitOfWork;
        _compatibilidadService = compatibilidadService;
        _notificacionService = notificacionService;
    }

    public async Task PostularAsync(int candidatoId, int vacanteId)
    {
        if (await _unitOfWork.Postulaciones.ExistePostulacionAsync(candidatoId, vacanteId))
        {
            throw new InvalidOperationException("Ya te has postulado a esta vacante.");
        }

        var vacante = await _unitOfWork.Vacantes.ObtenerConDetalleAsync(vacanteId)
            ?? throw new InvalidOperationException("Esta vacante no existe.");

        if (vacante.Estado != EstadosVacante.Activa)
        {
            throw new InvalidOperationException("Esta vacante ya no acepta postulaciones.");
        }

        var candidato = await _unitOfWork.Candidatos.ObtenerConDetallePorIdAsync(candidatoId)
            ?? throw new InvalidOperationException("No se encontró tu perfil de candidato.");

        // Regla de negocio: solo se exige coincidencia de carrera cuando la
        // vacante realmente pide una carrera específica (CarreraId != null).
        // Muchos empleos (motorizado, ayudante de producción, bombero de
        // gasolinera, etc.) no requieren estudios universitarios/técnicos,
        // así que la empresa deja "Carrera específica" vacío al publicarlos,
        // y en ese caso cualquier candidato puede postularse sin importar
        // su nivel de estudios.
        if (vacante.CarreraId.HasValue)
        {
            var estudioLaCarreraDeLaVacante = candidato.Educaciones.Any(e => e.CarreraId == vacante.CarreraId);
            if (!estudioLaCarreraDeLaVacante)
            {
                throw new InvalidOperationException(
                    $"Esta vacante requiere la carrera \"{vacante.Carrera?.Nombre}\", pero ninguno de los estudios de tu perfil corresponde a ella. " +
                    "Puedes revisar/agregar tu educación desde \"Editar perfil\".");
            }
        }

        var postulacion = new Postulacion
        {
            CandidatoId = candidatoId,
            VacanteId = vacanteId,
            EstadoPostulacionId = 1, // "Postulado" — sembrado con Id=1
            FechaPostulacion = DateTime.UtcNow
        };

        await _unitOfWork.Postulaciones.AgregarAsync(postulacion);
        await _unitOfWork.GuardarCambiosAsync();

        // 🔔 Notificación al reclutador: nueva postulación.
        if (!string.IsNullOrWhiteSpace(vacante.Empresa?.UsuarioId))
        {
            var nombreCandidato = $"{candidato.Nombres} {candidato.Apellidos}".Trim();

            await _notificacionService.CrearAsync(
                vacante.Empresa!.UsuarioId,
                Notificacion.Tipos.NuevaPostulacion,
                $"🔔 {nombreCandidato} se postuló a \"{vacante.Titulo}\".",
                $"/Empresa/CandidatosPostulados?vacanteId={vacanteId}");
        }
    }

    public async Task<bool> YaPostuloAsync(int candidatoId, int vacanteId) =>
        await _unitOfWork.Postulaciones.ExistePostulacionAsync(candidatoId, vacanteId);

    public async Task<List<PostulacionCandidatoDto>> ObtenerMisPostulacionesAsync(int candidatoId)
    {
        var postulaciones = await _unitOfWork.Postulaciones.ObtenerPorCandidatoAsync(candidatoId);

        return postulaciones
            .OrderByDescending(p => p.FechaPostulacion)
            .Select(p => new PostulacionCandidatoDto
            {
                Id = p.Id,
                VacanteId = p.VacanteId,
                VacanteTitulo = p.Vacante?.Titulo ?? string.Empty,
                EmpresaNombre = p.Vacante?.Empresa?.NombreEmpresa ?? string.Empty,
                EmpresaLogoUrl = p.Vacante?.Empresa?.LogoArchivo?.RutaArchivo,
                FechaPostulacion = p.FechaPostulacion,
                Estado = p.EstadoPostulacion?.Nombre ?? string.Empty,
                Etapa = p.EtapaActual(),
                CvRevisado = p.CvRevisado,
                VideoCvVisto = p.VideoCvVisto,
                PruebaPsicometricaAprobada = p.PruebaPsicometricaAprobada,
                PruebaPsicometricaPuntaje = p.PruebaPsicometricaPuntaje,
                EntrevistaProgramada = p.EntrevistaProgramada,
                FechaEntrevista = p.FechaEntrevista
            })
            .ToList();
    }

    public async Task<List<CandidatoPostuladoDto>> ObtenerPostulantesDeVacanteAsync(int empresaId, int vacanteId)
    {
        var vacante = await _unitOfWork.Vacantes.ObtenerConDetalleAsync(vacanteId);
        if (vacante is null || vacante.EmpresaId != empresaId)
        {
            throw new InvalidOperationException("Esta vacante no existe o no te pertenece.");
        }

        var postulaciones = (await _unitOfWork.Postulaciones.ObtenerPorVacanteAsync(vacanteId)).ToList();

        // Ranking automático: se recalcula el puntaje de compatibilidad de cada
        // candidato contra esta vacante y se persiste para reutilizarlo (ej. en
        // notificaciones o reportes) sin tener que recalcularlo cada vez.
        var huboCambios = false;
        foreach (var p in postulaciones)
        {
            if (p.Candidato is null) continue;

            var puntaje = _compatibilidadService.Calcular(p.Candidato, vacante);
            if (p.PuntajeCompatibilidad != puntaje)
            {
                p.PuntajeCompatibilidad = puntaje;
                _unitOfWork.Postulaciones.Actualizar(p);
                huboCambios = true;
            }
        }

        if (huboCambios)
        {
            await _unitOfWork.GuardarCambiosAsync();
        }

        return postulaciones
            // Ranking automático por compatibilidad (estrellas), no por fecha.
            .OrderByDescending(p => p.PuntajeCompatibilidad)
            .ThenByDescending(p => p.FechaPostulacion)
            .Select(p => new CandidatoPostuladoDto
            {
                PostulacionId = p.Id,
                CandidatoId = p.CandidatoId,
                UsuarioId = p.Candidato?.UsuarioId ?? string.Empty,
                NombreCompleto = $"{p.Candidato?.Nombres} {p.Candidato?.Apellidos}".Trim(),
                FotoUrl = p.Candidato?.FotoUrl,
                FechaPostulacion = p.FechaPostulacion,
                Estado = p.EstadoPostulacion?.Nombre ?? string.Empty,
                EstadoId = p.EstadoPostulacionId,
                CvUrl = p.Candidato?.CvArchivo?.RutaArchivo,
                Disponibilidad = p.Candidato?.Disponibilidad,
                PuntajeCompatibilidad = p.PuntajeCompatibilidad ?? 0m,
                EntrevistaProgramada = p.EntrevistaProgramada
            })
            .ToList();
    }

    public async Task CambiarEstadoAsync(int empresaId, int postulacionId, int nuevoEstadoId)
    {
        var postulacion = await _unitOfWork.Postulaciones.ObtenerConDetalleAsync(postulacionId)
            ?? throw new InvalidOperationException("No se encontró esta postulación.");

        if (postulacion.Vacante?.EmpresaId != empresaId)
        {
            throw new InvalidOperationException("Esta postulación no corresponde a una vacante de tu empresa.");
        }

        postulacion.EstadoPostulacionId = nuevoEstadoId;
        postulacion.FechaActualizacionEstado = DateTime.UtcNow;

        _unitOfWork.Postulaciones.Actualizar(postulacion);
        await _unitOfWork.GuardarCambiosAsync();

        var nuevoEstado = await _unitOfWork.EstadosPostulacion.ObtenerPorIdAsync(nuevoEstadoId);

        // Si se marca a este candidato como Contratado, la vacante se cierra
        // automáticamente (ya se cubrió el puesto) y deja de aceptar postulaciones.
        if (nuevoEstado?.Nombre == EstadoPostulacion.Nombres.Contratado && postulacion.Vacante is not null)
        {
            postulacion.Vacante.Estado = EstadosVacante.Cerrada;
            _unitOfWork.Vacantes.Actualizar(postulacion.Vacante);
            await _unitOfWork.GuardarCambiosAsync();
        }

        // 🔔 Notificación al candidato: cambio de estado de su postulación.
        if (!string.IsNullOrWhiteSpace(postulacion.Candidato?.UsuarioId))
        {
            var nombreVacante = postulacion.Vacante?.Titulo ?? "una vacante";

            await _notificacionService.CrearAsync(
                postulacion.Candidato!.UsuarioId,
                Notificacion.Tipos.CambioEstadoPostulacion,
                $"🔔 Tu postulación a \"{nombreVacante}\" cambió a: {nuevoEstado?.Nombre ?? "actualizado"}.",
                "/Candidato/MisPostulaciones");
        }
    }

    public async Task<List<EstadoPostulacionOptionDto>> ObtenerEstadosDisponiblesAsync()
    {
        var estados = await _unitOfWork.EstadosPostulacion.ObtenerTodosAsync();
        return estados
            .Select(e => new EstadoPostulacionOptionDto { Id = e.Id, Nombre = e.Nombre })
            .ToList();
    }

    public async Task<EmpresaDashboardResumenDto> ObtenerResumenDashboardAsync(int empresaId)
    {
        var postulaciones = (await _unitOfWork.Postulaciones.ObtenerPorEmpresaAsync(empresaId)).ToList();
        var vacantes = await _unitOfWork.Vacantes.ObtenerPorEmpresaAsync(empresaId);

        var hoy = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var haceSieteDias = hoy.AddDays(-7);

        return new EmpresaDashboardResumenDto
        {
            VacantesActivas = vacantes.Count(v => v.Estado == EstadosVacante.Activa),

            // Postulaciones recibidas en los últimos 7 días.
            CandidatosNuevos = postulaciones.Count(p => p.FechaPostulacion.Date >= haceSieteDias),

            // Entrevistas agendadas para el día de hoy.
            EntrevistasHoy = postulaciones.Count(p =>
                p.EntrevistaProgramada && p.FechaEntrevista.HasValue && p.FechaEntrevista.Value.Date == hoy),

            // CV ya revisado pero la prueba psicométrica todavía no tiene resultado.
            PruebasPendientes = postulaciones.Count(p =>
                p.CvRevisado && p.PruebaPsicometricaAprobada == null &&
                p.EstadoPostulacion?.Nombre != EstadoPostulacion.Nombres.Rechazado),

            // Postulaciones marcadas como "Contratado" con cambio de estado dentro del mes actual.
            ContratacionesDelMes = postulaciones.Count(p =>
                p.EstadoPostulacion?.Nombre == EstadoPostulacion.Nombres.Contratado &&
                p.FechaActualizacionEstado.HasValue && p.FechaActualizacionEstado.Value >= inicioMes)
        };
    }

    public async Task<List<CandidatoComparacionDto>> ObtenerComparacionAsync(int empresaId, int vacanteId, List<int> postulacionIds)
    {
        var vacante = await _unitOfWork.Vacantes.ObtenerConDetalleAsync(vacanteId);
        if (vacante is null || vacante.EmpresaId != empresaId)
        {
            throw new InvalidOperationException("Esta vacante no existe o no te pertenece.");
        }

        var postulaciones = (await _unitOfWork.Postulaciones.ObtenerPorVacanteAsync(vacanteId))
            .Where(p => postulacionIds.Contains(p.Id))
            .ToList();

        var resultado = new List<CandidatoComparacionDto>();

        foreach (var p in postulaciones)
        {
            if (p.Candidato is null) continue;

            var puntaje = _compatibilidadService.Calcular(p.Candidato, vacante);

            // Mejor idioma: el que tenga el nivel MCER más alto entre los que domina el candidato.
            var mejorIdioma = p.Candidato.Idiomas
                .OrderByDescending(ci => CompatibilidadService.NivelIdiomaAValor(ci.Nivel))
                .FirstOrDefault();

            // Puntaje de video CV: promedio de las calificaciones (1-5 estrellas) que
            // los reclutadores dejaron en los comentarios de esta postulación, sobre 100.
            var calificacionesVideo = p.ComentariosVideoCv
                .Where(c => c.Calificacion.HasValue)
                .Select(c => c.Calificacion!.Value)
                .ToList();

            resultado.Add(new CandidatoComparacionDto
            {
                PostulacionId = p.Id,
                CandidatoId = p.CandidatoId,
                NombreCompleto = $"{p.Candidato.Nombres} {p.Candidato.Apellidos}".Trim(),
                FotoUrl = p.Candidato.FotoUrl,
                AniosExperiencia = CompatibilidadService.CalcularAniosExperiencia(p.Candidato),
                PruebaPsicometricaPuntaje = p.PruebaPsicometricaPuntaje,
                VideoCvPuntaje = calificacionesVideo.Count > 0 ? Math.Round((decimal)calificacionesVideo.Average() * 20m, 1) : null,
                IdiomaPrincipal = mejorIdioma?.Idioma?.Nombre,
                NivelIdiomaPrincipal = mejorIdioma?.Nivel,
                PuntajeCompatibilidad = puntaje
            });
        }

        return resultado.OrderByDescending(r => r.PuntajeCompatibilidad).ToList();
    }

    public async Task<List<ComentarioVideoCvDto>> ObtenerComentariosVideoCvAsync(int empresaId, int postulacionId)
    {
        var postulacion = await _unitOfWork.Postulaciones.ObtenerConDetalleAsync(postulacionId)
            ?? throw new InvalidOperationException("No se encontró esta postulación.");

        if (postulacion.Vacante?.EmpresaId != empresaId)
        {
            throw new InvalidOperationException("Esta postulación no corresponde a una vacante de tu empresa.");
        }

        return postulacion.ComentariosVideoCv
            .OrderByDescending(c => c.FechaCreacion)
            .Select(c => new ComentarioVideoCvDto
            {
                Id = c.Id,
                ReclutadorNombre = c.ReclutadorNombre,
                Comentario = c.Comentario,
                Calificacion = c.Calificacion,
                FechaCreacion = c.FechaCreacion
            })
            .ToList();
    }

    public async Task<string?> MarcarCvRevisadoYObtenerUrlAsync(int empresaId, int postulacionId)
    {
        var postulacion = await _unitOfWork.Postulaciones.ObtenerConDetalleAsync(postulacionId)
            ?? throw new InvalidOperationException("No se encontró esta postulación.");

        if (postulacion.Vacante?.EmpresaId != empresaId)
        {
            throw new InvalidOperationException("Esta postulación no corresponde a una vacante de tu empresa.");
        }

        if (!postulacion.CvRevisado)
        {
            postulacion.CvRevisado = true;
            postulacion.FechaCvRevisado = DateTime.UtcNow;
            _unitOfWork.Postulaciones.Actualizar(postulacion);
            await _unitOfWork.GuardarCambiosAsync();
        }

        return postulacion.Candidato?.CvArchivo?.RutaArchivo;
    }

    public async Task<DetalleVideoCvDto> ObtenerDetalleVideoCvAsync(int empresaId, int postulacionId)
    {
        var postulacion = await _unitOfWork.Postulaciones.ObtenerConDetalleAsync(postulacionId)
            ?? throw new InvalidOperationException("No se encontró esta postulación.");

        if (postulacion.Vacante?.EmpresaId != empresaId)
        {
            throw new InvalidOperationException("Esta postulación no corresponde a una vacante de tu empresa.");
        }

        if (postulacion.Candidato is null)
        {
            throw new InvalidOperationException("No se encontró el candidato de esta postulación.");
        }

        if (!postulacion.VideoCvVisto)
        {
            postulacion.VideoCvVisto = true;
            postulacion.FechaVideoCvVisto = DateTime.UtcNow;
            _unitOfWork.Postulaciones.Actualizar(postulacion);
            await _unitOfWork.GuardarCambiosAsync();
        }

        return new DetalleVideoCvDto
        {
            PostulacionId = postulacion.Id,
            CandidatoId = postulacion.CandidatoId,
            NombreCompleto = $"{postulacion.Candidato.Nombres} {postulacion.Candidato.Apellidos}".Trim(),
            FotoUrl = postulacion.Candidato.FotoUrl,
            VideoCvUrl = postulacion.Candidato.VideoCvArchivo?.RutaArchivo,
            VacanteTitulo = postulacion.Vacante?.Titulo ?? string.Empty
        };
    }

    public async Task ComentarVideoCvAsync(int empresaId, int postulacionId, string reclutadorUsuarioId, string reclutadorNombre, string comentario, int? calificacion)
    {
        var postulacion = await _unitOfWork.Postulaciones.ObtenerConDetalleAsync(postulacionId)
            ?? throw new InvalidOperationException("No se encontró esta postulación.");

        if (postulacion.Vacante?.EmpresaId != empresaId)
        {
            throw new InvalidOperationException("Esta postulación no corresponde a una vacante de tu empresa.");
        }

        var nuevoComentario = new ComentarioVideoCv
        {
            PostulacionId = postulacionId,
            ReclutadorUsuarioId = reclutadorUsuarioId,
            ReclutadorNombre = reclutadorNombre,
            Comentario = comentario,
            Calificacion = calificacion,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.ComentariosVideoCv.AgregarAsync(nuevoComentario);

        if (!postulacion.VideoCvVisto)
        {
            postulacion.VideoCvVisto = true;
            postulacion.FechaVideoCvVisto = DateTime.UtcNow;
            _unitOfWork.Postulaciones.Actualizar(postulacion);
        }

        await _unitOfWork.GuardarCambiosAsync();

        // 🔔 Notificar al candidato que su video CV recibió un comentario.
        if (!string.IsNullOrWhiteSpace(postulacion.Candidato?.UsuarioId))
        {
            await _notificacionService.CrearAsync(
                postulacion.Candidato!.UsuarioId,
                Notificacion.Tipos.ComentarioVideoCv,
                $"🔔 Un reclutador comentó tu Video Currículum para \"{postulacion.Vacante?.Titulo}\".",
                "/Candidato/MisPostulaciones");
        }
    }

    // ---------- Video entrevista (Jitsi Meet) ----------

    public async Task<string> ObtenerSalaVideollamadaEmpresaAsync(int empresaId, int postulacionId)
    {
        var postulacion = await _unitOfWork.Postulaciones.ObtenerConDetalleAsync(postulacionId)
            ?? throw new InvalidOperationException("No se encontró esta postulación.");

        if (postulacion.Vacante?.EmpresaId != empresaId)
        {
            throw new InvalidOperationException("Esta postulación no corresponde a una vacante de tu empresa.");
        }

        return await AsegurarSalaVideollamadaAsync(postulacion);
    }

    public async Task<string> ObtenerSalaVideollamadaCandidatoAsync(int candidatoId, int postulacionId)
    {
        var postulacion = await _unitOfWork.Postulaciones.ObtenerConDetalleAsync(postulacionId)
            ?? throw new InvalidOperationException("No se encontró esta postulación.");

        if (postulacion.CandidatoId != candidatoId)
        {
            throw new InvalidOperationException("Esta postulación no te pertenece.");
        }

        return await AsegurarSalaVideollamadaAsync(postulacion);
    }

    /// <summary>
    /// Genera el identificador de la sala la primera vez que alguna de las
    /// partes entra, y lo reutiliza después — así ambos lados (empresa y
    /// candidato) siempre terminan en la misma videollamada.
    /// </summary>
    private async Task<string> AsegurarSalaVideollamadaAsync(Postulacion postulacion)
    {
        if (!string.IsNullOrEmpty(postulacion.SalaVideollamadaId))
        {
            return postulacion.SalaVideollamadaId;
        }

        // Prefijo fijo + GUID sin guiones, para que la sala en Jitsi no sea adivinable por terceros.
        var sala = $"ChambeaJobs-{Guid.NewGuid():N}";
        postulacion.SalaVideollamadaId = sala;
        _unitOfWork.Postulaciones.Actualizar(postulacion);
        await _unitOfWork.GuardarCambiosAsync();

        return sala;
    }

    public async Task ResponderPruebaPsicometricaAsync(int candidatoId, int postulacionId, int puntaje)
    {
        var postulacion = await _unitOfWork.Postulaciones.ObtenerConDetalleAsync(postulacionId)
            ?? throw new InvalidOperationException("No se encontró esta postulación.");

        if (postulacion.CandidatoId != candidatoId)
        {
            throw new InvalidOperationException("Esta postulación no te pertenece.");
        }

        puntaje = Math.Clamp(puntaje, 0, 100);

        postulacion.PruebaPsicometricaPuntaje = puntaje;
        postulacion.PruebaPsicometricaAprobada = puntaje >= 60; // umbral de aprobación estándar
        postulacion.FechaPruebaPsicometrica = DateTime.UtcNow;

        _unitOfWork.Postulaciones.Actualizar(postulacion);
        await _unitOfWork.GuardarCambiosAsync();

        // 🔔 Notificar al reclutador: prueba respondida.
        if (!string.IsNullOrWhiteSpace(postulacion.Vacante?.Empresa?.UsuarioId))
        {
            var nombreCandidato = postulacion.Candidato is not null
                ? $"{postulacion.Candidato.Nombres} {postulacion.Candidato.Apellidos}".Trim()
                : "Un candidato";

            await _notificacionService.CrearAsync(
                postulacion.Vacante!.Empresa!.UsuarioId,
                Notificacion.Tipos.PruebaRespondida,
                $"🔔 {nombreCandidato} respondió la prueba psicométrica ({puntaje}/100).",
                $"/Empresa/CandidatosPostulados?vacanteId={postulacion.VacanteId}");
        }
    }

    public async Task ProgramarEntrevistaAsync(int empresaId, int postulacionId, DateTime fechaEntrevista, string? nota)
    {
        var postulacion = await _unitOfWork.Postulaciones.ObtenerConDetalleAsync(postulacionId)
            ?? throw new InvalidOperationException("No se encontró esta postulación.");

        if (postulacion.Vacante?.EmpresaId != empresaId)
        {
            throw new InvalidOperationException("Esta postulación no corresponde a una vacante de tu empresa.");
        }

        postulacion.EntrevistaProgramada = true;
        postulacion.FechaEntrevista = fechaEntrevista;
        postulacion.NotaEntrevista = nota;
        postulacion.RecordatorioEntrevistaEnviado = false; // por si se reprograma, vuelve a habilitar el recordatorio

        _unitOfWork.Postulaciones.Actualizar(postulacion);
        await _unitOfWork.GuardarCambiosAsync();

        // 🔔 Notificar al candidato de inmediato (el recordatorio "Entrevista mañana" lo envía el job diario).
        if (!string.IsNullOrWhiteSpace(postulacion.Candidato?.UsuarioId))
        {
            await _notificacionService.CrearAsync(
                postulacion.Candidato!.UsuarioId,
                Notificacion.Tipos.EntrevistaProxima,
                $"🔔 Se programó tu entrevista para \"{postulacion.Vacante?.Titulo}\" el {fechaEntrevista:dd/MM/yyyy HH:mm}.",
                "/Candidato/MisPostulaciones");
        }
    }
}
