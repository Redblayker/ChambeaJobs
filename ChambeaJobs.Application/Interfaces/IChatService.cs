using ChambeaJobs.Application.DTOs;

namespace ChambeaJobs.Application.Interfaces;

/// <summary>
/// Motor que decide qué responder ante un mensaje del usuario. La
/// implementación actual (<c>ChatbotReglasService</c>) usa reglas por
/// palabras clave, sin IA. El día que se conecte Azure OpenAI u otro
/// proveedor, basta con crear una nueva clase que implemente esta misma
/// interfaz (ver <c>IChatbotIaProvider</c> más abajo) y registrarla en
/// Program.cs — el resto del sistema (ChatService, Hub, Controller, UI) no
/// cambia en absoluto.
/// </summary>
public interface IChatbotMotorRespuestas
{
    /// <param name="rolUsuario">"Candidato", "Empresa" o "Visitante".</param>
    Task<ChatRespuestaBotDto> ObtenerRespuestaAsync(string mensajeUsuario, string rolUsuario, string? usuarioId);
}

/// <summary>
/// Punto de extensión para conectar un proveedor de IA real (Azure OpenAI u
/// otro compatible) más adelante. No se usa todavía — se deja definida para
/// que la integración futura no requiera tocar el resto del módulo.
/// </summary>
public interface IChatbotIaProvider
{
    Task<string> GenerarRespuestaAsync(string mensajeUsuario, string contextoSistema);
}

public interface IChatService
{
    Task<ChatConversacionDto> ObtenerOCrearConversacionAsync(int? conversacionId, string? usuarioId, string rolUsuario);
    Task<ChatConversacionDto> EnviarMensajeAsync(ChatEnviarMensajeDto dto, string? usuarioId, string rolUsuario, string nombreContacto, string correoContacto);
    Task<int> EscalarASoporteAsync(int conversacionId, string nombreContacto, string correoContacto);
}

public interface IRealTimeChatBotNotifier
{
    Task NotificarNuevoMensajeAsync(int conversacionId, ChatMensajeDto mensaje);
}
