using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Calcula un puntaje de compatibilidad (0-100) entre un candidato y una
/// vacante, usado para el ranking automático de postulantes (RRHH) y para
/// la comparación de candidatos. Ver <c>CompatibilidadService</c> para el
/// detalle de la ponderación.
/// </summary>
public interface ICompatibilidadService
{
    /// <summary>
    /// Requiere que candidato.Habilidades, candidato.Idiomas y
    /// candidato.Experiencias vengan cargados (Include) para un cálculo preciso.
    /// </summary>
    decimal Calcular(Candidato candidato, Vacante vacante);
}
