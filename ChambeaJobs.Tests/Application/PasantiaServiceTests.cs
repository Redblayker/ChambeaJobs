using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Application.Services;
using ChambeaJobs.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace ChambeaJobs.Tests.Application;

/// <summary>
/// Pruebas del módulo de Pasantías: la validación de fechas/remuneración al
/// publicar, y la regla de "solo tu carrera" al postularse (igual que en
/// Vacantes de empleo, pero en su propio módulo independiente).
/// </summary>
public class PasantiaServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<INotificacionService> _notificacionService = new();
    private readonly Mock<IAuditoriaService> _auditoriaService = new();
    private readonly PasantiaService _servicio;

    public PasantiaServiceTests()
    {
        _servicio = new PasantiaService(_unitOfWork.Object, _notificacionService.Object, _auditoriaService.Object);
    }

    [Fact]
    public async Task PublicarPasantiaAsync_CuandoLaFechaDeCierreEsHoyOAnterior_LanzaExcepcion()
    {
        var datos = new PasantiaFormDto
        {
            Titulo = "Pasantía de prueba",
            CategoriaId = 1,
            UbicacionId = 1,
            Modalidad = "Remoto",
            Descripcion = "Descripción",
            DuracionMeses = 3,
            FechaCierre = DateTime.UtcNow.Date // hoy, no es una fecha futura válida
        };

        var accion = () => _servicio.PublicarPasantiaAsync(empresaId: 1, datos);

        await accion.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*posterior a hoy*");
    }

    [Fact]
    public async Task PublicarPasantiaAsync_CuandoEsRemuneradaSinMonto_LanzaExcepcion()
    {
        var datos = new PasantiaFormDto
        {
            Titulo = "Pasantía de prueba",
            CategoriaId = 1,
            UbicacionId = 1,
            Modalidad = "Remoto",
            Descripcion = "Descripción",
            DuracionMeses = 3,
            FechaCierre = DateTime.UtcNow.AddDays(30),
            EsRemunerada = true,
            MontoRemuneracion = null // falta el monto
        };

        var accion = () => _servicio.PublicarPasantiaAsync(empresaId: 1, datos);

        await accion.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*monto*");
    }

    [Fact]
    public async Task PostularAsync_CuandoElCandidatoNoEstudioEsaCarrera_LanzaExcepcion()
    {
        var pasantia = new Pasantia
        {
            Id = 10,
            Estado = EstadosPasantia.Activa,
            CategoriaId = 3,
            Categoria = new Categoria { Nombre = "Diseño Gráfico" }
        };
        var candidatoSinLaCarrera = new Candidato
        {
            Id = 1,
            Educaciones = new List<Educacion> { new() { CategoriaId = 7 } } // otra carrera distinta
        };

        _unitOfWork.Setup(u => u.PostulacionesPasantia.ExistePostulacionAsync(1, 10)).ReturnsAsync(false);
        _unitOfWork.Setup(u => u.Pasantias.ObtenerConDetalleAsync(10)).ReturnsAsync(pasantia);
        _unitOfWork.Setup(u => u.Candidatos.ObtenerConDetallePorIdAsync(1)).ReturnsAsync(candidatoSinLaCarrera);

        var accion = () => _servicio.PostularAsync(candidatoId: 1, pasantiaId: 10);

        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Diseño Gráfico*");
    }

    [Fact]
    public async Task PostularAsync_CuandoYaSePostuloAntes_LanzaExcepcion()
    {
        _unitOfWork.Setup(u => u.PostulacionesPasantia.ExistePostulacionAsync(1, 10)).ReturnsAsync(true);

        var accion = () => _servicio.PostularAsync(candidatoId: 1, pasantiaId: 10);

        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ya te has postulado*");
    }
}
