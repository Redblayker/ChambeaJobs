using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="IAuditoriaService"/>
public class AuditoriaService : IAuditoriaService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditoriaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task RegistrarAsync(string usuarioId, string accion, string entidadAfectada, string entidadId, string? detalle = null)
    {
        var registro = new Auditoria
        {
            UsuarioId = usuarioId,
            Accion = accion,
            EntidadAfectada = entidadAfectada,
            EntidadId = entidadId,
            DetalleJson = detalle,
            FechaHora = DateTime.UtcNow
        };

        await _unitOfWork.Auditorias.AgregarAsync(registro);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task<List<AuditoriaDto>> ObtenerRecientesAsync(int cantidad = 100)
    {
        var registros = await _unitOfWork.Auditorias.ObtenerTodosAsync();

        return registros
            .OrderByDescending(a => a.FechaHora)
            .Take(cantidad)
            .Select(a => new AuditoriaDto
            {
                Id = a.Id,
                // Nota: se muestra el Id del usuario; resolver el correo real
                // requeriría acceso a Identity (UserManager), que vive en la
                // capa Infrastructure — se puede enriquecer desde el controlador si se necesita.
                UsuarioCorreo = a.UsuarioId,
                Accion = a.Accion,
                EntidadAfectada = a.EntidadAfectada,
                EntidadId = a.EntidadId,
                FechaHora = a.FechaHora
            })
            .ToList();
    }
}
