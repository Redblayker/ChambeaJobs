namespace ChambeaJobs.Application.Eventos;

/// <summary>Marcador para cualquier evento de dominio publicable por el
/// sistema de eventos (ver <see cref="IPublicadorEventos"/>).</summary>
public interface IEventoDominio { }

public record EmpresaRegistradaEvento(int EmpresaId) : IEventoDominio;
public record VacantePublicadaEvento(int EmpresaId, int VacanteId) : IEventoDominio;
public record PlanCompradoEvento(int EmpresaId, int PaqueteEmpresaId) : IEventoDominio;
public record PagoRealizadoEvento(int EmpresaId, int PagoId, decimal Monto) : IEventoDominio;
public record PlanRenovadoEvento(int EmpresaId, int PaqueteEmpresaId) : IEventoDominio;
public record PlanVencidoEvento(int EmpresaId, int PaqueteEmpresaId) : IEventoDominio;
public record PostulacionRecibidaEvento(int EmpresaId, int VacanteId, int PostulacionId) : IEventoDominio;
public record PerfilActualizadoEvento(int EmpresaId) : IEventoDominio;
public record EmpresaEliminadaEvento(int EmpresaId) : IEventoDominio;
