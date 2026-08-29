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
        var rutaFisica = Path.Combine(_entorno.WebRootPath, rutaRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(rutaFisica))
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

        var rutaFisica = Path.Combine(_entorno.WebRootPath, rutaRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(rutaFisica))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(rutaFisica);
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
        var firma = new byte[16];
        var leidos = 0;
        while (leidos < firma.Length)
        {
            var leidosAhora = await stream.ReadAsync(firma.AsMemory(leidos, firma.Length - leidos));
            if (leidosAhora == 0) break;
            leidos += leidosAhora;
        }

        if (!FirmaCoincideConExtension(extension, firma.AsSpan(0, leidos)))
            throw new ArgumentException("El contenido del archivo no coincide con su formato declarado.");
    }

    private static bool FirmaCoincideConExtension(string extension, ReadOnlySpan<byte> firma) =>
        extension.ToLowerInvariant() switch
        {
            ".pdf" => firma.StartsWith("%PDF-"u8),
            ".png" => firma.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".jpg" or ".jpeg" => firma.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF }),
            ".webm" => firma.StartsWith(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }),
            ".mp4" or ".mov" => firma.Length >= 8 && firma.Slice(4, 4).SequenceEqual("ftyp"u8),
            _ => false
        };
}
