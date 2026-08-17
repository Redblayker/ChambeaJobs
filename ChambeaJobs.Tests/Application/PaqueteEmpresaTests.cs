using ChambeaJobs.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ChambeaJobs.Tests.Application;

/// <summary>
/// Pruebas de dominio puro (sin mocks) sobre <see cref="PaqueteEmpresa.TieneCupoDisponible"/>.
/// Esta es exactamente la lógica que causó un bug real en el proyecto
/// (el plan Empresarial/ilimitado marcaba "Agotado" incorrectamente) — estas
/// pruebas existen para que ese error específico nunca vuelva a colarse.
/// </summary>
public class PaqueteEmpresaTests
{
    [Fact]
    public void TieneCupoDisponible_PlanIlimitado_SiempreDaCupoMientrasEsteVigente()
    {
        // VacantesIncluidas = null representa el plan Empresarial (sin límite).
        var paquete = new PaqueteEmpresa
        {
            Estado = EstadosPaquete.Vigente,
            VacantesIncluidas = null,
            VacantesConsumidas = 9999, // aunque haya publicado muchísimas vacantes...
            FechaVencimiento = DateTime.UtcNow.AddDays(10)
        };

        paquete.TieneCupoDisponible().Should().BeTrue("el plan ilimitado nunca debe agotarse por cantidad de vacantes");
    }

    [Fact]
    public void TieneCupoDisponible_PlanBasico_ConCupoLibre_DaCupoDisponible()
    {
        var paquete = new PaqueteEmpresa
        {
            Estado = EstadosPaquete.Vigente,
            VacantesIncluidas = 10,
            VacantesConsumidas = 4,
            FechaVencimiento = DateTime.UtcNow.AddDays(10)
        };

        paquete.TieneCupoDisponible().Should().BeTrue();
    }

    [Fact]
    public void TieneCupoDisponible_PlanBasico_ConCupoAgotado_NoDaCupoDisponible()
    {
        var paquete = new PaqueteEmpresa
        {
            Estado = EstadosPaquete.Vigente,
            VacantesIncluidas = 10,
            VacantesConsumidas = 10, // ya usó las 10
            FechaVencimiento = DateTime.UtcNow.AddDays(10)
        };

        paquete.TieneCupoDisponible().Should().BeFalse();
    }

    [Fact]
    public void TieneCupoDisponible_CuandoElPaqueteYaVencio_NoDaCupoAunConEspacioLibre()
    {
        var paquete = new PaqueteEmpresa
        {
            Estado = EstadosPaquete.Vigente,
            VacantesIncluidas = 10,
            VacantesConsumidas = 1, // le sobra cupo...
            FechaVencimiento = DateTime.UtcNow.AddDays(-1) // ...pero ya venció
        };

        paquete.TieneCupoDisponible().Should().BeFalse("un paquete vencido no debe permitir publicar aunque le quede cupo numérico");
    }

    [Fact]
    public void TieneCupoDisponible_CuandoElEstadoNoEsVigente_NoDaCupo()
    {
        var paquete = new PaqueteEmpresa
        {
            Estado = EstadosPaquete.Pendiente, // esperando aprobación de un admin
            VacantesIncluidas = null,
            VacantesConsumidas = 0,
            FechaVencimiento = DateTime.UtcNow.AddDays(10)
        };

        paquete.TieneCupoDisponible().Should().BeFalse();
    }
}
