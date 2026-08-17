using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="ICvGeneradorService"/>
public class CvGeneradorService : ICvGeneradorService
{
    private readonly ICandidatoService _candidatoService;
    private readonly IFileStorageService _almacenamiento;

    // Paleta de colores del CV.
    private const string ColorBarraLateral = "#122C4A";
    private const string ColorAcento = "#E67E22";
    private const string ColorTitulo = "#122C4A";
    private const string ColorTexto = "#3A4250";
    private const string ColorSuave = "#6B7686";
    private const string ColorLineaSuave = "#E3E7EC";
    private const string ColorTextoClaro = "#D8E1EC";
    private const string ColorTextoClaroSuave = "#9FB2C9";

    public CvGeneradorService(ICandidatoService candidatoService, IFileStorageService almacenamiento)
    {
        _candidatoService = candidatoService;
        _almacenamiento = almacenamiento;
    }

    public async Task<byte[]> GenerarPdfAsync(string usuarioId, string? correo, string? telefono)
    {
        var perfil = await _candidatoService.ObtenerPerfilPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró tu perfil de candidato.");

        var fotoBytes = await _almacenamiento.LeerArchivoAsync(perfil.FotoUrl);

        QuestPDF.Settings.License = LicenseType.Community;

        // Título profesional: se toma del puesto más reciente, si existe.
        var puestoActual = perfil.Experiencias
            .OrderByDescending(e => e.FechaFin ?? DateTime.MaxValue)
            .ThenByDescending(e => e.FechaInicio)
            .FirstOrDefault()?.Puesto;

        var iniciales = ObtenerIniciales(perfil.Nombres, perfil.Apellidos);

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(0);
                pagina.DefaultTextStyle(estilo => estilo.FontFamily("Helvetica").FontSize(9.5f).FontColor(ColorTexto));

                pagina.Content().Row(fila =>
                {
                    // ---------- Barra lateral ----------
                    fila.ConstantItem(175).Background(ColorBarraLateral).MinHeight(790).Padding(20).Column(barra =>
                    {
                        // Foto de perfil (o iniciales si el candidato no ha subido una).
                        if (fotoBytes is { Length: > 0 })
                        {
                            barra.Item().AlignCenter().Width(105).Height(105)
                                 .Border(2).BorderColor(ColorAcento).Padding(2)
                                 .Image(fotoBytes).FitArea();
                        }
                        else
                        {
                            barra.Item().AlignCenter().Width(105).Height(105)
                                 .Border(2).BorderColor(ColorAcento).Background("#1B3C61")
                                 .AlignCenter().AlignMiddle()
                                 .Text(iniciales).FontSize(30).Bold().FontColor(ColorTextoClaro);
                        }

                        barra.Item().PaddingTop(16);

                        // ---------- Contacto ----------
                        SeccionBarraLateral(barra, "Contacto");
                        if (!string.IsNullOrWhiteSpace(correo))
                        {
                            ItemBarraLateral(barra, correo);
                        }
                        if (!string.IsNullOrWhiteSpace(telefono))
                        {
                            ItemBarraLateral(barra, telefono);
                        }
                        if (!string.IsNullOrWhiteSpace(perfil.Direccion))
                        {
                            ItemBarraLateral(barra, perfil.Direccion);
                        }

                        // ---------- Disponibilidad ----------
                        if (!string.IsNullOrWhiteSpace(perfil.Disponibilidad))
                        {
                            barra.Item().PaddingTop(14);
                            SeccionBarraLateral(barra, "Disponibilidad");
                            ItemBarraLateral(barra, perfil.Disponibilidad);
                        }

                        // ---------- Habilidades ----------
                        if (perfil.Habilidades.Any())
                        {
                            barra.Item().PaddingTop(14);
                            SeccionBarraLateral(barra, "Habilidades");
                            foreach (var hab in perfil.Habilidades)
                            {
                                barra.Item().PaddingBottom(6).Column(col =>
                                {
                                    col.Item().Text(hab.Nombre).FontSize(9).FontColor(ColorTextoClaro);
                                    if (!string.IsNullOrEmpty(hab.NivelDominio))
                                    {
                                        col.Item().PaddingTop(2).BarraNivel(NivelANumero(hab.NivelDominio, maximo: 3));
                                    }
                                });
                            }
                        }

                        // ---------- Idiomas ----------
                        if (perfil.Idiomas.Any())
                        {
                            barra.Item().PaddingTop(8);
                            SeccionBarraLateral(barra, "Idiomas");
                            foreach (var idi in perfil.Idiomas)
                            {
                                barra.Item().PaddingBottom(6).Column(col =>
                                {
                                    col.Item().Text($"{idi.Nombre} · {idi.Nivel}").FontSize(9).FontColor(ColorTextoClaro);
                                    col.Item().PaddingTop(2).BarraNivel(NivelMcerANumero(idi.Nivel));
                                });
                            }
                        }
                    });

                    // ---------- Columna principal ----------
                    fila.RelativeItem().Padding(28).Column(cuerpo =>
                    {
                        cuerpo.Item().Text($"{perfil.Nombres} {perfil.Apellidos}").FontSize(23).Bold().FontColor(ColorTitulo);
                        if (!string.IsNullOrWhiteSpace(puestoActual))
                        {
                            cuerpo.Item().PaddingTop(2).Text(puestoActual).FontSize(12).FontColor(ColorAcento);
                        }
                        cuerpo.Item().PaddingTop(10).PaddingBottom(6).LineHorizontal(1.5f).LineColor(ColorAcento);

                        void TituloSeccion(string texto)
                        {
                            cuerpo.Item().PaddingTop(14).Text(texto).FontSize(12.5f).Bold().FontColor(ColorTitulo);
                            cuerpo.Item().PaddingTop(2).PaddingBottom(6).LineHorizontal(0.75f).LineColor(ColorLineaSuave);
                        }

                        // Experiencia laboral
                        if (perfil.Experiencias.Any())
                        {
                            TituloSeccion("Experiencia Laboral");
                            foreach (var exp in perfil.Experiencias.OrderByDescending(e => e.FechaInicio))
                            {
                                cuerpo.Item().PaddingBottom(9).Row(fila2 =>
                                {
                                    fila2.RelativeItem().Column(izq =>
                                    {
                                        izq.Item().Text(exp.Puesto).Bold().FontSize(10.5f).FontColor(ColorTitulo);
                                        izq.Item().Text(exp.NombreEmpresa).FontSize(9.5f).FontColor(ColorAcento);
                                        if (!string.IsNullOrEmpty(exp.Descripcion))
                                        {
                                            izq.Item().PaddingTop(3).Text(exp.Descripcion).FontSize(9).FontColor(ColorTexto);
                                        }
                                    });
                                    fila2.ConstantItem(110).AlignRight().Text(
                                        $"{exp.FechaInicio:MMM yyyy} - {(exp.FechaFin.HasValue ? exp.FechaFin.Value.ToString("MMM yyyy") : "Actualidad")}")
                                        .FontSize(8.5f).FontColor(ColorSuave);
                                });
                            }
                        }

                        // Educación
                        if (perfil.Educaciones.Any())
                        {
                            TituloSeccion("Educación");
                            foreach (var edu in perfil.Educaciones.OrderByDescending(e => e.FechaInicio))
                            {
                                cuerpo.Item().PaddingBottom(9).Row(fila2 =>
                                {
                                    fila2.RelativeItem().Column(izq =>
                                    {
                                        izq.Item().Text(edu.TituloObtenido).Bold().FontSize(10.5f).FontColor(ColorTitulo);
                                        izq.Item().Text($"{edu.Institucion} · {edu.NivelEducativo}").FontSize(9.5f).FontColor(ColorAcento);
                                    });
                                    fila2.ConstantItem(110).AlignRight().Text(
                                        $"{edu.FechaInicio:yyyy} - {(edu.FechaFin.HasValue ? edu.FechaFin.Value.ToString("yyyy") : "Actualidad")}")
                                        .FontSize(8.5f).FontColor(ColorSuave);
                                });
                            }
                        }

                        // Cursos y capacitaciones
                        if (perfil.Cursos.Any())
                        {
                            TituloSeccion("Cursos y Capacitaciones");
                            foreach (var curso in perfil.Cursos)
                            {
                                cuerpo.Item().PaddingBottom(6).Row(fila2 =>
                                {
                                    fila2.RelativeItem().Column(izq =>
                                    {
                                        izq.Item().Text(curso.Nombre).Bold().FontSize(9.5f).FontColor(ColorTitulo);
                                        if (!string.IsNullOrEmpty(curso.Institucion))
                                        {
                                            izq.Item().Text(curso.Institucion).FontSize(9).FontColor(ColorSuave);
                                        }
                                    });
                                    if (curso.HorasDuracion.HasValue)
                                    {
                                        fila2.ConstantItem(110).AlignRight().Text($"{curso.HorasDuracion} horas").FontSize(8.5f).FontColor(ColorSuave);
                                    }
                                });
                            }
                        }

                        // Certificados y Documentos
                        if (perfil.Certificados.Any())
                        {
                            TituloSeccion("Certificados y Documentos");
                            foreach (var cert in perfil.Certificados)
                            {
                                cuerpo.Item().PaddingBottom(4).Text(texto =>
                                {
                                    texto.Span($"[{cert.TipoDocumento}]  ").FontSize(8.5f).Bold().FontColor(ColorAcento);
                                    texto.Span(cert.Nombre).FontSize(9.5f).FontColor(ColorTexto);
                                    if (!string.IsNullOrEmpty(cert.InstitucionEmisora))
                                    {
                                        texto.Span($" — {cert.InstitucionEmisora}").FontSize(9).FontColor(ColorSuave);
                                    }
                                });
                            }
                        }
                    });
                });

                // ---------- Pie de página ----------
                pagina.Footer().PaddingVertical(6).AlignCenter().Text(texto =>
                {
                    texto.Span("Generado automáticamente desde ChambeaJobs — ").FontSize(7.5f).FontColor(ColorSuave);
                    texto.Span(DateTime.UtcNow.ToString("dd/MM/yyyy")).FontSize(7.5f).FontColor(ColorSuave);
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static string ObtenerIniciales(string nombres, string apellidos)
    {
        var inicialNombre = nombres.Trim().FirstOrDefault();
        var inicialApellido = apellidos.Trim().FirstOrDefault();
        var texto = new string(new[] { inicialNombre, inicialApellido }.Where(c => c != default(char)).ToArray());
        return texto.Length > 0 ? texto.ToUpperInvariant() : "?";
    }

    /// <summary>Convierte "Básico/Intermedio/Avanzado" en un valor de 1 a <paramref name="maximo"/>.</summary>
    private static int NivelANumero(string nivel, int maximo)
    {
        return nivel.Trim().ToLowerInvariant() switch
        {
            "básico" or "basico" => 1,
            "intermedio" => 2,
            "avanzado" => 3,
            _ => maximo,
        };
    }

    /// <summary>Convierte un nivel MCER (A1-C2) en una barra de progreso de 1 a 3.</summary>
    private static int NivelMcerANumero(string nivelMcer)
    {
        return nivelMcer.Trim().ToUpperInvariant() switch
        {
            "A1" or "A2" => 1,
            "B1" or "B2" => 2,
            "C1" or "C2" => 3,
            _ => 2,
        };
    }

    private static void SeccionBarraLateral(ColumnDescriptor barra, string titulo)
    {
        barra.Item().Text(titulo.ToUpperInvariant()).FontSize(10).Bold().FontColor(ColorAcento);
        barra.Item().PaddingTop(3).PaddingBottom(6).LineHorizontal(0.75f).LineColor("#2C4A6B");
    }

    private static void ItemBarraLateral(ColumnDescriptor barra, string texto)
    {
        barra.Item().PaddingBottom(5).Text(texto).FontSize(8.75f).FontColor(ColorTextoClaro);
    }
}

/// <summary>Extensión para dibujar una barra de nivel (1 a 3 segmentos) en la barra lateral del CV.</summary>
internal static class NivelBarraExtensions
{
    public static void BarraNivel(this IContainer contenedor, int nivelActivo, int totalSegmentos = 3)
    {
        contenedor.Row(fila =>
        {
            for (var i = 1; i <= totalSegmentos; i++)
            {
                var activo = i <= nivelActivo;
                fila.RelativeItem().PaddingRight(i == totalSegmentos ? 0 : 3).Height(4)
                    .Background(activo ? "#E67E22" : "#2C4A6B");
            }
        });
    }
}
