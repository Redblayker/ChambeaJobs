namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Genera un currículum en PDF a partir de la información ya registrada
/// en el perfil del Candidato (experiencia, educación, habilidades,
/// idiomas y certificados), usando una plantilla prediseñada.
/// </summary>
public interface ICvGeneradorService
{
    Task<byte[]> GenerarPdfAsync(string usuarioId, string? correo, string? telefono);
}
