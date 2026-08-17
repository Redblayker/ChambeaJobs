using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ChambeaJobs.Infrastructure.Services.Finanzas;

public class FinanzasDashboardService : IFinanzasDashboardService
{
    private readonly ApplicationDbContext _db;

    public FinanzasDashboardService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<FinanzasDashboardDto> ObtenerDashboardAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        var hoy = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var inicioAnio = new DateTime(hoy.Year, 1, 1);

        async Task<decimal> SumaIngresos(DateTime desde2, DateTime hasta2) =>
            await _db.IngresosFinancieros.Where(i => i.Estado == EstadoMovimientoFinanciero.Activo && i.FechaIngreso >= desde2 && i.FechaIngreso <= hasta2)
                .SumAsync(i => (decimal?)i.Monto) ?? 0m;

        async Task<decimal> SumaGastos(DateTime desde2, DateTime hasta2) =>
            await _db.GastosFinancieros.Where(g => g.Estado == EstadoMovimientoFinanciero.Activo && g.Fecha >= desde2 && g.Fecha <= hasta2)
                .SumAsync(g => (decimal?)g.Monto) ?? 0m;

        var ingresosDia = await SumaIngresos(hoy, hoy.AddDays(1).AddSeconds(-1));
        var ingresosMes = await SumaIngresos(inicioMes, hoy.AddDays(1).AddSeconds(-1));
        var ingresosAnio = await SumaIngresos(inicioAnio, hoy.AddDays(1).AddSeconds(-1));
        var gastosMes = await SumaGastos(inicioMes, hoy.AddDays(1).AddSeconds(-1));
        var gastosAnio = await SumaGastos(inicioAnio, hoy.AddDays(1).AddSeconds(-1));

        var pagosAprobados = await _db.Pagos.CountAsync(p => p.EstadoPago == EstadosPago.Aprobado);
        var pagosPendientes = await _db.Pagos.CountAsync(p => p.EstadoPago == EstadosPago.Pendiente);
        var pagosRechazados = await _db.Pagos.CountAsync(p => p.EstadoPago == EstadosPago.Rechazado);
        var cuentasPorCobrar = await _db.Pagos.Where(p => p.EstadoPago == EstadosPago.Pendiente).SumAsync(p => (decimal?)p.Monto) ?? 0m;

        var empresasClientes = await _db.PaquetesEmpresa.Select(p => p.EmpresaId).Distinct().CountAsync();
        var renovacionesDelMes = await _db.PaquetesEmpresa.CountAsync(p => !p.EsPruebaGratis && p.FechaCompra >= inicioMes);

        var ingresosPorPlan = await _db.IngresosFinancieros
            .Where(i => i.Estado == EstadoMovimientoFinanciero.Activo && i.FechaIngreso >= inicioMes)
            .Include(i => i.PlanSuscripcion)
            .GroupBy(i => i.PlanSuscripcion != null ? i.PlanSuscripcion.Nombre : "Sin plan")
            .Select(g => new CrmReporteConteoDto { Etiqueta = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        return new FinanzasDashboardDto
        {
            IngresosDia = ingresosDia,
            IngresosMes = ingresosMes,
            IngresosAnio = ingresosAnio,
            GastosMes = gastosMes,
            GastosAnio = gastosAnio,
            UtilidadEstimadaMes = ingresosMes - gastosMes,
            UtilidadEstimadaAnio = ingresosAnio - gastosAnio,
            PagosAprobados = pagosAprobados,
            PagosPendientes = pagosPendientes,
            PagosRechazados = pagosRechazados,
            CuentasPorCobrarEstimadas = cuentasPorCobrar,
            EmpresasClientes = empresasClientes,
            RenovacionesDelMes = renovacionesDelMes,
            IngresosPorPlan = ingresosPorPlan
        };
    }

    public async Task<EstadoResultadosDto> ObtenerEstadoResultadosAsync(DateTime desde, DateTime hasta)
    {
        var ingresos = await _db.IngresosFinancieros
            .Include(i => i.CategoriaFinanciera)
            .Where(i => i.Estado == EstadoMovimientoFinanciero.Activo && i.FechaIngreso >= desde && i.FechaIngreso <= hasta)
            .ToListAsync();

        var gastos = await _db.GastosFinancieros
            .Include(g => g.CategoriaFinanciera)
            .Where(g => g.Estado == EstadoMovimientoFinanciero.Activo && g.Fecha >= desde && g.Fecha <= hasta)
            .ToListAsync();

        return new EstadoResultadosDto
        {
            Desde = desde,
            Hasta = hasta,
            TotalIngresos = ingresos.Sum(i => i.Monto),
            TotalGastos = gastos.Sum(g => g.Monto),
            IngresosPorCategoria = ingresos.GroupBy(i => i.CategoriaFinanciera!.Nombre)
                .Select(g => new FinanzasCategoriaMontoDto { Etiqueta = g.Key, Monto = g.Sum(i => i.Monto) })
                .ToList(),
            GastosPorCategoria = gastos.GroupBy(g => g.CategoriaFinanciera!.Nombre)
                .Select(g => new FinanzasCategoriaMontoDto { Etiqueta = g.Key, Monto = g.Sum(x => x.Monto) })
                .ToList()
        };
    }

    public async Task<BalanceGeneralDto> ObtenerBalanceGeneralAsync()
    {
        // Estructura preparada, con lo único que hoy se puede derivar de
        // datos reales: efectivo acumulado (ingresos - gastos históricos) y
        // cuentas por cobrar (pagos pendientes). El resto del balance
        // (equipos, obligaciones, capital) queda en 0 hasta que existan
        // movimientos de esos tipos — nunca se inventan cifras.
        var totalIngresos = await _db.IngresosFinancieros.Where(i => i.Estado == EstadoMovimientoFinanciero.Activo).SumAsync(i => (decimal?)i.Monto) ?? 0m;
        var totalGastos = await _db.GastosFinancieros.Where(g => g.Estado == EstadoMovimientoFinanciero.Activo).SumAsync(g => (decimal?)g.Monto) ?? 0m;
        var cuentasPorCobrar = await _db.Pagos.Where(p => p.EstadoPago == EstadosPago.Pendiente).SumAsync(p => (decimal?)p.Monto) ?? 0m;

        return new BalanceGeneralDto
        {
            Efectivo = totalIngresos - totalGastos,
            CuentasPorCobrar = cuentasPorCobrar,
            CuentasPorPagar = 0m,
            ResultadosAcumulados = totalIngresos - totalGastos
        };
    }

    public async Task<byte[]> GenerarReportePdfAsync(FinanzasExportOpcionesDto opciones)
    {
        var hoy = DateTime.UtcNow;
        var desde = opciones.Desde ?? new DateTime(hoy.Year, hoy.Month, 1);
        var hasta = opciones.Hasta ?? hoy;

        var dashboard = opciones.IncluirIndicadores ? await ObtenerDashboardAsync() : null;
        var estadoResultados = opciones.IncluirEstadoResultados ? await ObtenerEstadoResultadosAsync(desde, hasta) : null;
        var balance = opciones.IncluirBalanceGeneral ? await ObtenerBalanceGeneralAsync() : null;

        var ingresos = opciones.IncluirIngresos
            ? await _db.IngresosFinancieros.Include(i => i.Empresa).Include(i => i.CategoriaFinanciera)
                .Where(i => i.FechaIngreso >= desde && i.FechaIngreso <= hasta)
                .OrderByDescending(i => i.FechaIngreso).ToListAsync()
            : new List<IngresoFinanciero>();

        var gastos = opciones.IncluirGastos
            ? await _db.GastosFinancieros.Include(g => g.CategoriaFinanciera)
                .Where(g => g.Fecha >= desde && g.Fecha <= hasta)
                .OrderByDescending(g => g.Fecha).ToListAsync()
            : new List<GastoFinanciero>();

        var auditoria = opciones.IncluirAuditoria
            ? await _db.AuditoriasFinancieras.Where(a => a.FechaHora >= desde && a.FechaHora <= hasta).OrderByDescending(a => a.FechaHora).Take(200).ToListAsync()
            : new List<AuditoriaFinanciera>();

        QuestPDF.Settings.License = LicenseType.Community;

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(36);
                pagina.DefaultTextStyle(estilo => estilo.FontFamily("Helvetica").FontSize(9));

                pagina.Header().Column(encabezado =>
                {
                    encabezado.Item().Row(fila =>
                    {
                        fila.RelativeItem().Column(marca =>
                        {
                            marca.Item().Text("ChambeaJobs").FontSize(20).Bold().FontColor("#14497A");
                            marca.Item().Text("Reporte de Contabilidad y Finanzas — CONFIDENCIAL").FontSize(9).FontColor("#C6520F").Bold();
                        });
                        fila.ConstantItem(180).AlignRight().Column(c =>
                        {
                            c.Item().Text($"Del {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}").FontSize(9).FontColor("#616B7A");
                            c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor("#8892A0");
                        });
                    });
                    encabezado.Item().PaddingTop(8).LineHorizontal(1).LineColor("#E5E8EC");
                });

                pagina.Content().PaddingVertical(16).Column(cuerpo =>
                {
                    if (dashboard is not null)
                    {
                        cuerpo.Item().Text("Indicadores generales").Bold().FontSize(12).FontColor("#14497A");
                        cuerpo.Item().PaddingTop(4).PaddingBottom(10).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                            void Celda(string etiqueta, string valor)
                            {
                                tabla.Cell().Padding(6).Background("#F1F3F5").Column(col =>
                                {
                                    col.Item().Text(valor).Bold().FontSize(12);
                                    col.Item().Text(etiqueta).FontSize(7.5f).FontColor("#616B7A");
                                });
                            }
                            Celda("Ingresos del mes", $"${dashboard.IngresosMes:0.00}");
                            Celda("Gastos del mes", $"${dashboard.GastosMes:0.00}");
                            Celda("Resultado del mes", $"${dashboard.UtilidadEstimadaMes:0.00}");
                            Celda("Cuentas por cobrar", $"${dashboard.CuentasPorCobrarEstimadas:0.00}");
                            Celda("Ingresos del año", $"${dashboard.IngresosAnio:0.00}");
                            Celda("Gastos del año", $"${dashboard.GastosAnio:0.00}");
                            Celda("Pagos aprobados", dashboard.PagosAprobados.ToString());
                            Celda("Pagos pendientes", dashboard.PagosPendientes.ToString());
                        });
                    }

                    if (estadoResultados is not null)
                    {
                        cuerpo.Item().Text("Estado de resultados").Bold().FontSize(12).FontColor("#14497A");
                        cuerpo.Item().PaddingTop(4).PaddingBottom(10).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });
                            void Fila(string etiqueta, string valor, bool negrita = false)
                            {
                                tabla.Cell().Padding(5).Background("#F1F3F5").Text(etiqueta).FontColor("#616B7A");
                                var celda = tabla.Cell().Padding(5).Text(valor);
                                if (negrita) celda.Bold();
                            }
                            Fila("Total de ingresos", $"${estadoResultados.TotalIngresos:0.00}");
                            Fila("Total de gastos", $"${estadoResultados.TotalGastos:0.00}");
                            Fila("Resultado del período", $"${estadoResultados.ResultadoDelPeriodo:0.00}", negrita: true);
                        });
                    }

                    if (balance is not null)
                    {
                        cuerpo.Item().Text("Balance general (estructura preparada)").Bold().FontSize(12).FontColor("#14497A");
                        cuerpo.Item().PaddingTop(4).PaddingBottom(10).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });
                            void Fila(string etiqueta, string valor)
                            {
                                tabla.Cell().Padding(5).Background("#F1F3F5").Text(etiqueta).FontColor("#616B7A");
                                tabla.Cell().Padding(5).Text(valor);
                            }
                            Fila("Total activos (efectivo + cuentas por cobrar)", $"${balance.TotalActivos:0.00}");
                            Fila("Total pasivos", $"${balance.TotalPasivos:0.00}");
                            Fila("Total patrimonio", $"${balance.TotalPatrimonio:0.00}");
                        });
                    }

                    if (opciones.IncluirIngresos)
                    {
                        cuerpo.Item().Text($"Ingresos del período ({ingresos.Count})").Bold().FontSize(12).FontColor("#14497A");
                        cuerpo.Item().PaddingTop(4).PaddingBottom(10).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); });
                            tabla.Header(h =>
                            {
                                void Enc(string t) => h.Cell().Padding(5).Background("#14497A").Text(t).FontColor("#fff").Bold().FontSize(8);
                                Enc("Empresa"); Enc("RUC"); Enc("Categoría"); Enc("Monto"); Enc("Fecha"); Enc("Estado");
                            });
                            foreach (var i in ingresos)
                            {
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(i.Empresa?.NombreEmpresa ?? "—").FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(i.Empresa?.RUC ?? "—").FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(i.CategoriaFinanciera?.Nombre ?? "—").FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text($"${i.Monto:0.00}").FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(i.FechaIngreso.ToString("dd/MM/yyyy")).FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(i.Estado.ToString()).FontSize(8);
                            }
                            tabla.Cell().ColumnSpan(3).Padding(5).Background("#F1F3F5").AlignRight().Text("Total:").Bold().FontSize(8.5f);
                            tabla.Cell().ColumnSpan(3).Padding(5).Background("#F1F3F5").Text($"${ingresos.Where(i => i.Estado == EstadoMovimientoFinanciero.Activo).Sum(i => i.Monto):0.00}").Bold().FontSize(8.5f);
                        });
                    }

                    if (opciones.IncluirGastos)
                    {
                        cuerpo.Item().Text($"Gastos del período ({gastos.Count})").Bold().FontSize(12).FontColor("#14497A");
                        cuerpo.Item().PaddingTop(4).PaddingBottom(10).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); });
                            tabla.Header(h =>
                            {
                                void Enc(string t) => h.Cell().Padding(5).Background("#14497A").Text(t).FontColor("#fff").Bold().FontSize(8);
                                Enc("Descripción"); Enc("Categoría"); Enc("Monto"); Enc("Fecha"); Enc("Estado");
                            });
                            foreach (var g in gastos)
                            {
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(g.Descripcion).FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(g.CategoriaFinanciera?.Nombre ?? "—").FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text($"${g.Monto:0.00}").FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(g.Fecha.ToString("dd/MM/yyyy")).FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(g.Estado.ToString()).FontSize(8);
                            }
                            tabla.Cell().ColumnSpan(2).Padding(5).Background("#F1F3F5").AlignRight().Text("Total:").Bold().FontSize(8.5f);
                            tabla.Cell().ColumnSpan(3).Padding(5).Background("#F1F3F5").Text($"${gastos.Where(g => g.Estado == EstadoMovimientoFinanciero.Activo).Sum(g => g.Monto):0.00}").Bold().FontSize(8.5f);
                        });
                    }

                    if (opciones.IncluirAuditoria)
                    {
                        cuerpo.Item().Text($"Auditoría del período ({auditoria.Count})").Bold().FontSize(12).FontColor("#14497A");
                        cuerpo.Item().PaddingTop(4).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(4); });
                            tabla.Header(h =>
                            {
                                void Enc(string t) => h.Cell().Padding(5).Background("#14497A").Text(t).FontColor("#fff").Bold().FontSize(8);
                                Enc("Fecha"); Enc("Acción"); Enc("Registro"); Enc("Detalle");
                            });
                            foreach (var a in auditoria)
                            {
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(a.FechaHora.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(a.Accion).FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(a.RegistroAfectado).FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(a.ValorNuevo ?? "—").FontSize(8);
                            }
                        });
                    }
                });

                pagina.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Documento confidencial de uso interno — ChambeaJobs.").FontSize(8).FontColor("#8892A0");
                });
            });
        });

        return documento.GeneratePdf();
    }
}
