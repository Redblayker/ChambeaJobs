using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="IComprobantePagoService"/>
public class ComprobantePagoService : IComprobantePagoService
{
    private readonly IUnitOfWork _unitOfWork;

    public ComprobantePagoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<byte[]> GenerarComprobanteAsync(int pagoId, int empresaId, string correoEmpresa)
    {
        var pago = await _unitOfWork.Pagos.ObtenerPorIdAsync(pagoId)
            ?? throw new InvalidOperationException("No se encontró este pago.");

        if (pago.EstadoPago != EstadosPago.Aprobado)
        {
            throw new InvalidOperationException("Solo se puede descargar el comprobante de un pago aprobado.");
        }

        var paquete = await _unitOfWork.PaquetesEmpresa.ObtenerPorIdAsync(pago.PaqueteEmpresaId)
            ?? throw new InvalidOperationException("No se encontró el paquete asociado a este pago.");

        if (paquete.EmpresaId != empresaId)
        {
            throw new InvalidOperationException("Este comprobante no pertenece a tu empresa.");
        }

        var empresa = await _unitOfWork.Empresas.ObtenerPorIdAsync(empresaId)
            ?? throw new InvalidOperationException("No se encontró tu perfil de empresa.");

        return ConstruirPdf(pago, paquete, empresa, correoEmpresa);
    }

    public async Task<byte[]> GenerarComprobanteAdminAsync(int pagoId)
    {
        var pago = await _unitOfWork.Pagos.ObtenerPorIdAsync(pagoId)
            ?? throw new InvalidOperationException("No se encontró este pago.");

        if (pago.EstadoPago != EstadosPago.Aprobado)
        {
            throw new InvalidOperationException("Solo se puede descargar el comprobante de un pago aprobado.");
        }

        var paquete = await _unitOfWork.PaquetesEmpresa.ObtenerPorIdAsync(pago.PaqueteEmpresaId)
            ?? throw new InvalidOperationException("No se encontró el paquete asociado a este pago.");

        var empresa = await _unitOfWork.Empresas.ObtenerPorIdAsync(paquete.EmpresaId)
            ?? throw new InvalidOperationException("No se encontró la empresa asociada a este pago.");

        // El admin no necesariamente conoce el correo de la empresa a mano;
        // se muestra "—" ya que ese dato vive en Identity, fuera del dominio.
        return ConstruirPdf(pago, paquete, empresa, "—");
    }

    private static byte[] ConstruirPdf(Pago pago, PaqueteEmpresa paquete, Empresa empresa, string correoEmpresa)
    {
        // Número de comprobante: correlativo simple basado en el Id + año de pago (ej. CJ-2026-000042).
        var numeroComprobante = $"CJ-{pago.FechaPago:yyyy}-{pago.Id:D6}";

        QuestPDF.Settings.License = LicenseType.Community;

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(40);
                pagina.DefaultTextStyle(estilo => estilo.FontFamily("Helvetica").FontSize(10));

                pagina.Header().Column(encabezado =>
                {
                    encabezado.Item().Row(fila =>
                    {
                        fila.RelativeItem().Column(marca =>
                        {
                            marca.Item().Text("ChambeaJobs").FontSize(22).Bold().FontColor("#14497A");
                            marca.Item().Text("Portal de búsqueda de empleo — Nicaragua").FontSize(9).FontColor("#616B7A");
                        });
                        fila.ConstantItem(180).Column(numero =>
                        {
                            numero.Item().AlignRight().Text("COMPROBANTE DE PAGO").Bold().FontSize(13).FontColor("#12161C");
                            numero.Item().AlignRight().Text(numeroComprobante).FontSize(11).FontColor("#F0661E").Bold();
                        });
                    });
                    encabezado.Item().PaddingTop(10).LineHorizontal(1).LineColor("#E5E8EC");
                });

                pagina.Content().PaddingVertical(20).Column(cuerpo =>
                {
                    cuerpo.Item().Text("Datos de la empresa").Bold().FontSize(11).FontColor("#14497A");
                    cuerpo.Item().PaddingBottom(10).Column(datosEmpresa =>
                    {
                        datosEmpresa.Item().Text($"Razón social: {empresa.NombreEmpresa}");
                        datosEmpresa.Item().Text($"RUC: {empresa.RUC}");
                        datosEmpresa.Item().Text($"Correo: {correoEmpresa}");
                    });

                    cuerpo.Item().PaddingTop(10).Text("Detalle del pago").Bold().FontSize(11).FontColor("#14497A");
                    cuerpo.Item().PaddingTop(8).Table(tabla =>
                    {
                        tabla.ColumnsDefinition(columnas =>
                        {
                            columnas.RelativeColumn(3);
                            columnas.RelativeColumn(2);
                        });

                        void Fila(string etiqueta, string valor)
                        {
                            tabla.Cell().Padding(6).Background("#F1F3F5").Text(etiqueta).FontColor("#616B7A");
                            tabla.Cell().Padding(6).Text(valor);
                        }

                        Fila("Concepto", $"Paquete de publicación de vacantes ({paquete.VacantesIncluidas} vacantes / 30 días)");
                        Fila("Monto pagado", $"${pago.Monto:0.00} USD");
                        Fila("Método de pago", pago.MetodoPago);
                        Fila("Referencia de transacción", pago.ReferenciaTransaccion ?? "N/A");
                        Fila("Fecha de pago reportada", pago.FechaPago.ToString("dd/MM/yyyy"));
                        Fila("Fecha de aprobación", pago.FechaRevision?.ToString("dd/MM/yyyy HH:mm") ?? "N/A");
                        Fila("Vigencia del paquete", $"{paquete.FechaCompra:dd/MM/yyyy} — {paquete.FechaVencimiento:dd/MM/yyyy}");
                    });

                    cuerpo.Item().PaddingTop(24).Background("#FDEEE3").Padding(12).Text(texto =>
                    {
                        texto.Span("Nota: ").Bold().FontColor("#C6520F");
                        texto.Span("Este documento es un comprobante interno de ChambeaJobs y no constituye una factura fiscal válida ante la DGI. Para efectos contables/tributarios, solicita la factura correspondiente según el régimen fiscal de tu empresa.")
                            .FontColor("#C6520F");
                    });
                });

                pagina.Footer().AlignCenter().Text(texto =>
                {
                    texto.Span("Generado automáticamente por ChambeaJobs el ").FontSize(8).FontColor("#8892A0");
                    texto.Span(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")).FontSize(8).FontColor("#8892A0");
                });
            });
        });

        return documento.GeneratePdf();
    }
}
