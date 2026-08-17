using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace ChambeaJobs.Infrastructure.Services;

/// <inheritdoc cref="IAdminEmpresaService"/>
public class AdminEmpresaService : IAdminEmpresaService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoriaService;

    public AdminEmpresaService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IAuditoriaService auditoriaService)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _auditoriaService = auditoriaService;
    }

    public async Task<List<EmpresaAdminDto>> ObtenerTodasAsync()
    {
        var empresas = await _unitOfWork.Empresas.ObtenerTodosAsync();
        var resultado = new List<EmpresaAdminDto>();

        foreach (var empresa in empresas)
        {
            var usuario = await _userManager.FindByIdAsync(empresa.UsuarioId);
            var paqueteVigente = await _unitOfWork.PaquetesEmpresa.ObtenerVigentePorEmpresaAsync(empresa.Id);
            var vacantes = await _unitOfWork.Vacantes.ObtenerPorEmpresaAsync(empresa.Id);

            resultado.Add(new EmpresaAdminDto
            {
                Id = empresa.Id,
                UsuarioId = empresa.UsuarioId,
                NombreEmpresa = empresa.NombreEmpresa,
                Correo = usuario?.Email ?? string.Empty,
                EstadoCuenta = usuario?.EstadoCuenta ?? false,
                EstadoPaquete = paqueteVigente?.Estado ?? "Sin paquete",
                VacantesActivas = vacantes.Count(v => v.Estado == EstadosVacante.Activa)
            });
        }

        return resultado.OrderBy(e => e.NombreEmpresa).ToList();
    }

    public async Task SuspenderAsync(string usuarioId, string adminId)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId)
            ?? throw new InvalidOperationException("Este usuario no existe.");

        usuario.EstadoCuenta = false;
        await _userManager.UpdateAsync(usuario);
        await _auditoriaService.RegistrarAsync(adminId, "Suspender", "Empresa", usuarioId);
    }

    public async Task ActivarAsync(string usuarioId, string adminId)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId)
            ?? throw new InvalidOperationException("Este usuario no existe.");

        usuario.EstadoCuenta = true;
        await _userManager.UpdateAsync(usuario);
        await _auditoriaService.RegistrarAsync(adminId, "Activar", "Empresa", usuarioId);
    }
}
