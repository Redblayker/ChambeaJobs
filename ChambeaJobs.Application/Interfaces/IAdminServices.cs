using ChambeaJobs.Application.DTOs;

namespace ChambeaJobs.Application.Interfaces;

public interface IAdminUsuarioService
{
    Task<List<UsuarioAdminDto>> ObtenerCandidatosYAdminsAsync();
    Task SuspenderAsync(string usuarioId, string adminId);
    Task ActivarAsync(string usuarioId, string adminId);

    /// <summary>Ids de todos los usuarios con rol Administrador, para notificaciones globales (ej. nuevo pago pendiente).</summary>
    Task<List<string>> ObtenerIdsAdministradoresAsync();
}

public interface IAdminEmpresaService
{
    Task<List<EmpresaAdminDto>> ObtenerTodasAsync();
    Task SuspenderAsync(string usuarioId, string adminId);
    Task ActivarAsync(string usuarioId, string adminId);
}

public interface IAdminVacanteService
{
    Task<List<VacanteAdminDto>> ObtenerTodasAsync();
    Task DespublicarAsync(int vacanteId, string adminId);
    Task EliminarAsync(int vacanteId, string adminId);
}

public interface IAdminCatalogoService
{
    Task<List<CategoriaFormDto>> ObtenerCategoriasAsync();
    Task CrearCategoriaAsync(string nombre, string? descripcion);
    Task EditarCategoriaAsync(int id, string nombre, string? descripcion);
    Task EliminarCategoriaAsync(int id);

    Task<List<UbicacionFormDto>> ObtenerUbicacionesAsync();
    Task CrearUbicacionAsync(string departamento, string ciudad);
    Task EditarUbicacionAsync(int id, string departamento, string ciudad);
    Task EliminarUbicacionAsync(int id);
}

public interface IConfiguracionSistemaService
{
    Task<ConfiguracionPaqueteDto> ObtenerConfiguracionPaqueteAsync();
    Task ActualizarConfiguracionPaqueteAsync(ConfiguracionPaqueteDto datos, string adminId);
}

public interface IReporteService
{
    Task<ReporteDashboardDto> ObtenerDashboardAsync();
}

public interface IAuditoriaService
{
    Task RegistrarAsync(string usuarioId, string accion, string entidadAfectada, string entidadId, string? detalle = null);
    Task<List<AuditoriaDto>> ObtenerRecientesAsync(int cantidad = 100);
}

public interface ISoporteService
{
    /// <summary>Crea el ticket y devuelve su Id, para poder entrar directo al chat de esa conversación.</summary>
    Task<int> CrearTicketAsync(string? usuarioId, CrearTicketSoporteDto datos);

    Task<List<TicketSoporteDto>> ObtenerTodosAsync();

    /// <summary>Ticket con todos sus mensajes cargados (la conversación completa), o null si no existe.</summary>
    Task<TicketSoporteDto?> ObtenerConMensajesAsync(int ticketId);

    /// <summary>
    /// El ticket abierto más reciente de este usuario/correo — se usa para
    /// que, si ya tenías una conversación en curso, vuelvas a caer ahí en
    /// vez de crear un ticket duplicado cada vez que visitas Soporte.
    /// </summary>
    Task<TicketSoporteDto?> ObtenerMiTicketAbiertoAsync(string? usuarioId, string correo);

    /// <summary>Envía un mensaje dentro de la conversación (de cualquiera de los dos lados) y lo persiste.</summary>
    Task<MensajeSoporteDto> EnviarMensajeAsync(int ticketId, string? autorUsuarioId, string autorNombre, bool esAdmin, string mensaje);

    Task ResponderAsync(int ticketId, string respuesta, string adminId);

    Task CerrarTicketAsync(int ticketId);
}
