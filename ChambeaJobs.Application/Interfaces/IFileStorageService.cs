using Microsoft.AspNetCore.Http;

namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Abstracción del almacenamiento físico de archivos (CV, logos, fotos).
/// La implementación actual guarda en disco
/// local bajo wwwroot/uploads; en el despliegue a Azure se puede sustituir por una implementación con
/// Azure Blob Storage sin cambiar el resto del código, gracias a esta
/// interfaz (principio D de SOLID).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Guarda el archivo subido y devuelve la ruta relativa donde quedó
    /// almacenado (ej. "/uploads/cv/abc123.pdf").
    /// </summary>
    Task<string> GuardarArchivoAsync(IFormFile archivo, string subcarpeta);

    void EliminarArchivo(string rutaRelativa);

    /// <summary>
    /// Lee el contenido binario de un archivo previamente guardado (ej. para
    /// incrustar una foto de perfil en el CV en PDF). Devuelve null si la ruta
    /// es vacía o el archivo ya no existe en disco.
    /// </summary>
    Task<byte[]?> LeerArchivoAsync(string? rutaRelativa);
}
