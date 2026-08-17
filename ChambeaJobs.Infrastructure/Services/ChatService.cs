using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChambeaJobs.Infrastructure.Services;

/// <inheritdoc cref="IChatService"/>
public class ChatService : IChatService
{
    private readonly ApplicationDbContext _db;
    private readonly ISoporteService _soporteService;
    private readonly IRealTimeChatBotNotifier _notifier;

    public ChatService(ApplicationDbContext db, ISoporteService soporteService, IRealTimeChatBotNotifier notifier)
    {
        _db = db;
        _soporteService = soporteService;
        _notifier = notifier;
    }

    public async Task<ChatConversacionDto> ObtenerOCrearConversacionAsync(int? conversacionId, string? usuarioId, string rolUsuario)
    {
        ChatConversacion? conversacion = null;

        if (conversacionId.HasValue)
        {
            conversacion = await _db.ChatConversaciones
                .Include(c => c.Mensajes)
                .FirstOrDefaultAsync(c => c.Id == conversacionId.Value);
        }

        if (conversacion is null)
        {
            conversacion = new ChatConversacion
            {
                UsuarioId = usuarioId,
                RolUsuario = rolUsuario,
                Estado = EstadoConversacionChat.EnCurso,
                FechaInicio = DateTime.UtcNow
            };
            _db.ChatConversaciones.Add(conversacion);
            await _db.SaveChangesAsync();

            var bienvenida = rolUsuario == "Empresa"
                ? "¡Hola! Este es el chat de asistencia de ChambeaJobs. Cuéntanos tu consulta —por ejemplo, cómo publicar una vacante, revisar tus candidatos o consultar tus pagos— y un agente de soporte te responderá en breves momentos."
                : "¡Hola! Este es el chat de asistencia de ChambeaJobs. Cuéntanos tu consulta —por ejemplo, cómo crear tu cuenta, subir tu CV o postularte— y un agente de soporte te responderá en breves momentos.";

            _db.ChatMensajes.Add(new ChatMensaje
            {
                ConversacionId = conversacion.Id,
                Emisor = EmisorMensajeChat.Bot,
                Texto = bienvenida,
                FechaEnvio = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            conversacion = await _db.ChatConversaciones
                .Include(c => c.Mensajes)
                .FirstAsync(c => c.Id == conversacion.Id);
        }

        return Mapear(conversacion);
    }

    public async Task<ChatConversacionDto> EnviarMensajeAsync(ChatEnviarMensajeDto dto, string? usuarioId, string rolUsuario, string nombreContacto, string correoContacto)
    {
        var conversacionDto = await ObtenerOCrearConversacionAsync(dto.ConversacionId, usuarioId, rolUsuario);
        var conversacion = await _db.ChatConversaciones.FirstAsync(c => c.Id == conversacionDto.Id);

        var mensajeUsuario = new ChatMensaje
        {
            ConversacionId = conversacion.Id,
            Emisor = EmisorMensajeChat.Usuario,
            Texto = dto.Texto.Trim(),
            FechaEnvio = DateTime.UtcNow
        };
        _db.ChatMensajes.Add(mensajeUsuario);
        await _db.SaveChangesAsync();
        await _notifier.NotificarNuevoMensajeAsync(conversacion.Id, Mapear(mensajeUsuario));

        // Sin respuestas automáticas: el primer mensaje del usuario conecta
        // directo con un agente de soporte real (ver EscalarASoporteAsync).
        if (conversacion.Estado == EstadoConversacionChat.EnCurso)
        {
            await EscalarASoporteAsync(conversacion.Id, nombreContacto, correoContacto);

            _db.ChatMensajes.Add(new ChatMensaje
            {
                ConversacionId = conversacion.Id,
                Emisor = EmisorMensajeChat.Bot,
                Texto = "En breves momentos un agente de soporte se comunicará contigo.",
                FechaEnvio = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            var mensajeConfirmacion = await _db.ChatMensajes
                .Where(m => m.ConversacionId == conversacion.Id)
                .OrderByDescending(m => m.Id)
                .FirstAsync();
            await _notifier.NotificarNuevoMensajeAsync(conversacion.Id, Mapear(mensajeConfirmacion));
        }

        var actualizada = await _db.ChatConversaciones
            .Include(c => c.Mensajes)
            .FirstAsync(c => c.Id == conversacion.Id);

        return Mapear(actualizada);
    }

    public async Task<int> EscalarASoporteAsync(int conversacionId, string nombreContacto, string correoContacto)
    {
        var conversacion = await _db.ChatConversaciones
            .Include(c => c.Mensajes)
            .FirstOrDefaultAsync(c => c.Id == conversacionId)
            ?? throw new InvalidOperationException("No se encontró la conversación.");

        if (conversacion.TicketSoporteId.HasValue)
        {
            return conversacion.TicketSoporteId.Value; // ya estaba escalada, evita duplicar el ticket
        }

        var ultimaConsulta = conversacion.Mensajes
            .Where(m => m.Emisor == EmisorMensajeChat.Usuario)
            .OrderByDescending(m => m.FechaEnvio)
            .Select(m => m.Texto)
            .FirstOrDefault() ?? "(sin mensaje)";

        var ticketId = await _soporteService.CrearTicketAsync(conversacion.UsuarioId, new CrearTicketSoporteDto
        {
            NombreContacto = nombreContacto,
            CorreoContacto = correoContacto,
            Asunto = "Consulta desde el chat de asistencia",
            Mensaje = ultimaConsulta
        });

        conversacion.Estado = EstadoConversacionChat.Escalada;
        conversacion.TicketSoporteId = ticketId;
        conversacion.FechaCierre = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ticketId;
    }

    private static ChatConversacionDto Mapear(ChatConversacion c) => new()
    {
        Id = c.Id,
        Estado = c.Estado.ToString(),
        TicketSoporteId = c.TicketSoporteId,
        Mensajes = c.Mensajes
            .OrderBy(m => m.FechaEnvio)
            .Select(Mapear)
            .ToList()
    };

    private static ChatMensajeDto Mapear(ChatMensaje m) => new()
    {
        Id = m.Id,
        Emisor = m.Emisor.ToString(),
        Texto = m.Texto,
        UrlAccion = m.UrlAccion,
        FechaEnvio = m.FechaEnvio
    };
}
