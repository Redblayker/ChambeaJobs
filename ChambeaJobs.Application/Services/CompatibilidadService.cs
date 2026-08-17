using System.Text.RegularExpressions;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <summary>
/// Motor de compatibilidad candidato-vacante (ranking automático solicitado
/// por RRHH: "ordenar por compatibilidad en estrellas y no por fecha").
///
/// Como las vacantes de ChambeaJobs describen los requisitos como texto
/// libre (campo Vacante.Requisitos) y no como una lista estructurada de
/// habilidades, el puntaje se calcula con una heurística de coincidencia
/// de texto + señales objetivas del perfil. Ponderación (100 pts totales):
///
///   - Habilidades mencionadas en el requisito/descripción: 40 pts
///   - Idiomas requeridos y su nivel:                        15 pts
///   - Años de experiencia vs. lo requerido (o vs. 5 años):   25 pts
///   - Perfil completo + Video CV disponible:                 20 pts
///
/// Nota para una futura mejora: si se agrega una lista estructurada de
/// habilidades requeridas a Vacante, el componente de habilidades puede
/// calcularse por coincidencia exacta en vez de texto libre.
/// </summary>
public class CompatibilidadService : ICompatibilidadService
{
    private static readonly Dictionary<string, int> NivelesIdioma = new(StringComparer.OrdinalIgnoreCase)
    {
        ["A1"] = 1, ["A2"] = 2, ["B1"] = 3, ["B2"] = 4, ["C1"] = 5, ["C2"] = 6, ["Nativo"] = 6
    };

    /// <summary>Convierte un nivel MCER (A1-C2, Nativo) a un valor numérico ordenable (1-6).</summary>
    public static int NivelIdiomaAValor(string? nivel) =>
        nivel is not null && NivelesIdioma.TryGetValue(nivel, out var valor) ? valor : 0;

    public decimal Calcular(Candidato candidato, Vacante vacante)
    {
        var textoVacante = $"{vacante.Requisitos} {vacante.Descripcion} {vacante.Titulo}".ToLowerInvariant();

        var puntajeHabilidades = CalcularPuntajeHabilidades(candidato, textoVacante);
        var puntajeIdiomas = CalcularPuntajeIdiomas(candidato, textoVacante);
        var puntajeExperiencia = CalcularPuntajeExperiencia(candidato, textoVacante);
        var puntajePerfil = CalcularPuntajePerfil(candidato);

        var total = puntajeHabilidades + puntajeIdiomas + puntajeExperiencia + puntajePerfil;
        return Math.Clamp(Math.Round(total, 2), 0m, 100m);
    }

    /// <summary>40 pts: proporción de las habilidades del candidato que aparecen mencionadas en la vacante.</summary>
    private static decimal CalcularPuntajeHabilidades(Candidato candidato, string textoVacante)
    {
        const decimal pesoMaximo = 40m;
        var habilidades = candidato.Habilidades
            .Where(ch => !string.IsNullOrWhiteSpace(ch.Habilidad?.Nombre))
            .Select(ch => ch.Habilidad!.Nombre)
            .ToList();

        if (habilidades.Count == 0) return 0m;

        var coincidencias = habilidades.Count(h => textoVacante.Contains(h.ToLowerInvariant()));
        var ratio = (decimal)coincidencias / habilidades.Count;
        return Math.Round(ratio * pesoMaximo, 2);
    }

    /// <summary>15 pts: si la vacante menciona un idioma que el candidato domina, pondera según su nivel MCER.</summary>
    private static decimal CalcularPuntajeIdiomas(Candidato candidato, string textoVacante)
    {
        const decimal pesoMaximo = 15m;
        if (candidato.Idiomas.Count == 0) return 0m;

        var mejorNivel = 0;
        foreach (var ci in candidato.Idiomas)
        {
            var nombreIdioma = ci.Idioma?.Nombre;
            if (string.IsNullOrWhiteSpace(nombreIdioma)) continue;
            if (!textoVacante.Contains(nombreIdioma.ToLowerInvariant())) continue;

            var nivel = NivelesIdioma.GetValueOrDefault(ci.Nivel, 3);
            if (nivel > mejorNivel) mejorNivel = nivel;
        }

        if (mejorNivel == 0) return 0m; // ningún idioma del candidato es relevante para esta vacante
        return Math.Round((decimal)mejorNivel / 6m * pesoMaximo, 2);
    }

    /// <summary>25 pts: años de experiencia del candidato vs. lo requerido (detectado en el texto) o vs. 5 años por defecto.</summary>
    private static decimal CalcularPuntajeExperiencia(Candidato candidato, string textoVacante)
    {
        const decimal pesoMaximo = 25m;

        var aniosCandidato = CalcularAniosExperiencia(candidato);
        var aniosRequeridos = ExtraerAniosRequeridos(textoVacante) ?? 5m;

        if (aniosRequeridos <= 0) aniosRequeridos = 5m;
        var ratio = Math.Min(1m, aniosCandidato / aniosRequeridos);
        return Math.Round(ratio * pesoMaximo, 2);
    }

    /// <summary>20 pts: 10 por completitud del perfil, 10 por tener video CV disponible.</summary>
    private static decimal CalcularPuntajePerfil(Candidato candidato)
    {
        var puntajeCompletitud = candidato.CalcularPorcentajeCompletitud() / 100m * 10m;
        var puntajeVideo = !string.IsNullOrWhiteSpace(candidato.VideoCvUrl) || candidato.VideoCvArchivoId.HasValue ? 10m : 0m;
        return Math.Round(puntajeCompletitud + puntajeVideo, 2);
    }

    /// <summary>Suma los años (con fracción) de todas las experiencias laborales del candidato; los trabajos actuales cuentan hasta hoy.</summary>
    public static decimal CalcularAniosExperiencia(Candidato candidato)
    {
        if (candidato.Experiencias.Count == 0) return 0m;

        var totalDias = candidato.Experiencias.Sum(e =>
        {
            var fin = e.FechaFin ?? DateTime.UtcNow;
            var dias = (fin - e.FechaInicio).TotalDays;
            return dias > 0 ? dias : 0;
        });

        return Math.Round((decimal)(totalDias / 365.25), 1);
    }

    /// <summary>Busca patrones como "3 años", "5+ años de experiencia" en el texto de la vacante.</summary>
    private static decimal? ExtraerAniosRequeridos(string textoVacante)
    {
        var match = Regex.Match(textoVacante, @"(\d+)\s*\+?\s*(años|año|years|year)");
        if (match.Success && decimal.TryParse(match.Groups[1].Value, out var anios))
        {
            return anios;
        }
        return null;
    }
}
