using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Application.Services;
using ChambeaJobs.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace ChambeaJobs.Tests.Application;

/// <summary>
/// Pruebas de la regla de negocio más crítica del sistema: postularse a una
/// vacante. Cubre las 4 validaciones que bloquean una postulación inválida
/// y el camino feliz donde todo procede correctamente.
/// </summary>
public class PostulacionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICompatibilidadService> _compatibilidadService = new();
    private readonly Mock<INotificacionService> _notificacionService = new();
    private readonly PostulacionService _servicio;

    public PostulacionServiceTests()
    {
        _servicio = new PostulacionService(_unitOfWork.Object, _compatibilidadService.Object, _notificacionService.Object);
    }

    [Fact]
    public async Task PostularAsync_CuandoYaExisteLaPostulacion_LanzaExcepcion()
    {
        // Arrange: el candidato 1 ya se postuló antes a la vacante 5.
        _unitOfWork.Setup(u => u.Postulaciones.ExistePostulacionAsync(1, 5)).ReturnsAsync(true);

        // Act
        var accion = () => _servicio.PostularAsync(candidatoId: 1, vacanteId: 5);

        // Assert
        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ya te has postulado*", "no debe permitir postularse dos veces a la misma vacante");
    }

    [Fact]
    public async Task PostularAsync_CuandoLaVacanteNoExiste_LanzaExcepcion()
    {
        _unitOfWork.Setup(u => u.Postulaciones.ExistePostulacionAsync(1, 99)).ReturnsAsync(false);
        _unitOfWork.Setup(u => u.Vacantes.ObtenerConDetalleAsync(99)).ReturnsAsync((Vacante?)null);

        var accion = () => _servicio.PostularAsync(candidatoId: 1, vacanteId: 99);

        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no existe*");
    }

    [Fact]
    public async Task PostularAsync_CuandoLaVacanteYaEstaCerrada_LanzaExcepcion()
    {
        var vacanteCerrada = new Vacante { Id = 5, Estado = EstadosVacante.Cerrada, CategoriaId = 2 };

        _unitOfWork.Setup(u => u.Postulaciones.ExistePostulacionAsync(1, 5)).ReturnsAsync(false);
        _unitOfWork.Setup(u => u.Vacantes.ObtenerConDetalleAsync(5)).ReturnsAsync(vacanteCerrada);

        var accion = () => _servicio.PostularAsync(candidatoId: 1, vacanteId: 5);

        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ya no acepta postulaciones*");
    }

    [Fact]
    public async Task PostularAsync_CuandoElCandidatoNoEstudioEsaCarrera_LanzaExcepcion()
    {
        // Arrange: la vacante es de categoría 2 (ej. Contabilidad), pero el
        // candidato solo tiene estudios registrados en la categoría 7 (ej. Tecnología).
        var vacante = new Vacante { Id = 5, Estado = EstadosVacante.Activa, CategoriaId = 2, Categoria = new Categoria { Nombre = "Contabilidad" } };
        var candidatoSinLaCarrera = new Candidato
        {
            Id = 1,
            Educaciones = new List<Educacion> { new() { CategoriaId = 7 } }
        };

        _unitOfWork.Setup(u => u.Postulaciones.ExistePostulacionAsync(1, 5)).ReturnsAsync(false);
        _unitOfWork.Setup(u => u.Vacantes.ObtenerConDetalleAsync(5)).ReturnsAsync(vacante);
        _unitOfWork.Setup(u => u.Candidatos.ObtenerConDetallePorIdAsync(1)).ReturnsAsync(candidatoSinLaCarrera);

        var accion = () => _servicio.PostularAsync(candidatoId: 1, vacanteId: 5);

        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Contabilidad*", "el mensaje debe explicar a qué carrera pertenece la vacante");
    }

    [Fact]
    public async Task PostularAsync_CuandoTodoEsValido_CreaLaPostulacionYGuardaCambios()
    {
        // Arrange: la carrera del candidato SÍ coincide con la categoría de la vacante.
        var vacante = new Vacante { Id = 5, Estado = EstadosVacante.Activa, CategoriaId = 7, Categoria = new Categoria { Nombre = "Tecnología" } };
        var candidatoConLaCarrera = new Candidato
        {
            Id = 1,
            Nombres = "Kenny",
            Apellidos = "Espinoza",
            Educaciones = new List<Educacion> { new() { CategoriaId = 7 } }
        };

        _unitOfWork.Setup(u => u.Postulaciones.ExistePostulacionAsync(1, 5)).ReturnsAsync(false);
        _unitOfWork.Setup(u => u.Vacantes.ObtenerConDetalleAsync(5)).ReturnsAsync(vacante);
        _unitOfWork.Setup(u => u.Candidatos.ObtenerConDetallePorIdAsync(1)).ReturnsAsync(candidatoConLaCarrera);

        // Act
        await _servicio.PostularAsync(candidatoId: 1, vacanteId: 5);

        // Assert: se agregó exactamente una postulación con los datos correctos, y se guardaron los cambios.
        _unitOfWork.Verify(u => u.Postulaciones.AgregarAsync(It.Is<Postulacion>(p =>
            p.CandidatoId == 1 && p.VacanteId == 5)), Times.Once);
        _unitOfWork.Verify(u => u.GuardarCambiosAsync(), Times.AtLeastOnce);
    }
}
