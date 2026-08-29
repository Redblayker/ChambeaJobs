using ChambeaJobs.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ChambeaJobs.Infrastructure.Storage;

/// <summary>
/// Implementación local (sistema de archivos, bajo wwwroot/uploads) de
/// IFileStorageService. Válida para despliegue en IIS; para Azure se recomienda sustituir esta clase
/// por una que use Azure Blob Storage, sin tocar el resto del código
/// gracias a la interfaz.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _entorno;

    // Reglas de validación.
    //
    // IMPORTANTE: antes esta validación solo distinguía "cv" (PDF) de "todo lo
    // demás" (imagen), lo que rechazaba SIEMPRE los PDF de Certificados y Cursos
    // (una carpeta como "certificados" caía en la rama de imagen JPG/PNG).
    // Ahora se valida por categoría explícita según la subcarpeta de destino.
    private static readonly HashSet<string> ExtensionesCvPermitidas = new(StringComparer.OrdinalIgnoreCase) { ".pdf" };
    private static readonly HashSet<string> ExtensionesImagenPermitidas = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };
    private static readonly HashSet<string> ExtensionesVideoPermitidas = new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".webm", ".mov" };
    private const int PesoMaximoCvBytes = 5 * 1024 * 1024;     // 5 MB (CV, Certificados, Cursos)
    private const int PesoMaximoImagenBytes = 2 * 1024 * 1024; // 2 MB (Foto, Logo, Galería)
    private const int PesoMaximoVideoBytes = 50 * 1024 * 1024; // 50 MB (Video Currículum)

    /// <summary>Subcarpetas cuyo contenido debe ser un PDF (documentos).</summary>
    private static readonly HashSet<string> CarpetasDocumentoPdf = new(StringComparer.OrdinalIgnoreCase)
    {
        "cv", "certificados", "cursos"
    };

    /// <summary>Subcarpetas cuyo contenido debe ser una imagen (JPG/PNG).</summary>
    private static readonly HashSet<string> CarpetasImagen = new(StringComparer.OrdinalIgnoreCase)
    {
        "fotos", "logos", "galeria-empresa"
    };

    /// <summary>Subcarpetas cuyo contenido debe ser un video (MP4/WEBM/MOV).</summary>
    private static readonly HashSet<string> CarpetasVideo = new(StringComparer.OrdinalIgnoreCase)
    {
        "videos-cv", "videos-galeria-empresa"
    };
    private static readonly HashSet<string> CarpetasDocumentoOImagen = new(StringComparer.OrdinalIgnoreCase)
    {
        "crm", "comprobantes-gastos"
    };

    public LocalFileStorageService(IWebHostEnvironment entorno)
    {
        _entorno = entorno;
    }

    public async Task<string> GuardarArchivoAsync(IFormFile archivo, string subcarpeta)
    {
        await ValidarArchivoAsync(archivo, subcarpeta);

        var carpetaDestino = Path.Combine(_entorno.WebRootPath, "uploads", subcarpeta);
        Directory.CreateDirectory(carpetaDestino);

        var extension = Path.GetExtension(archivo.FileName);
        var nombreUnico = $"{Guid.NewGuid()}{extension}";
        var rutaFisica = Path.Combine(carpetaDestino, nombreUnico);

        await using (var stream = new FileStream(rutaFisica, FileMode.Create))
        {
            await archivo.CopyToAsync(stream);
        }

        return $"/uploads/{subcarpeta}/{nombreUnico}";
    }

    public void EliminarArchivo(string rutaRelativa)
    {
        var rutaFisica = ObtenerRutaCargaSegura(rutaRelativa);
        if (rutaFisica is not null && File.Exists(rutaFisica))
        {
            File.Delete(rutaFisica);
        }
    }

    public async Task<byte[]?> LeerArchivoAsync(string? rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa))
        {
            return null;
        }

        var rutaFisica = ObtenerRutaCargaSegura(rutaRelativa);
        if (rutaFisica is null || !File.Exists(rutaFisica))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(rutaFisica);
    }

    private string? ObtenerRutaCargaSegura(string rutaRelativa)
    {
        var rutaNormalizada = rutaRelativa.Replace('\\', '/').TrimStart('/');
        if (!rutaNormalizada.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var directorioUploads = Path.GetFullPath(Path.Combine(_entorno.WebRootPath, "uploads"));
        var rutaFisica = Path.GetFullPath(Path.Combine(
            _entorno.WebRootPath,
            rutaNormalizada.Replace('/', Path.DirectorySeparatorChar)));
        var prefijoUploads = directorioUploads + Path.DirectorySeparatorChar;

        return rutaFisica.StartsWith(prefijoUploads, StringComparison.OrdinalIgnoreCase)
            ? rutaFisica
            : null;
    }

    private static async Task ValidarArchivoAsync(IFormFile archivo, string subcarpeta)
    {
        if (archivo.Length == 0)
        {
            throw new ArgumentException("El archivo está vacío.");
        }

        var extension = Path.GetExtension(archivo.FileName);

        HashSet<string> extensionesPermitidas;
        long pesoMaximo;
        if (CarpetasDocumentoPdf.Contains(subcarpeta))
        {
            extensionesPermitidas = ExtensionesCvPermitidas;
            pesoMaximo = PesoMaximoCvBytes;
        }
        else if (CarpetasImagen.Contains(subcarpeta))
        {
            extensionesPermitidas = ExtensionesImagenPermitidas;
            pesoMaximo = PesoMaximoImagenBytes;
        }
        else if (CarpetasVideo.Contains(subcarpeta))
        {
            extensionesPermitidas = ExtensionesVideoPermitidas;
            pesoMaximo = PesoMaximoVideoBytes;
        }
        else if (CarpetasDocumentoOImagen.Contains(subcarpeta))
        {
            extensionesPermitidas = new HashSet<string>(ExtensionesCvPermitidas.Concat(ExtensionesImagenPermitidas), StringComparer.OrdinalIgnoreCase);
            pesoMaximo = PesoMaximoCvBytes;
        }
        else
        {
            // Carpeta desconocida: por seguridad, no se asume ningún tipo por
            // defecto — es preferible fallar explícitamente a permitir un
            // archivo arbitrario sin validar (evita subida de tipos peligrosos
            // como .exe, .js, .html, etc. por una carpeta nueva mal registrada).
            throw new ArgumentException($"No hay reglas de validación definidas para la carpeta '{subcarpeta}'.");
        }

        if (!extensionesPermitidas.Contains(extension))
            throw new ArgumentException("El tipo de archivo no está permitido.");
        if (archivo.Length > pesoMaximo)
            throw new ArgumentException($"El archivo no puede superar los {pesoMaximo / 1024 / 1024} MB.");

        await using var stream = archivo.OpenReadStream();
        var contenido = new byte[checked((int)archivo.Length)];
        var leidos = 0;
        while (leidos < contenido.Length)
        {
            var leidosAhora = await stream.ReadAsync(contenido.AsMemory(leidos));
            if (leidosAhora == 0)
            {
                break;
            }

            leidos += leidosAhora;
        }

        if (leidos != contenido.Length || !ContenidoCoincideConExtension(extension, contenido))
            throw new ArgumentException("El contenido del archivo no coincide con su formato declarado.");
    }

    private static bool ContenidoCoincideConExtension(string extension, ReadOnlySpan<byte> contenido) =>
        extension.ToLowerInvariant() switch
        {
            ".pdf" => EsPdfValido(contenido),
            ".png" => EsPngValido(contenido),
            ".jpg" or ".jpeg" => EsJpegValido(contenido),
            ".webm" => EsWebmValido(contenido),
            ".mp4" or ".mov" => EsArchivoIsoBaseMediaValido(contenido),
            _ => false
        };

    private static bool EsPdfValido(ReadOnlySpan<byte> contenido) =>
        contenido.StartsWith("%PDF-"u8)
        && contenido.Length >= 8
        && contenido.Slice(Math.Max(0, contenido.Length - 1024)).IndexOf("%%EOF"u8) >= 0;

    private static bool EsPngValido(ReadOnlySpan<byte> contenido)
    {
        if (!contenido.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
        {
            return false;
        }

        var offset = 8;
        var encontroIend = false;
        while (offset + 12 <= contenido.Length)
        {
            var longitud = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(contenido.Slice(offset, 4));
            if (longitud > int.MaxValue || offset + 12L + longitud > contenido.Length)
            {
                return false;
            }

            var tipo = contenido.Slice(offset + 4, 4);
            if (tipo.SequenceEqual("IEND"u8))
            {
                encontroIend = longitud == 0 && offset + 12 == contenido.Length;
                break;
            }

            offset += checked((int)(12 + longitud));
        }

        return encontroIend;
    }

    private static bool EsJpegValido(ReadOnlySpan<byte> contenido) =>
        contenido.Length >= 4
        && contenido.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF })
        && contenido[^2] == 0xFF
        && contenido[^1] == 0xD9;

    private static bool EsWebmValido(ReadOnlySpan<byte> contenido) =>
        contenido.StartsWith(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 })
        && contenido.IndexOf(new byte[] { 0x42, 0x82, 0x84, (byte)'w', (byte)'e', (byte)'b', (byte)'m' }) >= 0;

    private static bool EsArchivoIsoBaseMediaValido(ReadOnlySpan<byte> contenido)
    {
        if (contenido.Length < 16 || !contenido.Slice(4, 4).SequenceEqual("ftyp"u8))
        {
            return false;
        }

        var tamanoCaja = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(contenido);
        return tamanoCaja >= 16 && tamanoCaja <= contenido.Length;
    }
}
