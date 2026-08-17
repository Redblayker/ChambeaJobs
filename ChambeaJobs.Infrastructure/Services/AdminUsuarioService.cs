using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Infrastructure.Data;
using ChambeaJobs.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Services;

/// <summary>
/// Gestión de usuarios (Candidatos y Administradores) para el panel admin.
/// Vive en Infrastructure porque necesita UserManager/ApplicationUser de Identity,
/// que la capa Application no puede referenciar directamente (evita acoplar
/// Application a detalles de Identity — principio de inversión de dependencias).
/// </summary>
public class AdminUsuarioService : IAdminUsuarioService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IAuditoriaService _auditoriaService;

    public AdminUsuarioService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IAuditoriaService auditoriaService)
    {
        _userManager = userManager;
        _context = context;
        _auditoriaService = auditoriaService;
    }

    public async Task<List<UsuarioAdminDto>> ObtenerCandidatosYAdminsAsync()
    {
        var query =
            from usuario in _context.Users
            join userRole in _context.UserRoles on usuario.Id equals userRole.UserId
            join rol in _context.Roles on userRole.RoleId equals rol.Id
            where rol.Name == RolesSistema.Candidato || rol.Name == RolesSistema.Administrador
            select new UsuarioAdminDto
            {
                Id = usuario.Id,
                Correo = usuario.Email ?? string.Empty,
                Rol = rol.Name ?? string.Empty,
                EstadoCuenta = usuario.EstadoCuenta,
                FechaRegistro = usuario.FechaRegistro
            };

        return await query.OrderBy(u => u.Correo).ToListAsync();
    }

    public async Task SuspenderAsync(string usuarioId, string adminId)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId)
            ?? throw new InvalidOperationException("Este usuario no existe.");

        usuario.EstadoCuenta = false;
        await _userManager.UpdateAsync(usuario);
        await _auditoriaService.RegistrarAsync(adminId, "Suspender", "Usuario", usuarioId);
    }

    public async Task ActivarAsync(string usuarioId, string adminId)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId)
            ?? throw new InvalidOperationException("Este usuario no existe.");

        usuario.EstadoCuenta = true;
        await _userManager.UpdateAsync(usuario);
        await _auditoriaService.RegistrarAsync(adminId, "Activar", "Usuario", usuarioId);
    }

    public async Task<List<string>> ObtenerIdsAdministradoresAsync()
    {
        var query =
            from usuario in _context.Users
            join userRole in _context.UserRoles on usuario.Id equals userRole.UserId
            join rol in _context.Roles on userRole.RoleId equals rol.Id
            where rol.Name == RolesSistema.Administrador
            select usuario.Id;

        return await query.ToListAsync();
    }
}
