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

    public LocalFileStorageService(IWebHostEnvironment entorno)
    {
        _entorno = entorno;
    }

    public async Task<string> GuardarArchivoAsync(IFormFile archivo, string subcarpeta)
    {
        ValidarArchivo(archivo, subcarpeta);

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

    private static void ValidarArchivo(IFormFile archivo, string subcarpeta)
    {
        if (archivo.Length == 0)
        {
            throw new ArgumentException("El archivo está vacío.");
        }

        var extension = Path.GetExtension(archivo.FileName);

        if (CarpetasDocumentoPdf.Contains(subcarpeta))
        {
            if (!ExtensionesCvPermitidas.Contains(extension))
            {
                throw new ArgumentException("El archivo debe estar en formato PDF.");
            }
            if (archivo.Length > PesoMaximoCvBytes)
            {
                throw new ArgumentException("El archivo no puede superar los 5 MB.");
            }
        }
        else if (CarpetasImagen.Contains(subcarpeta))
        {
            if (!ExtensionesImagenPermitidas.Contains(extension))
            {
                throw new ArgumentException("La imagen debe ser JPG o PNG.");
            }
            if (archivo.Length > PesoMaximoImagenBytes)
            {
                throw new ArgumentException("La imagen no puede superar los 2 MB.");
            }
        }
        else if (CarpetasVideo.Contains(subcarpeta))
        {
            if (!ExtensionesVideoPermitidas.Contains(extension))
            {
                throw new ArgumentException("El video debe ser MP4, WEBM o MOV.");
            }
            if (archivo.Length > PesoMaximoVideoBytes)
            {
                throw new ArgumentException("El video no puede superar los 50 MB.");
            }
        }
        else
        {
            // Carpeta desconocida: por seguridad, no se asume ningún tipo por
            // defecto — es preferible fallar explícitamente a permitir un
            // archivo arbitrario sin validar (evita subida de tipos peligrosos
            // como .exe, .js, .html, etc. por una carpeta nueva mal registrada).
            throw new ArgumentException($"No hay reglas de validación definidas para la carpeta '{subcarpeta}'.");
        }
    }
}
