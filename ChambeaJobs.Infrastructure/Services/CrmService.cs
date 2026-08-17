using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;
using ChambeaJobs.Domain.Enums;
using ChambeaJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ChambeaJobs.Infrastructure.Services;

/// <inheritdoc cref="ICrmService"/>
public class CrmService : ICrmService
{
    private readonly ApplicationDbContext _db;

    private static readonly Dictionary<EtapaPipelineCrm, string> Etiquetas = new()
    {
        [EtapaPipelineCrm.Prospecto] = "Prospecto",
        [EtapaPipelineCrm.PrimerContacto] = "Primer contacto",
        [EtapaPipelineCrm.PresentacionRealizada] = "Presentación realizada",
        [EtapaPipelineCrm.Demostracion] = "Demostración",
        [EtapaPipelineCrm.Negociacion] = "Negociación",
        [EtapaPipelineCrm.EnEspera] = "En espera",
        [EtapaPipelineCrm.EmpresaVerificada] = "Empresa verificada",
        [EtapaPipelineCrm.EmpresaActiva] = "Empresa activa",
        [EtapaPipelineCrm.ClienteActivo] = "Cliente activo",
        [EtapaPipelineCrm.ClienteInactivo] = "Cliente inactivo",
        [EtapaPipelineCrm.ClienteCancelado] = "Cliente cancelado",
        [EtapaPipelineCrm.Cancelado] = "Cancelado"
    };

    /// <summary>Orden del "carril automático" (registro → verificada → activa
    /// → cliente activo). Se usa solo para decidir si un evento automático
    /// representa un avance real — nunca hace retroceder una etapa que el
    /// administrador haya fijado más adelante a mano.</summary>
    private static readonly Dictionary<EtapaPipelineCrm, int> OrdenAutomatico = new()
    {
        [EtapaPipelineCrm.Prospecto] = 1,
        [EtapaPipelineCrm.EmpresaVerificada] = 2,
        [EtapaPipelineCrm.EmpresaActiva] = 3,
        [EtapaPipelineCrm.ClienteActivo] = 4
    };

    public CrmService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<CrmPipelineColumnaDto>> ObtenerPipelineAsync()
    {
        var empresas = await _db.CrmEmpresas
            .Include(e => e.Actividades)
            .OrderByDescending(e => e.FechaCreacion)
            .ToListAsync();

        return Enum.GetValues<EtapaPipelineCrm>()
            .Select(etapa => new CrmPipelineColumnaDto
            {
                Etapa = etapa,
                EtiquetaEtapa = Etiquetas[etapa],
                Empresas = empresas
                    .Where(e => e.Etapa == etapa)
                    .Select(MapearListItem)
                    .ToList()
            })
            .ToList();
    }

    public async Task<List<CrmEmpresaListItemDto>> ListarAsync(string? busqueda, EtapaPipelineCrm? etapa)
    {
        var query = _db.CrmEmpresas.Include(e => e.Actividades).AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim().ToLower();
            query = query.Where(e =>
                e.NombreEmpresa.ToLower().Contains(termino) ||
                (e.ContactoPrincipal != null && e.ContactoPrincipal.ToLower().Contains(termino)) ||
                (e.Correo != null && e.Correo.ToLower().Contains(termino)));
        }

        if (etapa.HasValue)
        {
            query = query.Where(e => e.Etapa == etapa.Value);
        }

        var empresas = await query.OrderByDescending(e => e.FechaCreacion).ToListAsync();
        return empresas.Select(MapearListItem).ToList();
    }

    public async Task<CrmEmpresaDetalleDto?> ObtenerDetalleAsync(int id)
    {
        var empresa = await _db.CrmEmpresas
            .Include(e => e.Actividades)
            .Include(e => e.ArchivosAdjuntos)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (empresa is null) return null;

        string? planActual = null;
        DateTime? planVencimiento = null;
        string? planEstado = null;

        if (empresa.EmpresaId.HasValue)
        {
            var paquete = await _db.PaquetesEmpresa
                .Include(p => p.PlanSuscripcion)
                .Where(p => p.EmpresaId == empresa.EmpresaId.Value)
                .OrderByDescending(p => p.FechaCompra)
                .FirstOrDefaultAsync();

            if (paquete is not null)
            {
                planActual = paquete.PlanSuscripcion?.Nombre;
                planVencimiento = paquete.FechaVencimiento;
                planEstado = paquete.Estado;
            }
        }

        return new CrmEmpresaDetalleDto
        {
            Id = empresa.Id,
            EmpresaId = empresa.EmpresaId,
            NombreEmpresa = empresa.NombreEmpresa,
            Etapa = empresa.Etapa,
            ContactoPrincipal = empresa.ContactoPrincipal,
            Telefono = empresa.Telefono,
            Correo = empresa.Correo,
            RUC = empresa.RUC,
            Direccion = empresa.Direccion,
            SectorEmpresarial = empresa.SectorEmpresarial,
            TamanoEmpresa = empresa.TamanoEmpresa,
            SitioWeb = empresa.SitioWeb,
            RedesSociales = empresa.RedesSociales,
            Observaciones = empresa.Observaciones,
            FechaCreacion = empresa.FechaCreacion,
            PlanActual = planActual,
            PlanFechaVencimiento = planVencimiento,
            PlanEstado = planEstado,
            Actividades = empresa.Actividades
                .OrderByDescending(a => a.FechaActividad)
                .Select(a => new CrmActividadDto
                {
                    Id = a.Id,
                    Tipo = a.Tipo,
                    Descripcion = a.Descripcion,
                    FechaActividad = a.FechaActividad,
                    UsuarioNombre = a.UsuarioNombre
                })
                .ToList(),
            ArchivosAdjuntos = empresa.ArchivosAdjuntos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new CrmArchivoAdjuntoDto
                {
                    Id = a.Id,
                    NombreOriginal = a.NombreOriginal,
                    RutaArchivo = a.RutaArchivo,
                    Descripcion = a.Descripcion,
                    FechaSubida = a.FechaSubida
                })
                .ToList()
        };
    }

    public async Task<int> CrearAsync(CrmEmpresaFormDto dto, string usuarioCreadorId)
    {
        var entidad = new CrmEmpresa
        {
            EmpresaId = dto.EmpresaId,
            NombreEmpresa = dto.NombreEmpresa.Trim(),
            Etapa = dto.Etapa,
            ContactoPrincipal = dto.ContactoPrincipal,
            Telefono = dto.Telefono,
            Correo = dto.Correo,
            RUC = dto.RUC,
            Direccion = dto.Direccion,
            SectorEmpresarial = dto.SectorEmpresarial,
            TamanoEmpresa = dto.TamanoEmpresa,
            SitioWeb = dto.SitioWeb,
            RedesSociales = dto.RedesSociales,
            Observaciones = dto.Observaciones,
            UsuarioCreadorId = usuarioCreadorId,
            FechaCreacion = DateTime.UtcNow
        };

        _db.CrmEmpresas.Add(entidad);
        await _db.SaveChangesAsync();
        return entidad.Id;
    }

    public async Task ActualizarAsync(int id, CrmEmpresaFormDto dto)
    {
        var entidad = await _db.CrmEmpresas.FindAsync(id)
            ?? throw new InvalidOperationException("No se encontró el registro CRM.");

        entidad.EmpresaId = dto.EmpresaId;
        entidad.NombreEmpresa = dto.NombreEmpresa.Trim();
        entidad.Etapa = dto.Etapa;
        entidad.ContactoPrincipal = dto.ContactoPrincipal;
        entidad.Telefono = dto.Telefono;
        entidad.Correo = dto.Correo;
        entidad.RUC = dto.RUC;
        entidad.Direccion = dto.Direccion;
        entidad.SectorEmpresarial = dto.SectorEmpresarial;
        entidad.TamanoEmpresa = dto.TamanoEmpresa;
        entidad.SitioWeb = dto.SitioWeb;
        entidad.RedesSociales = dto.RedesSociales;
        entidad.Observaciones = dto.Observaciones;
        entidad.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task MoverEtapaAsync(CrmMoverEtapaDto dto)
    {
        var entidad = await _db.CrmEmpresas.FindAsync(dto.CrmEmpresaId)
            ?? throw new InvalidOperationException("No se encontró el registro CRM.");

        entidad.Etapa = dto.NuevaEtapa;
        entidad.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var entidad = await _db.CrmEmpresas.FindAsync(id);
        if (entidad is null) return;

        _db.CrmEmpresas.Remove(entidad);
        await _db.SaveChangesAsync();
    }

    public async Task<int> RegistrarActividadAsync(int crmEmpresaId, CrmActividadFormDto dto, string usuarioId, string usuarioNombre)
    {
        var existe = await _db.CrmEmpresas.AnyAsync(e => e.Id == crmEmpresaId);
        if (!existe) throw new InvalidOperationException("No se encontró el registro CRM.");

        var actividad = new CrmActividad
        {
            CrmEmpresaId = crmEmpresaId,
            Tipo = dto.Tipo,
            Descripcion = dto.Descripcion.Trim(),
            FechaActividad = dto.FechaActividad,
            UsuarioId = usuarioId,
            UsuarioNombre = usuarioNombre,
            FechaCreacion = DateTime.UtcNow
        };

        _db.CrmActividades.Add(actividad);
        await _db.SaveChangesAsync();
        return actividad.Id;
    }

    public async Task SincronizarDesdeRegistroEmpresaAsync(int empresaId)
    {
        var yaExiste = await _db.CrmEmpresas.AnyAsync(e => e.EmpresaId == empresaId);
        if (yaExiste) return;

        var empresa = await _db.Empresas.FirstOrDefaultAsync(e => e.Id == empresaId);
        if (empresa is null) return;

        _db.CrmEmpresas.Add(new CrmEmpresa
        {
            EmpresaId = empresa.Id,
            NombreEmpresa = empresa.NombreEmpresa,
            Etapa = EtapaPipelineCrm.Prospecto,
            RUC = empresa.RUC,
            SitioWeb = empresa.SitioWeb,
            UsuarioCreadorId = "sistema",
            FechaCreacion = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task<CrmReporteDto> ObtenerReporteAsync()
    {
        var empresas = await _db.CrmEmpresas.ToListAsync();
        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var inicioAnio = new DateTime(DateTime.UtcNow.Year, 1, 1);

        var ingresosMes = await _db.Pagos
            .Where(p => p.EstadoPago == EstadosPago.Aprobado && p.FechaPago >= inicioMes)
            .SumAsync(p => (decimal?)p.Monto) ?? 0m;

        var ingresosAnio = await _db.Pagos
            .Where(p => p.EstadoPago == EstadosPago.Aprobado && p.FechaPago >= inicioAnio)
            .SumAsync(p => (decimal?)p.Monto) ?? 0m;

        var vacantesPublicadasTotal = await _db.Vacantes.CountAsync();
        var candidatosRegistrados = await _db.Candidatos.CountAsync();
        var postulacionesTotal = await _db.Postulaciones.CountAsync();

        var totalProspectosHistorico = empresas.Count;
        var totalConvertidos = empresas.Count(e => e.Etapa is EtapaPipelineCrm.ClienteActivo or EtapaPipelineCrm.ClienteInactivo);
        var tasaConversion = totalProspectosHistorico == 0 ? 0 : Math.Round((double)totalConvertidos / totalProspectosHistorico * 100, 1);

        var planesMasVendidos = await _db.PaquetesEmpresa
            .Include(p => p.PlanSuscripcion)
            .Where(p => p.PlanSuscripcion != null)
            .GroupBy(p => p.PlanSuscripcion!.Nombre)
            .Select(g => new CrmReporteConteoDto { Etiqueta = g.Key, Cantidad = g.Count() })
            .OrderByDescending(g => g.Cantidad)
            .ToListAsync();

        return new CrmReporteDto
        {
            TotalEmpresas = empresas.Count,
            TotalClientesActivos = empresas.Count(e => e.Etapa == EtapaPipelineCrm.ClienteActivo),
            TotalClientesInactivos = empresas.Count(e => e.Etapa == EtapaPipelineCrm.ClienteInactivo),
            TotalProspectos = empresas.Count(e => e.Etapa == EtapaPipelineCrm.Prospecto),
            IngresosMesActual = ingresosMes,
            IngresosAnioActual = ingresosAnio,
            VacantesPublicadasTotal = vacantesPublicadasTotal,
            CandidatosRegistrados = candidatosRegistrados,
            PostulacionesTotal = postulacionesTotal,
            TasaConversionProspectoACliente = tasaConversion,
            PorEtapa = Enum.GetValues<EtapaPipelineCrm>()
                .Select(et => new CrmReporteConteoDto { Etiqueta = Etiquetas[et], Cantidad = empresas.Count(e => e.Etapa == et) })
                .Where(c => c.Cantidad > 0)
                .ToList(),
            PorSector = empresas
                .Where(e => !string.IsNullOrWhiteSpace(e.SectorEmpresarial))
                .GroupBy(e => e.SectorEmpresarial!)
                .Select(g => new CrmReporteConteoDto { Etiqueta = g.Key, Cantidad = g.Count() })
                .OrderByDescending(g => g.Cantidad)
                .ToList(),
            PlanesMasVendidos = planesMasVendidos,
            Alertas = await ObtenerAlertasAsync(),
            ResumenIngresos = await ObtenerResumenIngresosAsync()
        };
    }

    public async Task<CrmResumenIngresosDto> ObtenerResumenIngresosAsync()
    {
        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var inicioAnio = new DateTime(DateTime.UtcNow.Year, 1, 1);

        var pagosDelAnio = await _db.Pagos
            .Include(p => p.PaqueteEmpresa).ThenInclude(pe => pe!.PlanSuscripcion)
            .Include(p => p.PaqueteEmpresa).ThenInclude(pe => pe!.Empresa)
            .Where(p => p.EstadoPago == EstadosPago.Aprobado && p.FechaPago >= inicioAnio)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();

        static CrmDetallePagoDto Mapear(Pago p) => new()
        {
            NombreEmpresa = p.PaqueteEmpresa?.Empresa?.NombreEmpresa ?? "—",
            RUC = p.PaqueteEmpresa?.Empresa?.RUC,
            NombrePlan = p.PaqueteEmpresa?.PlanSuscripcion?.Nombre ?? "—",
            Monto = p.Monto,
            FechaPago = p.FechaPago
        };

        var pagosDelMes = pagosDelAnio.Where(p => p.FechaPago >= inicioMes).ToList();

        var resumenPorPlanMes = pagosDelMes
            .GroupBy(p => p.PaqueteEmpresa?.PlanSuscripcion?.Nombre ?? "—")
            .Select(g => new CrmResumenPorPlanDto
            {
                NombrePlan = g.Key,
                CantidadEmpresas = g.Count(),
                MontoPorPago = g.First().Monto,
                TotalPlan = g.Sum(p => p.Monto)
            })
            .OrderByDescending(g => g.TotalPlan)
            .ToList();

        return new CrmResumenIngresosDto
        {
            TotalMes = pagosDelMes.Sum(p => p.Monto),
            TotalAnio = pagosDelAnio.Sum(p => p.Monto),
            PagosDelMes = pagosDelMes.Select(Mapear).ToList(),
            PagosDelAnio = pagosDelAnio.Select(Mapear).ToList(),
            ResumenPorPlanMes = resumenPorPlanMes
        };
    }

    public async Task<List<CrmAgendaItemDto>> ObtenerAgendaAsync(DateTime desde, DateTime hasta)
    {
        return await _db.CrmActividades
            .Include(a => a.CrmEmpresa)
            .Where(a => a.FechaActividad >= desde && a.FechaActividad <= hasta)
            .OrderBy(a => a.FechaActividad)
            .Select(a => new CrmAgendaItemDto
            {
                CrmEmpresaId = a.CrmEmpresaId,
                NombreEmpresa = a.CrmEmpresa.NombreEmpresa,
                ActividadId = a.Id,
                Tipo = a.Tipo,
                Descripcion = a.Descripcion,
                FechaActividad = a.FechaActividad,
                UsuarioNombre = a.UsuarioNombre
            })
            .ToListAsync();
    }

    public async Task AgregarArchivoAdjuntoAsync(int crmEmpresaId, string nombreOriginal, string rutaArchivo, string? descripcion, string usuarioId)
    {
        var existe = await _db.CrmEmpresas.AnyAsync(e => e.Id == crmEmpresaId);
        if (!existe) throw new InvalidOperationException("No se encontró el registro CRM.");

        _db.CrmArchivosAdjuntos.Add(new CrmArchivoAdjunto
        {
            CrmEmpresaId = crmEmpresaId,
            NombreOriginal = nombreOriginal,
            RutaArchivo = rutaArchivo,
            Descripcion = descripcion,
            UsuarioId = usuarioId,
            FechaSubida = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task EliminarArchivoAdjuntoAsync(int archivoId)
    {
        var archivo = await _db.CrmArchivosAdjuntos.FindAsync(archivoId);
        if (archivo is null) return;

        _db.CrmArchivosAdjuntos.Remove(archivo);
        await _db.SaveChangesAsync();
    }

    public async Task MarcarClienteInactivoSiCorrespondeAsync(int empresaId)
    {
        var crm = await _db.CrmEmpresas.FirstOrDefaultAsync(c => c.EmpresaId == empresaId);
        if (crm is null || crm.Etapa != EtapaPipelineCrm.ClienteActivo) return;

        crm.Etapa = EtapaPipelineCrm.ClienteInactivo;
        crm.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    /// <summary>Copia al CRM los datos que la empresa llenó en su propio
    /// perfil (sector, contacto, teléfono, tamaño, redes) — así el admin no
    /// tiene que volver a escribirlos a mano. Si la empresa todavía no tiene
    /// ficha CRM (caso raro, se sincroniza también al registrarse), no hace
    /// nada.</summary>
    public async Task SincronizarDatosDesdeEmpresaAsync(int empresaId)
    {
        var crm = await _db.CrmEmpresas.FirstOrDefaultAsync(c => c.EmpresaId == empresaId);
        if (crm is null) return;

        var empresa = await _db.Empresas.FirstOrDefaultAsync(e => e.Id == empresaId);
        if (empresa is null) return;

        crm.NombreEmpresa = empresa.NombreEmpresa;
        crm.RUC = empresa.RUC;
        crm.SitioWeb = empresa.SitioWeb;
        crm.SectorEmpresarial = empresa.SectorEmpresarial;
        crm.TamanoEmpresa = empresa.NumeroColaboradores;

        // Solo se pisa el contacto/teléfono si la empresa realmente cargó un
        // valor — si el admin ya había anotado un contacto distinto a mano
        // (ej. alguien de ventas con quien habló), no se lo borra con vacío.
        if (!string.IsNullOrWhiteSpace(empresa.NombreContacto)) crm.ContactoPrincipal = empresa.NombreContacto;
        if (!string.IsNullOrWhiteSpace(empresa.TelefonoContacto)) crm.Telefono = empresa.TelefonoContacto;

        var redes = new[] { empresa.FacebookUrl, empresa.InstagramUrl, empresa.LinkedInUrl, empresa.TiktokUrl }
            .Where(r => !string.IsNullOrWhiteSpace(r));
        var redesTexto = string.Join(" | ", redes);
        if (!string.IsNullOrWhiteSpace(redesTexto)) crm.RedesSociales = redesTexto;

        crm.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task AvanzarEtapaAutomaticaAsync(int empresaId, EtapaPipelineCrm etapaMinima)
    {
        if (!OrdenAutomatico.ContainsKey(etapaMinima)) return; // solo aplica al carril automático

        var crm = await _db.CrmEmpresas.FirstOrDefaultAsync(c => c.EmpresaId == empresaId);
        if (crm is null) return;

        var ordenActual = OrdenAutomatico.TryGetValue(crm.Etapa, out var o) ? o : 0;
        var ordenNuevo = OrdenAutomatico[etapaMinima];

        if (ordenNuevo <= ordenActual) return; // no es un avance, no se toca

        crm.Etapa = etapaMinima;
        crm.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<CrmEstadisticasEmpresaDto> ObtenerEstadisticasAsync(int empresaId)
    {
        var usuarioId = await _db.Empresas.Where(e => e.Id == empresaId).Select(e => e.UsuarioId).FirstOrDefaultAsync();

        var vacantesActivas = await _db.Vacantes.CountAsync(v => v.EmpresaId == empresaId && v.Estado == "Activa");
        var vacantesCerradas = await _db.Vacantes.CountAsync(v => v.EmpresaId == empresaId && v.Estado == "Cerrada");
        var ultimaVacante = await _db.Vacantes.Where(v => v.EmpresaId == empresaId)
            .OrderByDescending(v => v.FechaPublicacion).Select(v => (DateTime?)v.FechaPublicacion).FirstOrDefaultAsync();

        var postulacionesRecibidas = await _db.Postulaciones.CountAsync(p => p.Vacante!.EmpresaId == empresaId);
        var contratados = await _db.Postulaciones.CountAsync(p => p.Vacante!.EmpresaId == empresaId && p.EstadoPostulacion!.Nombre == ChambeaJobs.Domain.Entities.EstadoPostulacion.Nombres.Contratado);

        var totalInvertido = await _db.Pagos
            .Where(p => p.PaqueteEmpresa!.EmpresaId == empresaId && p.EstadoPago == EstadosPago.Aprobado)
            .SumAsync(p => (decimal?)p.Monto) ?? 0m;

        var ultimaCompra = await _db.Pagos
            .Where(p => p.PaqueteEmpresa!.EmpresaId == empresaId && p.EstadoPago == EstadosPago.Aprobado)
            .OrderByDescending(p => p.FechaPago).Select(p => (DateTime?)p.FechaPago).FirstOrDefaultAsync();

        var ultimoLogin = usuarioId is null
            ? null
            : await _db.Users.Where(u => u.Id == usuarioId).Select(u => u.UltimoInicioSesion).FirstOrDefaultAsync();

        return new CrmEstadisticasEmpresaDto
        {
            VacantesActivas = vacantesActivas,
            VacantesCerradas = vacantesCerradas,
            UltimaVacantePublicada = ultimaVacante,
            PostulacionesRecibidas = postulacionesRecibidas,
            CandidatosContratados = contratados,
            TotalInvertido = totalInvertido,
            UltimaCompra = ultimaCompra,
            UltimoInicioSesion = ultimoLogin
        };
    }

    public async Task<List<CrmAlertaDto>> ObtenerAlertasAsync()
    {
        var alertas = new List<CrmAlertaDto>();
        var ahora = DateTime.UtcNow;

        var empresasCrm = await _db.CrmEmpresas
            .Where(c => c.EmpresaId != null && c.Etapa != EtapaPipelineCrm.Cancelado && c.Etapa != EtapaPipelineCrm.ClienteCancelado)
            .ToListAsync();

        foreach (var crm in empresasCrm)
        {
            var empresaId = crm.EmpresaId!.Value;

            // 1) Perfil incompleto: sigue como Prospecto más de 7 días después de registrarse.
            if (crm.Etapa == EtapaPipelineCrm.Prospecto && (ahora - crm.FechaCreacion).TotalDays >= 7)
            {
                alertas.Add(new CrmAlertaDto
                {
                    CrmEmpresaId = crm.Id,
                    NombreEmpresa = crm.NombreEmpresa,
                    Tipo = "PerfilIncompleto",
                    Mensaje = $"{crm.NombreEmpresa} se registró hace más de 7 días y todavía no completó su perfil (sin logo/descripción).",
                    Severidad = "info"
                });
            }

            // 2) Inactividad: 30+ días sin iniciar sesión (o nunca ha entrado y ya
            // pasaron 30 días desde el registro).
            var usuarioId = await _db.Empresas.Where(e => e.Id == empresaId).Select(e => e.UsuarioId).FirstOrDefaultAsync();
            var ultimoLogin = usuarioId is null ? null : await _db.Users.Where(u => u.Id == usuarioId).Select(u => u.UltimoInicioSesion).FirstOrDefaultAsync();
            var referencia = ultimoLogin ?? crm.FechaCreacion;
            if ((ahora - referencia).TotalDays >= 30 && crm.Etapa is EtapaPipelineCrm.EmpresaActiva or EtapaPipelineCrm.ClienteActivo)
            {
                alertas.Add(new CrmAlertaDto
                {
                    CrmEmpresaId = crm.Id,
                    NombreEmpresa = crm.NombreEmpresa,
                    Tipo = "Inactividad",
                    Mensaje = $"{crm.NombreEmpresa} no inicia sesión hace 30+ días.",
                    Severidad = "warning"
                });
            }

            // 3) Postulaciones sin revisar.
            var sinRevisar = await _db.Postulaciones.CountAsync(p => p.Vacante!.EmpresaId == empresaId && !p.CvRevisado);
            if (sinRevisar > 0)
            {
                alertas.Add(new CrmAlertaDto
                {
                    CrmEmpresaId = crm.Id,
                    NombreEmpresa = crm.NombreEmpresa,
                    Tipo = "PostulacionesPendientes",
                    Mensaje = $"{crm.NombreEmpresa} tiene {sinRevisar} postulación(es) sin revisar.",
                    Severidad = "info"
                });
            }

            // 4) Plan próximo a vencer (7 días o menos).
            var paqueteVigente = await _db.PaquetesEmpresa
                .Include(p => p.PlanSuscripcion)
                .Where(p => p.EmpresaId == empresaId && p.Estado == EstadosPaquete.Vigente)
                .OrderByDescending(p => p.FechaVencimiento)
                .FirstOrDefaultAsync();

            if (paqueteVigente is not null)
            {
                var diasRestantes = (paqueteVigente.FechaVencimiento.Date - ahora.Date).Days;
                if (diasRestantes is >= 0 and <= 7)
                {
                    alertas.Add(new CrmAlertaDto
                    {
                        CrmEmpresaId = crm.Id,
                        NombreEmpresa = crm.NombreEmpresa,
                        Tipo = "PlanPorVencer",
                        Mensaje = $"{crm.NombreEmpresa}: su plan vence en {diasRestantes} día(s).",
                        Severidad = "warning"
                    });
                }

                // 5) Candidata a plan superior: ya consumió el 80%+ de su cupo.
                if (paqueteVigente.VacantesIncluidas.HasValue && paqueteVigente.VacantesIncluidas.Value > 0)
                {
                    var porcentajeUsado = (double)paqueteVigente.VacantesConsumidas / paqueteVigente.VacantesIncluidas.Value;
                    if (porcentajeUsado >= 0.8)
                    {
                        alertas.Add(new CrmAlertaDto
                        {
                            CrmEmpresaId = crm.Id,
                            NombreEmpresa = crm.NombreEmpresa,
                            Tipo = "CandidataPlanSuperior",
                            Mensaje = $"{crm.NombreEmpresa} ya usó {paqueteVigente.VacantesConsumidas}/{paqueteVigente.VacantesIncluidas} vacantes de su plan — candidata a un plan superior.",
                            Severidad = "info"
                        });
                    }
                }
            }
        }

        return alertas;
    }

    public async Task<byte[]> GenerarReportePdfAsync(CrmReporteExportOpcionesDto opciones)
    {
        var reporte = await ObtenerReporteAsync();
        var empresas = opciones.IncluirListaEmpresas ? await ListarAsync(null, null) : new List<CrmEmpresaListItemDto>();

        QuestPDF.Settings.License = LicenseType.Community;

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(36);
                pagina.DefaultTextStyle(estilo => estilo.FontFamily("Helvetica").FontSize(9));

                pagina.Header().Column(encabezado =>
                {
                    encabezado.Item().Row(fila =>
                    {
                        fila.RelativeItem().Column(marca =>
                        {
                            marca.Item().Text("ChambeaJobs").FontSize(20).Bold().FontColor("#14497A");
                            marca.Item().Text("Reporte del CRM").FontSize(9).FontColor("#616B7A");
                        });
                        fila.ConstantItem(160).AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(9).FontColor("#8892A0");
                    });
                    encabezado.Item().PaddingTop(8).LineHorizontal(1).LineColor("#E5E8EC");
                });

                pagina.Content().PaddingVertical(16).Column(cuerpo =>
                {
                    if (opciones.IncluirRendimiento)
                    {
                        cuerpo.Item().Text("Rendimiento comercial").Bold().FontSize(12).FontColor("#14497A");
                        cuerpo.Item().PaddingTop(4).PaddingBottom(10).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                            void Celda(string etiqueta, string valor)
                            {
                                tabla.Cell().Padding(6).Background("#F1F3F5").Column(col =>
                                {
                                    col.Item().Text(valor).Bold().FontSize(13);
                                    col.Item().Text(etiqueta).FontSize(8).FontColor("#616B7A");
                                });
                            }
                            Celda("Total empresas", reporte.TotalEmpresas.ToString());
                            Celda("Clientes activos", reporte.TotalClientesActivos.ToString());
                            Celda("Clientes inactivos", reporte.TotalClientesInactivos.ToString());
                            Celda("Tasa de conversión", $"{reporte.TasaConversionProspectoACliente}%");
                        });
                    }

                    if (opciones.IncluirIngresos)
                    {
                        var ri = reporte.ResumenIngresos;

                        cuerpo.Item().Text("Ingresos y actividad").Bold().FontSize(12).FontColor("#14497A");
                        cuerpo.Item().PaddingTop(4).PaddingBottom(6).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });
                            void Fila(string etiqueta, string valor)
                            {
                                tabla.Cell().Padding(5).Background("#F1F3F5").Text(etiqueta).FontColor("#616B7A");
                                tabla.Cell().Padding(5).Text(valor);
                            }
                            Fila("Ingresos del mes", $"${reporte.IngresosMesActual:0.00}");
                            Fila("Ingresos del año", $"${reporte.IngresosAnioActual:0.00}");
                            Fila("Vacantes publicadas", reporte.VacantesPublicadasTotal.ToString());
                            Fila("Candidatos registrados", reporte.CandidatosRegistrados.ToString());
                            Fila("Postulaciones totales", reporte.PostulacionesTotal.ToString());
                        });

                        // Frase-resumen tipo "2 empresas pagaron el plan Empresarial
                        // ($50.00 c/u = $100.00) y 1 empresa pagó el plan Básico
                        // ($20.00)." — el respaldo textual que se pide, además de
                        // la tabla detallada de cada pago que sigue abajo.
                        if (ri.ResumenPorPlanMes.Any())
                        {
                            var partes = ri.ResumenPorPlanMes.Select(r =>
                                $"{r.CantidadEmpresas} empresa{(r.CantidadEmpresas != 1 ? "s" : "")} " +
                                $"pag{(r.CantidadEmpresas != 1 ? "aron" : "ó")} el plan {r.NombrePlan} " +
                                $"(${r.MontoPorPago:0.00} c/u = ${r.TotalPlan:0.00})");
                            var frase = "Explicación de los ingresos de este mes: " + string.Join("; ", partes) +
                                $". Total del mes: ${ri.TotalMes:0.00}.";

                            cuerpo.Item().PaddingBottom(10).Background("#EAF1F8").Padding(10)
                                .Text(frase).FontSize(9).FontColor("#14497A");
                        }

                        if (ri.PagosDelMes.Any())
                        {
                            cuerpo.Item().PaddingTop(4).Text("Detalle de cada pago aprobado este mes (respaldo para declaración/revisión de la DGI)").Bold().FontSize(9.5f).FontColor("#14497A");
                            cuerpo.Item().PaddingTop(4).PaddingBottom(10).Table(tabla =>
                            {
                                tabla.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); });
                                tabla.Header(h =>
                                {
                                    void Encabezado(string texto) => h.Cell().Padding(5).Background("#14497A").Text(texto).FontColor("#fff").Bold().FontSize(8);
                                    Encabezado("Empresa");
                                    Encabezado("RUC");
                                    Encabezado("Plan");
                                    Encabezado("Monto");
                                    Encabezado("Fecha de pago");
                                });
                                foreach (var p in ri.PagosDelMes)
                                {
                                    tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(p.NombreEmpresa).FontSize(8);
                                    tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(p.RUC ?? "—").FontSize(8);
                                    tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(p.NombrePlan).FontSize(8);
                                    tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text($"${p.Monto:0.00}").FontSize(8);
                                    tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(p.FechaPago.ToString("dd/MM/yyyy")).FontSize(8);
                                }
                                tabla.Cell().ColumnSpan(3).Padding(5).Background("#F1F3F5").AlignRight().Text("Total del mes:").Bold().FontSize(8.5f);
                                tabla.Cell().ColumnSpan(2).Padding(5).Background("#F1F3F5").Text($"${ri.TotalMes:0.00}").Bold().FontSize(8.5f);
                            });
                        }

                        if (ri.PagosDelAnio.Any())
                        {
                            cuerpo.Item().PaddingTop(4).Text($"Detalle de todos los pagos aprobados del año ({ri.PagosDelAnio.Count})").Bold().FontSize(9.5f).FontColor("#14497A");
                            cuerpo.Item().PaddingTop(4).PaddingBottom(10).Table(tabla =>
                            {
                                tabla.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); });
                                tabla.Header(h =>
                                {
                                    void Encabezado(string texto) => h.Cell().Padding(5).Background("#14497A").Text(texto).FontColor("#fff").Bold().FontSize(8);
                                    Encabezado("Empresa");
                                    Encabezado("RUC");
                                    Encabezado("Plan");
                                    Encabezado("Monto");
                                    Encabezado("Fecha de pago");
                                });
                                foreach (var p in ri.PagosDelAnio)
                                {
                                    tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(p.NombreEmpresa).FontSize(8);
                                    tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(p.RUC ?? "—").FontSize(8);
                                    tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(p.NombrePlan).FontSize(8);
                                    tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text($"${p.Monto:0.00}").FontSize(8);
                                    tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(p.FechaPago.ToString("dd/MM/yyyy")).FontSize(8);
                                }
                                tabla.Cell().ColumnSpan(3).Padding(5).Background("#F1F3F5").AlignRight().Text("Total del año:").Bold().FontSize(8.5f);
                                tabla.Cell().ColumnSpan(2).Padding(5).Background("#F1F3F5").Text($"${ri.TotalAnio:0.00}").Bold().FontSize(8.5f);
                            });
                        }
                    }

                    if (opciones.IncluirSectores && reporte.PorSector.Any())
                    {
                        cuerpo.Item().Text("Empresas por sector").Bold().FontSize(12).FontColor("#14497A");
                        cuerpo.Item().PaddingTop(4).PaddingBottom(10).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(1); });
                            foreach (var s in reporte.PorSector)
                            {
                                tabla.Cell().Padding(5).Text(s.Etiqueta);
                                tabla.Cell().Padding(5).AlignRight().Text(s.Cantidad.ToString());
                            }
                        });
                    }

                    if (opciones.IncluirAlertas && reporte.Alertas.Any())
                    {
                        cuerpo.Item().Text($"Alertas automáticas ({reporte.Alertas.Count})").Bold().FontSize(12).FontColor("#14497A");
                        cuerpo.Item().PaddingTop(4).PaddingBottom(10).Column(col =>
                        {
                            foreach (var a in reporte.Alertas)
                            {
                                col.Item().PaddingBottom(3).Text(t =>
                                {
                                    t.Span("• ").FontColor("#F0661E");
                                    t.Span(a.Mensaje).FontSize(9);
                                });
                            }
                        });
                    }

                    if (opciones.IncluirListaEmpresas)
                    {
                        cuerpo.Item().Text($"Listado de empresas ({empresas.Count})").Bold().FontSize(12).FontColor("#14497A");
                        cuerpo.Item().PaddingTop(4).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); });
                            tabla.Header(h =>
                            {
                                void Encabezado(string texto) => h.Cell().Padding(5).Background("#14497A").Text(texto).FontColor("#fff").Bold().FontSize(8);
                                Encabezado("Empresa");
                                Encabezado("Etapa");
                                Encabezado("Contacto");
                                Encabezado("Última actividad");
                            });
                            foreach (var e in empresas)
                            {
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(e.NombreEmpresa).FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(e.Etapa.ToString()).FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(e.ContactoPrincipal ?? "—").FontSize(8);
                                tabla.Cell().Padding(5).BorderBottom(1).BorderColor("#E5E8EC").Text(e.FechaUltimaActividad?.ToString("dd/MM/yyyy") ?? "—").FontSize(8);
                            }
                        });
                    }
                });

                pagina.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Generado automáticamente por ChambeaJobs CRM.").FontSize(8).FontColor("#8892A0");
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static CrmEmpresaListItemDto MapearListItem(CrmEmpresa e) => new()
    {
        Id = e.Id,
        NombreEmpresa = e.NombreEmpresa,
        Etapa = e.Etapa,
        ContactoPrincipal = e.ContactoPrincipal,
        Correo = e.Correo,
        Telefono = e.Telefono,
        EstaRegistradaEnPlataforma = e.EmpresaId.HasValue,
        FechaCreacion = e.FechaCreacion,
        FechaUltimaActividad = e.Actividades.Any() ? e.Actividades.Max(a => a.FechaActividad) : null
    };
}
