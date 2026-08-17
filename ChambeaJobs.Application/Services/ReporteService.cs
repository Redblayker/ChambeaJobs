using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="IReporteService"/>
public class ReporteService : IReporteService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReporteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReporteDashboardDto> ObtenerDashboardAsync()
    {
        var candidatos = await _unitOfWork.Candidatos.ObtenerTodosAsync();
        var empresas = await _unitOfWork.Empresas.ObtenerTodosAsync();
        var vacantes = await _unitOfWork.Vacantes.ObtenerTodosAsync();
        var pagos = await _unitOfWork.Pagos.ObtenerTodosAsync();
        var categorias = await _unitOfWork.Categorias.ObtenerTodosAsync();
        var postulaciones = await _unitOfWork.Postulaciones.ObtenerTodosAsync();

        var ingresos = pagos.Where(p => p.EstadoPago == EstadosPago.Aprobado).Sum(p => p.Monto);

        var hoy = DateTime.UtcNow.Date;
        var pagosAprobados = pagos.Where(p => p.EstadoPago == EstadosPago.Aprobado).ToList();

        decimal IngresosDesde(DateTime desde) =>
            pagosAprobados
                .Where(p => (p.FechaRevision ?? p.FechaPago).Date >= desde)
                .Sum(p => p.Monto);

        var ingresosSemana = IngresosDesde(hoy.AddDays(-6));
        var ingresosQuincena = IngresosDesde(hoy.AddDays(-14));
        var ingresosMes = IngresosDesde(new DateTime(hoy.Year, hoy.Month, 1));

        var postulacionesPorMes = postulaciones
            .GroupBy(p => p.FechaPostulacion.ToString("MMM yyyy"))
            .Select(g => new ReporteBarraDto { Etiqueta = g.Key, Valor = g.Count() })
            .ToList();

        var vacantesPorCategoria = categorias
            .Select(c => new ReporteBarraDto
            {
                Etiqueta = c.Nombre,
                Valor = vacantes.Count(v => v.CategoriaId == c.Id)
            })
            .ToList();

        return new ReporteDashboardDto
        {
            TotalCandidatos = candidatos.Count(),
            TotalEmpresas = empresas.Count(),
            VacantesActivas = vacantes.Count(v => v.Estado == EstadosVacante.Activa),
            IngresosTotales = ingresos,
            IngresosSemana = ingresosSemana,
            IngresosQuincena = ingresosQuincena,
            IngresosMes = ingresosMes,
            PostulacionesPorMes = postulacionesPorMes,
            VacantesPorCategoria = vacantesPorCategoria
        };
    }
}
