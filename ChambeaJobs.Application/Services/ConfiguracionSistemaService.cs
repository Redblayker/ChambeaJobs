using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="IConfiguracionSistemaService"/>
public class ConfiguracionSistemaService : IConfiguracionSistemaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaService _auditoriaService;

    public ConfiguracionSistemaService(IUnitOfWork unitOfWork, IAuditoriaService auditoriaService)
    {
        _unitOfWork = unitOfWork;
        _auditoriaService = auditoriaService;
    }

    public async Task<ConfiguracionPaqueteDto> ObtenerConfiguracionPaqueteAsync()
    {
        var configuraciones = await _unitOfWork.ConfiguracionesSistema.ObtenerTodosAsync();

        return new ConfiguracionPaqueteDto
        {
            Precio = decimal.Parse(BuscarValor(configuraciones, ConfiguracionSistema.Claves.PaqueteVacantesPrecio, "20.00")),
            CantidadVacantes = int.Parse(BuscarValor(configuraciones, ConfiguracionSistema.Claves.PaqueteVacantesCantidad, "10")),
            DiasVigencia = int.Parse(BuscarValor(configuraciones, ConfiguracionSistema.Claves.PaqueteVacantesDiasVigencia, "30"))
        };
    }

    public async Task ActualizarConfiguracionPaqueteAsync(ConfiguracionPaqueteDto datos, string adminId)
    {
        if (datos.Precio <= 0 || datos.CantidadVacantes <= 0 || datos.DiasVigencia <= 0)
        {
            throw new ArgumentException("El precio, la cantidad de vacantes y los días de vigencia deben ser valores positivos.");
        }

        await ActualizarClaveAsync(ConfiguracionSistema.Claves.PaqueteVacantesPrecio, datos.Precio.ToString("0.00"));
        await ActualizarClaveAsync(ConfiguracionSistema.Claves.PaqueteVacantesCantidad, datos.CantidadVacantes.ToString());
        await ActualizarClaveAsync(ConfiguracionSistema.Claves.PaqueteVacantesDiasVigencia, datos.DiasVigencia.ToString());

        await _auditoriaService.RegistrarAsync(
            adminId, "Editar", "ConfiguracionSistema", "PaqueteVacantes",
            $"Precio={datos.Precio}, Cantidad={datos.CantidadVacantes}, Dias={datos.DiasVigencia}");
    }

    private async Task ActualizarClaveAsync(string clave, string valor)
    {
        var configuraciones = await _unitOfWork.ConfiguracionesSistema.ObtenerTodosAsync();
        var configuracion = configuraciones.FirstOrDefault(c => c.Clave == clave);

        if (configuracion is null)
        {
            await _unitOfWork.ConfiguracionesSistema.AgregarAsync(new ConfiguracionSistema { Clave = clave, Valor = valor });
        }
        else
        {
            configuracion.Valor = valor;
            _unitOfWork.ConfiguracionesSistema.Actualizar(configuracion);
        }

        await _unitOfWork.GuardarCambiosAsync();
    }

    private static string BuscarValor(IEnumerable<ConfiguracionSistema> configuraciones, string clave, string valorPorDefecto) =>
        configuraciones.FirstOrDefault(c => c.Clave == clave)?.Valor ?? valorPorDefecto;
}
