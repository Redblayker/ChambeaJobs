using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="ISoporteService"/>
public class SoporteService : ISoporteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificacionService _notificacionService;
    private readonly IAdminUsuarioService _adminUsuarioService;
    private readonly IRealTimeChatNotifier _chatNotifier;

    public SoporteService(
        IUnitOfWork unitOfWork,
        INotificacionService notificacionService,
        IAdminUsuarioService adminUsuarioService,
        IRealTimeChatNotifier chatNotifier)
    {
        _unitOfWork = unitOfWork;
        _notificacionService = notificacionService;
        _adminUsuarioService = adminUsuarioService;
        _chatNotifier = chatNotifier;
    }

    public async Task<int> CrearTicketAsync(string? usuarioId, CrearTicketSoporteDto datos)
    {
        var ticket = new TicketSoporte
        {
            UsuarioId = usuarioId,
            NombreContacto = datos.NombreContacto,
            CorreoContacto = datos.CorreoContacto,
            Asunto = datos.Asunto,
            Mensaje = datos.Mensaje,
            Estado = TicketSoporte.Estados.Abierto,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.TicketsSoporte.AgregarAsync(ticket);
        await _unitOfWork.GuardarCambiosAsync();

        // El mensaje inicial también queda como el primer mensaje del chat,
        // para que la conversación se vea completa desde el principio.
        await _unitOfWork.MensajesSoporte.AgregarAsync(new MensajeSoporte
        {
            TicketSoporteId = ticket.Id,
            AutorUsuarioId = usuarioId,
            AutorNombre = datos.NombreContacto,
            EsAdmin = false,
            Mensaje = datos.Mensaje,
            FechaEnvio = ticket.FechaCreacion
        });
        await _unitOfWork.GuardarCambiosAsync();

        // Notificar a TODOS los administradores: hay un ticket nuevo esperando respuesta.
        var idsAdmins = await _adminUsuarioService.ObtenerIdsAdministradoresAsync();
        foreach (var adminId in idsAdmins)
        {
            await _notificacionService.CrearAsync(
                adminId,
                "TicketSoporteNuevo",
                $"Nuevo ticket de soporte de {datos.NombreContacto}: \"{datos.Asunto}\"",
                "/Admin/Soporte");
        }

        return ticket.Id;
    }

    public async Task<List<TicketSoporteDto>> ObtenerTodosAsync()
    {
        var tickets = await _unitOfWork.TicketsSoporte.ObtenerTodosAsync();

        return tickets
            .OrderBy(t => t.Estado == TicketSoporte.Estados.Abierto ? 0 : 1) // abiertos primero
            .ThenByDescending(t => t.FechaCreacion)
            .Select(t => new TicketSoporteDto
            {
                Id = t.Id,
                NombreContacto = t.NombreContacto,
                CorreoContacto = t.CorreoContacto,
                Asunto = t.Asunto,
                Mensaje = t.Mensaje,
                Estado = t.Estado,
                RespuestaAdmin = t.RespuestaAdmin,
                FechaCreacion = t.FechaCreacion,
                FechaRespuesta = t.FechaRespuesta
            })
            .ToList();
    }

    public async Task<TicketSoporteDto?> ObtenerConMensajesAsync(int ticketId)
    {
        var ticket = await _unitOfWork.TicketsSoporte.ObtenerPorIdAsync(ticketId);
        if (ticket is null)
        {
            return null;
        }

        var mensajes = (await _unitOfWork.MensajesSoporte.ObtenerTodosAsync())
            .Where(m => m.TicketSoporteId == ticketId)
            .OrderBy(m => m.FechaEnvio)
            .Select(m => new MensajeSoporteDto
            {
                Id = m.Id,
                AutorNombre = m.AutorNombre,
                EsAdmin = m.EsAdmin,
                Mensaje = m.Mensaje,
                FechaEnvio = m.FechaEnvio
            })
            .ToList();

        // Compatibilidad hacia atrás: un ticket creado ANTES de que existiera
        // el chat no tiene filas en MensajesSoporte (esa tabla es nueva) —
        // sin esto, el chat se vería vacío para conversaciones que ya
        // existían, aunque el ticket sí tenga su mensaje original guardado
        // en el campo TicketSoporte.Mensaje de toda la vida.
        if (!mensajes.Any())
        {
            mensajes.Add(new MensajeSoporteDto
            {
                Id = 0,
                AutorNombre = ticket.NombreContacto,
                EsAdmin = false,
                Mensaje = ticket.Mensaje,
                FechaEnvio = ticket.FechaCreacion
            });

            if (!string.IsNullOrWhiteSpace(ticket.RespuestaAdmin))
            {
                mensajes.Add(new MensajeSoporteDto
                {
                    Id = -1,
                    AutorNombre = "Soporte ChambeaJobs",
                    EsAdmin = true,
                    Mensaje = ticket.RespuestaAdmin,
                    FechaEnvio = ticket.FechaRespuesta ?? ticket.FechaCreacion
                });
            }
        }

        return new TicketSoporteDto
        {
            Id = ticket.Id,
            NombreContacto = ticket.NombreContacto,
            CorreoContacto = ticket.CorreoContacto,
            Asunto = ticket.Asunto,
            Mensaje = ticket.Mensaje,
            Estado = ticket.Estado,
            RespuestaAdmin = ticket.RespuestaAdmin,
            FechaCreacion = ticket.FechaCreacion,
            FechaRespuesta = ticket.FechaRespuesta,
            Mensajes = mensajes
        };
    }

    public async Task<TicketSoporteDto?> ObtenerMiTicketAbiertoAsync(string? usuarioId, string correo)
    {
        var tickets = await _unitOfWork.TicketsSoporte.ObtenerTodosAsync();

        // Si está logueado, se busca por UsuarioId (más confiable). Si es
        // visitante anónimo, se busca por correo — no es perfecto (dos
        // personas podrían compartir un correo genérico), pero es la única
        // pista disponible sin pedirle que se registre solo para escribir a soporte.
        var ticket = tickets
            .Where(t => t.Estado == TicketSoporte.Estados.Abierto)
            .Where(t => !string.IsNullOrWhiteSpace(usuarioId)
                ? t.UsuarioId == usuarioId
                : t.CorreoContacto.Equals(correo, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(t => t.FechaCreacion)
            .FirstOrDefault();

        return ticket is null ? null : await ObtenerConMensajesAsync(ticket.Id);
    }

    public async Task<MensajeSoporteDto> EnviarMensajeAsync(int ticketId, string? autorUsuarioId, string autorNombre, bool esAdmin, string mensaje)
    {
        var ticket = await _unitOfWork.TicketsSoporte.ObtenerPorIdAsync(ticketId)
            ?? throw new InvalidOperationException("No se encontró este ticket de soporte.");

        if (string.IsNullOrWhiteSpace(mensaje))
        {
            throw new ArgumentException("El mensaje no puede estar vacío.");
        }

        var nuevoMensaje = new MensajeSoporte
        {
            TicketSoporteId = ticketId,
            AutorUsuarioId = autorUsuarioId,
            AutorNombre = autorNombre,
            EsAdmin = esAdmin,
            Mensaje = mensaje.Trim(),
            FechaEnvio = DateTime.UtcNow
        };

        await _unitOfWork.MensajesSoporte.AgregarAsync(nuevoMensaje);
        await _unitOfWork.GuardarCambiosAsync();

        var mensajeDto = new MensajeSoporteDto
        {
            Id = nuevoMensaje.Id,
            AutorNombre = nuevoMensaje.AutorNombre,
            EsAdmin = nuevoMensaje.EsAdmin,
            Mensaje = nuevoMensaje.Mensaje,
            FechaEnvio = nuevoMensaje.FechaEnvio
        };

        // Empuja el mensaje en tiempo real a quien esté viendo esta
        // conversación en este momento (el otro lado del chat).
        await _chatNotifier.NotificarNuevoMensajeAsync(ticketId, mensajeDto);

        // Además, si respondió el admin, se le manda una notificación normal
        // al usuario dueño del ticket (por si no tiene la ventana de chat abierta).
        if (esAdmin && !string.IsNullOrWhiteSpace(ticket.UsuarioId))
        {
            await _notificacionService.CrearAsync(
                ticket.UsuarioId,
                "TicketSoporteNuevoMensaje",
                $"Soporte respondió tu ticket \"{ticket.Asunto}\".",
                $"/Soporte/Chat/{ticketId}");
        }

        return mensajeDto;
    }

    public async Task ResponderAsync(int ticketId, string respuesta, string adminId)
    {
        // Se conserva por compatibilidad con el flujo anterior (respuesta
        // única desde la lista de Admin), pero ahora también registra el
        // mensaje en el chat, para que quede como una conversación coherente
        // sin importar por cuál de las dos pantallas respondió el admin.
        await EnviarMensajeAsync(ticketId, adminId, "Soporte ChambeaJobs", esAdmin: true, respuesta);

        var ticket = await _unitOfWork.TicketsSoporte.ObtenerPorIdAsync(ticketId)
            ?? throw new InvalidOperationException("No se encontró este ticket de soporte.");

        ticket.RespuestaAdmin = respuesta;
        ticket.AdminUsuarioId = adminId;
        ticket.FechaRespuesta = DateTime.UtcNow;

        _unitOfWork.TicketsSoporte.Actualizar(ticket);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task CerrarTicketAsync(int ticketId)
    {
        var ticket = await _unitOfWork.TicketsSoporte.ObtenerPorIdAsync(ticketId)
            ?? throw new InvalidOperationException("No se encontró este ticket de soporte.");

        ticket.Estado = TicketSoporte.Estados.Resuelto;
        _unitOfWork.TicketsSoporte.Actualizar(ticket);
        await _unitOfWork.GuardarCambiosAsync();
    }
}
