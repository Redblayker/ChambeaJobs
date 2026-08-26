using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="IEvaluacionService"/>
public class EvaluacionService : IEvaluacionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostulacionService _postulacionService;
    private readonly INotificacionService _notificacionService;

    public EvaluacionService(IUnitOfWork unitOfWork, IPostulacionService postulacionService, INotificacionService notificacionService)
    {
        _unitOfWork = unitOfWork;
        _postulacionService = postulacionService;
        _notificacionService = notificacionService;
    }

    public async Task EnviarEvaluacionAsync(EnviarEvaluacionDto datos)
    {
        if (await _unitOfWork.Evaluaciones.ExisteParaPostulacionAsync(datos.PostulacionId))
        {
            throw new InvalidOperationException("Ya se envió una evaluación a este candidato para esta postulación.");
        }

        if (datos.FechaLimite.Date <= DateTime.UtcNow.Date)
        {
            throw new ArgumentException("La fecha límite debe ser posterior a hoy.");
        }

        var postulacion = await _unitOfWork.Postulaciones.ObtenerConDetalleAsync(datos.PostulacionId)
            ?? throw new InvalidOperationException("No se encontró esta postulación.");

        var empresaId = postulacion.Vacante?.EmpresaId
            ?? throw new InvalidOperationException("No se encontró la empresa de esta vacante.");

        var paqueteVigente = await _unitOfWork.PaquetesEmpresa.ObtenerVigentePorEmpresaAsync(empresaId);
        var incluyePrueba = paqueteVigente?.PlanSuscripcion?.IncluyePruebaPsicometrica ?? false;

        if (!incluyePrueba)
        {
            throw new InvalidOperationException(
                "La evaluación psicométrica solo está disponible en el Plan Empresarial. Actualiza tu plan desde \"Comprar paquete\" para usar esta función.");
        }

        var evaluacion = new EvaluacionPsicometrica
        {
            PostulacionId = datos.PostulacionId,
            FechaEnvio = DateTime.UtcNow,
            FechaLimite = datos.FechaLimite,
            Mensaje = datos.Mensaje,
            Estado = EstadosEvaluacion.Pendiente
        };

        await _unitOfWork.Evaluaciones.AgregarAsync(evaluacion);
        await _unitOfWork.GuardarCambiosAsync();

        // 🔔 Notificar al candidato: tiene una evaluación pendiente con fecha
        // límite — sin esto, podría perder la oportunidad sin enterarse.
        var usuarioIdCandidato = postulacion.Candidato?.UsuarioId;

        if (!string.IsNullOrWhiteSpace(usuarioIdCandidato))
        {
            var nombreVacante = postulacion.Vacante?.Titulo ?? "una vacante";
            var nombreEmpresa = postulacion.Vacante?.Empresa?.NombreEmpresa ?? "La empresa";

            await _notificacionService.CrearAsync(
                usuarioIdCandidato!,
                Notificacion.Tipos.EvaluacionPsicometricaRecibida,
                $"📋 {nombreEmpresa} te envió una evaluación de personalidad para \"{nombreVacante}\". Tienes hasta el {datos.FechaLimite:dd/MM/yyyy} para completarla.",
                $"/Evaluacion/Invitacion/{evaluacion.Id}");
        }
    }

    public async Task<Dictionary<int, EvaluacionResumenDto>> ObtenerResumenPorPostulacionesAsync(IEnumerable<int> postulacionIds)
    {
        var resultado = new Dictionary<int, EvaluacionResumenDto>();

        foreach (var postulacionId in postulacionIds)
        {
            var evaluacion = await _unitOfWork.Evaluaciones.ObtenerPorPostulacionAsync(postulacionId);

            resultado[postulacionId] = evaluacion is null
                ? new EvaluacionResumenDto { Estado = "Sin enviar" }
                : new EvaluacionResumenDto
                {
                    EvaluacionId = evaluacion.Id,
                    Estado = DeterminarEstadoVisible(evaluacion),
                    PuntajeCompatibilidad = evaluacion.PuntajeCompatibilidad,
                    FechaLimite = evaluacion.FechaLimite
                };
        }

        return resultado;

    }

    public async Task<Dictionary<int, EvaluacionResumenDto>> ObtenerPendientesDeCandidatoAsync(int candidatoId, IEnumerable<int> postulacionIds) =>
        await ObtenerResumenPorPostulacionesAsync(postulacionIds); // misma lógica, el filtrado por candidato ya viene dado por las postulaciones que se le pasan

    public async Task<InvitacionEvaluacionDto?> ObtenerInvitacionAsync(int evaluacionId, int candidatoId)
    {
        var evaluacion = await _unitOfWork.Evaluaciones.ObtenerConDetalleAsync(evaluacionId);
        if (evaluacion is null || evaluacion.Postulacion?.CandidatoId != candidatoId)
        {
            return null;
        }

        var totalPreguntas = (await _unitOfWork.PreguntasPsicometricas.ObtenerTodosAsync()).Count();

        return new InvitacionEvaluacionDto
        {
            EvaluacionId = evaluacion.Id,
            EmpresaNombre = evaluacion.Postulacion.Vacante?.Empresa?.NombreEmpresa ?? string.Empty,
            VacanteTitulo = evaluacion.Postulacion.Vacante?.Titulo ?? string.Empty,
            Mensaje = evaluacion.Mensaje,
            FechaLimite = evaluacion.FechaLimite,
            TotalPreguntas = totalPreguntas
        };
    }

    public async Task<RealizarEvaluacionDto?> ObtenerFormularioAsync(int evaluacionId, int candidatoId)
    {
        var evaluacion = await _unitOfWork.Evaluaciones.ObtenerConDetalleAsync(evaluacionId);
        if (evaluacion is null || evaluacion.Postulacion?.CandidatoId != candidatoId)
        {
            return null;
        }

        var preguntas = (await _unitOfWork.PreguntasPsicometricas.ObtenerTodosAsync())
            .OrderBy(p => p.Orden)
            .ToList();

        var respuestasGuardadas = evaluacion.Respuestas.ToDictionary(r => r.PreguntaId, r => r.Valor);

        return new RealizarEvaluacionDto
        {
            EvaluacionId = evaluacion.Id,
            VacanteTitulo = evaluacion.Postulacion.Vacante?.Titulo ?? string.Empty,
            EmpresaNombre = evaluacion.Postulacion.Vacante?.Empresa?.NombreEmpresa ?? string.Empty,
            Preguntas = preguntas.Select(p => new PreguntaRespuestaDto
            {
                PreguntaId = p.Id,
                Texto = p.Texto,
                Orden = p.Orden,
                ValorSeleccionado = respuestasGuardadas.TryGetValue(p.Id, out var valor) ? valor : null
            }).ToList()
        };
    }

    public async Task GuardarRespuestasParcialesAsync(int evaluacionId, int candidatoId, Dictionary<int, int> respuestas)
    {
        var evaluacion = await _unitOfWork.Evaluaciones.ObtenerConDetalleAsync(evaluacionId);
        if (evaluacion is null || evaluacion.Postulacion?.CandidatoId != candidatoId)
        {
            throw new InvalidOperationException("Esta evaluación no existe o no te pertenece.");
        }

        await GuardarRespuestasInternoAsync(evaluacion, respuestas);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task FinalizarEvaluacionAsync(int evaluacionId, int candidatoId, Dictionary<int, int> respuestas)
    {
        var evaluacion = await _unitOfWork.Evaluaciones.ObtenerConDetalleAsync(evaluacionId);
        if (evaluacion is null || evaluacion.Postulacion?.CandidatoId != candidatoId)
        {
            throw new InvalidOperationException("Esta evaluación no existe o no te pertenece.");
        }

        if (!evaluacion.VigenteParaResponder())
        {
            throw new InvalidOperationException("Esta evaluación ya fue completada o venció su fecha límite.");
        }

        await GuardarRespuestasInternoAsync(evaluacion, respuestas);

        var todasLasPreguntas = await _unitOfWork.PreguntasPsicometricas.ObtenerTodosAsync();
        if (evaluacion.Respuestas.Count < todasLasPreguntas.Count())
        {
            throw new InvalidOperationException("Debes responder todas las preguntas antes de finalizar.");
        }

        CalcularPuntajes(evaluacion, todasLasPreguntas);

        evaluacion.Estado = EstadosEvaluacion.Completada;
        evaluacion.FechaCompletado = DateTime.UtcNow;

        _unitOfWork.Evaluaciones.Actualizar(evaluacion);
        await _unitOfWork.GuardarCambiosAsync();

        // Sincroniza el puntaje REAL de compatibilidad Big Five con los campos
        // PruebaPsicometricaPuntaje/Aprobada de Postulacion — así el Dashboard
        // y el Comparador de candidatos (que ya leen esos campos) muestran el
        // resultado real automáticamente, sin necesitar cambios en esas vistas.
        // También reutiliza la notificación al reclutador ya implementada ahí.
        await _postulacionService.ResponderPruebaPsicometricaAsync(
            candidatoId, evaluacion.PostulacionId, evaluacion.PuntajeCompatibilidad!.Value);
    }

    public async Task<ResultadoEvaluacionDto?> ObtenerResultadoAsync(int evaluacionId)
    {
        var evaluacion = await _unitOfWork.Evaluaciones.ObtenerConDetalleAsync(evaluacionId);
        if (evaluacion is null || evaluacion.Estado != EstadosEvaluacion.Completada)
        {
            return null;
        }

        var candidato = evaluacion.Postulacion?.Candidato;

        return new ResultadoEvaluacionDto
        {
            EvaluacionId = evaluacion.Id,
            CandidatoNombre = $"{candidato?.Nombres} {candidato?.Apellidos}".Trim(),
            VacanteTitulo = evaluacion.Postulacion?.Vacante?.Titulo ?? string.Empty,
            EmpresaNombre = evaluacion.Postulacion?.Vacante?.Empresa?.NombreEmpresa ?? string.Empty,
            FechaCompletado = evaluacion.FechaCompletado ?? evaluacion.FechaEnvio,
            PuntajeCompatibilidad = evaluacion.PuntajeCompatibilidad ?? 0,
            Rasgos = new List<RasgoResultadoDto>
            {
                CrearRasgoResultado(RasgosBigFive.Responsabilidad, evaluacion.PuntajeResponsabilidad ?? 0),
                CrearRasgoResultado(RasgosBigFive.Extraversion, evaluacion.PuntajeExtraversion ?? 0),
                CrearRasgoResultado(RasgosBigFive.Amabilidad, evaluacion.PuntajeAmabilidad ?? 0),
                CrearRasgoResultado(RasgosBigFive.Apertura, evaluacion.PuntajeApertura ?? 0),
                CrearRasgoResultado(RasgosBigFive.EstabilidadEmocional, evaluacion.PuntajeEstabilidadEmocional ?? 0)
            }
        };
    }

    // ---------- Auxiliares privados ----------

    private async Task GuardarRespuestasInternoAsync(EvaluacionPsicometrica evaluacion, Dictionary<int, int> respuestas)
    {
        foreach (var (preguntaId, valor) in respuestas)
        {
            if (valor < 1 || valor > 5) continue; // ignora valores fuera de la escala Likert 1-5

            var existente = evaluacion.Respuestas.FirstOrDefault(r => r.PreguntaId == preguntaId);
            if (existente is not null)
            {
                existente.Valor = valor;
            }
            else
            {
                var nuevaRespuesta = new RespuestaPsicometrica
                {
                    EvaluacionId = evaluacion.Id,
                    PreguntaId = preguntaId,
                    Valor = valor
                };
                evaluacion.Respuestas.Add(nuevaRespuesta);
                await _unitOfWork.RespuestasPsicometricas.AgregarAsync(nuevaRespuesta);
            }
        }
    }

    /// <summary>
    /// Calcula el puntaje 0-100 de cada uno de los 5 rasgos (suma de sus 5
    /// preguntas, invirtiendo las que están redactadas en sentido contrario,
    /// y normalizando el rango 5-25 a 0-100), y una compatibilidad general
    /// ponderada. La ponderación de compatibilidad es un criterio general
    /// (no específico por vacante) que prioriza Responsabilidad y Estabilidad
    /// Emocional, rasgos con mayor correlación histórica con desempeño laboral.
    /// </summary>
    private static void CalcularPuntajes(EvaluacionPsicometrica evaluacion, IEnumerable<PreguntaPsicometrica> todasLasPreguntas)
    {
        int PuntajeDelRasgo(string rasgo)
        {
            var preguntasDelRasgo = todasLasPreguntas.Where(p => p.Rasgo == rasgo).ToList();
            var sumaCruda = 0;

            foreach (var pregunta in preguntasDelRasgo)
            {
                var respuesta = evaluacion.Respuestas.First(r => r.PreguntaId == pregunta.Id);
                sumaCruda += pregunta.EsInversa ? (6 - respuesta.Valor) : respuesta.Valor;
            }

            // Rango crudo posible: 5 (mínimo) a 25 (máximo) con 5 preguntas por rasgo.
            return (int)Math.Round((sumaCruda - 5) / 20.0 * 100);
        }

        evaluacion.PuntajeResponsabilidad = PuntajeDelRasgo(RasgosBigFive.Responsabilidad);
        evaluacion.PuntajeExtraversion = PuntajeDelRasgo(RasgosBigFive.Extraversion);
        evaluacion.PuntajeAmabilidad = PuntajeDelRasgo(RasgosBigFive.Amabilidad);
        evaluacion.PuntajeApertura = PuntajeDelRasgo(RasgosBigFive.Apertura);
        evaluacion.PuntajeEstabilidadEmocional = PuntajeDelRasgo(RasgosBigFive.EstabilidadEmocional);

        evaluacion.PuntajeCompatibilidad = (int)Math.Round(
            evaluacion.PuntajeResponsabilidad.Value * 0.30 +
            evaluacion.PuntajeEstabilidadEmocional.Value * 0.25 +
            evaluacion.PuntajeAmabilidad.Value * 0.20 +
            evaluacion.PuntajeApertura.Value * 0.15 +
            evaluacion.PuntajeExtraversion.Value * 0.10);
    }

    private static string DeterminarEstadoVisible(EvaluacionPsicometrica evaluacion)
    {
        if (evaluacion.Estado == EstadosEvaluacion.Completada) return "Completada";
        if (DateTime.UtcNow > evaluacion.FechaLimite) return "Vencida";
        return "Pendiente";
    }

    private static RasgoResultadoDto CrearRasgoResultado(string nombre, int puntaje) => new()
    {
        Nombre = nombre,
        Puntaje = puntaje,
        Interpretacion = puntaje switch
        {
            >= 70 => "Nivel alto",
            >= 40 => "Nivel medio",
            _ => "Nivel bajo"
        }
    };
}
