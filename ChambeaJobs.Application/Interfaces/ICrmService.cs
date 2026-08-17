using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Domain.Enums;

namespace ChambeaJobs.Application.Interfaces;

public interface ICrmService
{
    Task<List<CrmPipelineColumnaDto>> ObtenerPipelineAsync();
    Task<List<CrmEmpresaListItemDto>> ListarAsync(string? busqueda, EtapaPipelineCrm? etapa);
    Task<CrmEmpresaDetalleDto?> ObtenerDetalleAsync(int id);
    Task<int> CrearAsync(CrmEmpresaFormDto dto, string usuarioCreadorId);
    Task ActualizarAsync(int id, CrmEmpresaFormDto dto);
    Task MoverEtapaAsync(CrmMoverEtapaDto dto);
    Task EliminarAsync(int id);
    Task<int> RegistrarActividadAsync(int crmEmpresaId, CrmActividadFormDto dto, string usuarioId, string usuarioNombre);
    Task<CrmReporteDto> ObtenerReporteAsync();
    Task<CrmResumenIngresosDto> ObtenerResumenIngresosAsync();
    Task<List<CrmAgendaItemDto>> ObtenerAgendaAsync(DateTime desde, DateTime hasta);

    Task<byte[]> GenerarReportePdfAsync(CrmReporteExportOpcionesDto opciones);

    Task AgregarArchivoAdjuntoAsync(int crmEmpresaId, string nombreOriginal, string rutaArchivo, string? descripcion, string usuarioId);
    Task EliminarArchivoAdjuntoAsync(int archivoId);

    /// <summary>Mueve la ficha CRM de la empresa a <paramref name="etapaMinima"/>
    /// SOLO si eso representa un avance dentro del carril automático
    /// (Prospecto → Empresa verificada → Empresa activa → Cliente activo).
    /// No hace nada si la empresa no tiene ficha CRM, o si ya está más
    /// adelante (nunca retrocede por un evento automático).</summary>
    Task AvanzarEtapaAutomaticaAsync(int empresaId, EtapaPipelineCrm etapaMinima);

    Task SincronizarDatosDesdeEmpresaAsync(int empresaId);

    Task MarcarClienteInactivoSiCorrespondeAsync(int empresaId);

    Task<CrmEstadisticasEmpresaDto> ObtenerEstadisticasAsync(int empresaId);

    /// <summary>Situaciones que el CRM detecta solo, sin que nadie las capture
    /// a mano: empresa inactiva 30+ días, vacantes con postulaciones sin
    /// revisar, plan próximo a vencer, perfil incompleto, alto crecimiento,
    /// candidata a un plan superior.</summary>
    Task<List<CrmAlertaDto>> ObtenerAlertasAsync();

    /// <summary>Crea (o reutiliza si ya existe) la ficha CRM de una empresa que
    /// acaba de registrarse en la plataforma, para que aparezca automáticamente
    /// en el pipeline como "Cliente activo".</summary>
    Task SincronizarDesdeRegistroEmpresaAsync(int empresaId);
}
