using ChambeaJobs.Application.Eventos;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Infrastructure.Eventos.ManejadoresCrm;

public class CrmManejadorEmpresaRegistrada : IManejadorEvento<EmpresaRegistradaEvento>
{
    private readonly ICrmService _crmService;
    public CrmManejadorEmpresaRegistrada(ICrmService crmService) => _crmService = crmService;

    public Task ManejarAsync(EmpresaRegistradaEvento evento) =>
        _crmService.SincronizarDesdeRegistroEmpresaAsync(evento.EmpresaId);
}

public class CrmManejadorPerfilActualizado : IManejadorEvento<PerfilActualizadoEvento>
{
    private readonly ICrmService _crmService;
    public CrmManejadorPerfilActualizado(ICrmService crmService) => _crmService = crmService;

    public async Task ManejarAsync(PerfilActualizadoEvento evento)
    {
        await _crmService.SincronizarDatosDesdeEmpresaAsync(evento.EmpresaId);
        await _crmService.AvanzarEtapaAutomaticaAsync(evento.EmpresaId, EtapaPipelineCrm.EmpresaVerificada);
    }
}

public class CrmManejadorVacantePublicada : IManejadorEvento<VacantePublicadaEvento>
{
    private readonly ICrmService _crmService;
    public CrmManejadorVacantePublicada(ICrmService crmService) => _crmService = crmService;

    public Task ManejarAsync(VacantePublicadaEvento evento) =>
        _crmService.AvanzarEtapaAutomaticaAsync(evento.EmpresaId, EtapaPipelineCrm.EmpresaActiva);
}

public class CrmManejadorPagoRealizado : IManejadorEvento<PagoRealizadoEvento>
{
    private readonly ICrmService _crmService;
    public CrmManejadorPagoRealizado(ICrmService crmService) => _crmService = crmService;

    public Task ManejarAsync(PagoRealizadoEvento evento) =>
        _crmService.AvanzarEtapaAutomaticaAsync(evento.EmpresaId, EtapaPipelineCrm.ClienteActivo);
}

/// <summary>Segundo suscriptor del mismo evento PagoRealizadoEvento — el CRM
/// (arriba) y Finanzas reaccionan cada uno por su cuenta, sin saber que el
/// otro existe. Así es como queda desacoplado: PaqueteEmpresaService solo
/// avisa "se aprobó un pago", nunca llama directamente a Finanzas.</summary>
public class FinanzasManejadorPagoAprobado : IManejadorEvento<PagoRealizadoEvento>
{
    private readonly IIngresoFinancieroService _ingresoService;
    public FinanzasManejadorPagoAprobado(IIngresoFinancieroService ingresoService) => _ingresoService = ingresoService;

    public Task ManejarAsync(PagoRealizadoEvento evento) =>
        _ingresoService.CrearDesdeIngresoPagoAsync(evento.PagoId);
}

public class CrmManejadorPlanVencido : IManejadorEvento<PlanVencidoEvento>
{
    private readonly ICrmService _crmService;
    public CrmManejadorPlanVencido(ICrmService crmService) => _crmService = crmService;

    public Task ManejarAsync(PlanVencidoEvento evento) =>
        _crmService.MarcarClienteInactivoSiCorrespondeAsync(evento.EmpresaId);
}
