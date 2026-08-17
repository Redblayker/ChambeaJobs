using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="IPaqueteEmpresaService"/>
public class PaqueteEmpresaService : IPaqueteEmpresaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificacionService _notificacionService;
    private readonly IAdminUsuarioService _adminUsuarioService;
    private readonly IAuditoriaService _auditoriaService;
    private readonly ChambeaJobs.Application.Eventos.IPublicadorEventos _publicadorEventos;

    public PaqueteEmpresaService(
        IUnitOfWork unitOfWork,
        INotificacionService notificacionService,
        IAdminUsuarioService adminUsuarioService,
        IAuditoriaService auditoriaService,
        ChambeaJobs.Application.Eventos.IPublicadorEventos publicadorEventos)
    {
        _unitOfWork = unitOfWork;
        _notificacionService = notificacionService;
        _adminUsuarioService = adminUsuarioService;
        _auditoriaService = auditoriaService;
        _publicadorEventos = publicadorEventos;
    }

    public async Task<PaqueteEstadoDto> ObtenerEstadoPaqueteAsync(int empresaId)
    {
        await ActualizarVencimientosAsync();

        var paquete = await _unitOfWork.PaquetesEmpresa.ObtenerVigentePorEmpresaAsync(empresaId);

        if (paquete is null)
        {
            return new PaqueteEstadoDto { TienePaquete = false, PuedePublicar = false, Estado = "Sin paquete" };
        }

        return new PaqueteEstadoDto
        {
            TienePaquete = true,
            VacantesIncluidas = paquete.VacantesIncluidas,
            VacantesConsumidas = paquete.VacantesConsumidas,
            FechaVencimiento = paquete.FechaVencimiento,
            DiasRestantes = (int)Math.Max(0, (paquete.FechaVencimiento - DateTime.UtcNow).TotalDays),
            Estado = paquete.Estado,
            PuedePublicar = paquete.TieneCupoDisponible(),
            NombrePlan = paquete.PlanSuscripcion?.Nombre,
            IncluyePruebaPsicometrica = paquete.PlanSuscripcion?.IncluyePruebaPsicometrica ?? false,
            IncluyeVideoCv = paquete.PlanSuscripcion?.IncluyeVideoCv ?? false,
            PermiteVacantesDestacadas = paquete.PlanSuscripcion?.PermiteVacantesDestacadas ?? false
        };
    }

    public async Task<List<PagoHistorialDto>> ObtenerHistorialAsync(int empresaId)
    {
        var paquetes = await _unitOfWork.PaquetesEmpresa.ObtenerPorEmpresaAsync(empresaId);

        return paquetes
            .OrderByDescending(p => p.FechaCompra)
            .Select(p => new PagoHistorialDto
            {
                PagoId = p.Pago?.Id ?? 0,
                PaqueteId = p.Id,
                FechaCompra = p.FechaCompra,
                Monto = p.Pago?.Monto ?? 0,
                VacantesIncluidas = p.VacantesIncluidas,
                VacantesConsumidas = p.VacantesConsumidas,
                FechaVencimiento = p.FechaVencimiento,
                EstadoPaquete = p.Estado,
                EstadoPago = p.Pago?.EstadoPago ?? "Desconocido",
                ReferenciaTransaccion = p.Pago?.ReferenciaTransaccion
            })
            .ToList();
    }

    public async Task RegistrarSolicitudPagoAsync(int empresaId, RegistrarPagoDto datos)
    {
        var plan = await _unitOfWork.PlanesSuscripcion.ObtenerPorIdAsync(datos.PlanSuscripcionId)
            ?? throw new InvalidOperationException("El plan seleccionado no es válido.");

        var nuevoPaquete = new PaqueteEmpresa
        {
            EmpresaId = empresaId,
            PlanSuscripcionId = plan.Id,
            FechaCompra = DateTime.UtcNow,
            // La vigencia real de N días se cuenta desde la APROBACIÓN del
            // administrador, no desde la solicitud (ver AprobarPagoAsync).
            // Se deja aquí un valor provisional hasta que se apruebe.
            FechaVencimiento = DateTime.UtcNow.AddDays(plan.DiasVigencia),
            VacantesIncluidas = plan.VacantesIncluidas,
            VacantesConsumidas = 0,
            Estado = EstadosPaquete.Pendiente,
            RenovacionAutomatica = true
        };

        await _unitOfWork.PaquetesEmpresa.AgregarAsync(nuevoPaquete);
        await _unitOfWork.GuardarCambiosAsync(); // para obtener el Id generado

        var pago = new Pago
        {
            PaqueteEmpresaId = nuevoPaquete.Id,
            Monto = plan.Precio,
            MetodoPago = "Transferencia/Depósito manual",
            ReferenciaTransaccion = datos.ReferenciaTransaccion,
            EstadoPago = EstadosPago.Pendiente,
            FechaPago = DateTime.UtcNow
        };

        await _unitOfWork.Pagos.AgregarAsync(pago);
        await _unitOfWork.GuardarCambiosAsync();

        // 🔔 Notificar a TODOS los administradores: hay un pago nuevo esperando revisión.
        var empresa = await _unitOfWork.Empresas.ObtenerPorIdAsync(empresaId);
        var nombreEmpresa = empresa?.NombreEmpresa ?? "Una empresa";
        var idsAdmins = await _adminUsuarioService.ObtenerIdsAdministradoresAsync();

        foreach (var adminId in idsAdmins)
        {
            await _notificacionService.CrearAsync(
                adminId,
                Notificacion.Tipos.PagoPendiente,
                $"💳 {nombreEmpresa} registró un pago de ${plan.Precio:0.00} pendiente de aprobación (ref. {datos.ReferenciaTransaccion}).",
                "/Admin/PagosPendientes");
        }
    }

    public async Task<List<PagoAdminDto>> ObtenerHistorialCompletoAsync(string? nombreEmpresa, string? estadoPago, DateTime? desde, DateTime? hasta)
    {
        var paquetes = await _unitOfWork.PaquetesEmpresa.ObtenerTodosConDetalleAsync();

        var consulta = paquetes.Where(p => p.Pago is not null).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(nombreEmpresa))
        {
            consulta = consulta.Where(p =>
                p.Empresa?.NombreEmpresa.Contains(nombreEmpresa, StringComparison.OrdinalIgnoreCase) == true);
        }

        if (!string.IsNullOrWhiteSpace(estadoPago))
        {
            consulta = consulta.Where(p => p.Pago!.EstadoPago == estadoPago);
        }

        if (desde.HasValue)
        {
            consulta = consulta.Where(p => p.Pago!.FechaPago.Date >= desde.Value.Date);
        }

        if (hasta.HasValue)
        {
            consulta = consulta.Where(p => p.Pago!.FechaPago.Date <= hasta.Value.Date);
        }

        return consulta
            .OrderByDescending(p => p.Pago!.FechaPago)
            .Select(p => new PagoAdminDto
            {
                PagoId = p.Pago!.Id,
                EmpresaNombre = p.Empresa?.NombreEmpresa ?? "(sin nombre)",
                FechaPago = p.Pago.FechaPago,
                Monto = p.Pago.Monto,
                EstadoPago = p.Pago.EstadoPago,
                ReferenciaTransaccion = p.Pago.ReferenciaTransaccion,
                FechaRevision = p.Pago.FechaRevision,
                ComentarioRevision = p.Pago.ComentarioRevision
            })
            .ToList();
    }

    public async Task<List<PagoPendienteDto>> ObtenerPagosPendientesAsync()
    {
        var paquetesPendientes = await _unitOfWork.PaquetesEmpresa.ObtenerPendientesDeAprobacionAsync();

        return paquetesPendientes
            .Where(p => p.Pago is not null)
            .OrderBy(p => p.FechaCompra)
            .Select(p => new PagoPendienteDto
            {
                PagoId = p.Pago!.Id,
                PaqueteId = p.Id,
                EmpresaNombre = p.Empresa?.NombreEmpresa ?? "(sin nombre)",
                EmpresaCorreo = string.Empty, // se completa en el controlador si se requiere vía UserManager
                Monto = p.Pago.Monto,
                ReferenciaTransaccion = p.Pago.ReferenciaTransaccion,
                FechaSolicitud = p.Pago.FechaPago
            })
            .ToList();
    }

    public async Task AprobarPagoAsync(int pagoId, string? comentario)
    {
        var pago = await _unitOfWork.Pagos.ObtenerPorIdAsync(pagoId)
            ?? throw new InvalidOperationException("No se encontró el pago indicado.");

        var paquete = await _unitOfWork.PaquetesEmpresa.ObtenerPorIdAsync(pago.PaqueteEmpresaId)
            ?? throw new InvalidOperationException("No se encontró el paquete asociado a este pago.");

        var plan = await _unitOfWork.PlanesSuscripcion.ObtenerPorIdAsync(paquete.PlanSuscripcionId)
            ?? throw new InvalidOperationException("No se encontró el plan asociado a este paquete.");

        pago.EstadoPago = EstadosPago.Aprobado;
        pago.ComentarioRevision = comentario;
        pago.FechaRevision = DateTime.UtcNow;

        // La vigencia empieza a contar desde la aprobación, no desde la
        // solicitud.
        paquete.FechaVencimiento = DateTime.UtcNow.AddDays(plan.DiasVigencia);
        paquete.Estado = EstadosPaquete.Vigente;

        _unitOfWork.Pagos.Actualizar(pago);
        _unitOfWork.PaquetesEmpresa.Actualizar(paquete);
        await _unitOfWork.GuardarCambiosAsync();

        await _publicadorEventos.PublicarAsync(new ChambeaJobs.Application.Eventos.PagoRealizadoEvento(paquete.EmpresaId, pago.Id, pago.Monto));

        // 🔔 Notificar a la empresa: su paquete ya está activo y puede publicar.
        var empresa = await _unitOfWork.Empresas.ObtenerPorIdAsync(paquete.EmpresaId);
        if (!string.IsNullOrWhiteSpace(empresa?.UsuarioId))
        {
            var descripcionCupo = paquete.VacantesIncluidas.HasValue
                ? $"paquete de {paquete.VacantesIncluidas} vacantes"
                : "plan de vacantes ilimitadas";

            await _notificacionService.CrearAsync(
                empresa!.UsuarioId,
                Notificacion.Tipos.PagoAprobado,
                $"✅ Tu pago fue aprobado. Tu {descripcionCupo} ya está activo y vence el {paquete.FechaVencimiento:dd/MM/yyyy}.",
                "/Empresa/Dashboard");
        }
    }
    public async Task RechazarPagoAsync(int pagoId, string? comentario)
    {
        var pago = await _unitOfWork.Pagos.ObtenerPorIdAsync(pagoId)
            ?? throw new InvalidOperationException("No se encontró el pago indicado.");

        var paquete = await _unitOfWork.PaquetesEmpresa.ObtenerPorIdAsync(pago.PaqueteEmpresaId)
            ?? throw new InvalidOperationException("No se encontró el paquete asociado a este pago.");

        pago.EstadoPago = EstadosPago.Rechazado;
        pago.ComentarioRevision = comentario;
        pago.FechaRevision = DateTime.UtcNow;

        paquete.Estado = EstadosPaquete.Rechazado;

        _unitOfWork.Pagos.Actualizar(pago);
        _unitOfWork.PaquetesEmpresa.Actualizar(paquete);
        await _unitOfWork.GuardarCambiosAsync();

        // 🔔 Notificar a la empresa: su pago fue rechazado (con el motivo, si se dio).
        var empresa = await _unitOfWork.Empresas.ObtenerPorIdAsync(paquete.EmpresaId);
        if (!string.IsNullOrWhiteSpace(empresa?.UsuarioId))
        {
            var detalleMotivo = string.IsNullOrWhiteSpace(comentario) ? "" : $" Motivo: {comentario}";
            await _notificacionService.CrearAsync(
                empresa!.UsuarioId,
                Notificacion.Tipos.PagoRechazado,
                $"❌ Tu pago fue rechazado.{detalleMotivo}",
                "/Empresa/HistorialPagos");
        }
    }

    public async Task ActualizarVencimientosAsync()
    {
        var todos = await _unitOfWork.PaquetesEmpresa.ObtenerTodosAsync();
        var vencidosSinMarcar = todos.Where(p =>
            p.Estado == EstadosPaquete.Vigente && DateTime.UtcNow > p.FechaVencimiento);

        var huboCambios = false;
        foreach (var paquete in vencidosSinMarcar)
        {
            paquete.Estado = EstadosPaquete.Vencido;
            _unitOfWork.PaquetesEmpresa.Actualizar(paquete);
            huboCambios = true;
        }

        if (huboCambios)
        {
            await _unitOfWork.GuardarCambiosAsync();
        }
    }

    public async Task<List<PlanSuscripcionOptionDto>> ObtenerPlanesDisponiblesAsync()
    {
        var planes = await _unitOfWork.PlanesSuscripcion.ObtenerTodosAsync();

        return planes
            .Where(p => p.Activo)
            .OrderBy(p => p.Precio)
            .Select(p => new PlanSuscripcionOptionDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio,
                VacantesIncluidas = p.VacantesIncluidas,
                DiasVigencia = p.DiasVigencia,
                IncluyePruebaPsicometrica = p.IncluyePruebaPsicometrica,
                IncluyeVideoCv = p.IncluyeVideoCv,
                PermiteVacantesDestacadas = p.PermiteVacantesDestacadas
            })
            .ToList();
    }

    public async Task<List<EditarPlanSuscripcionDto>> ObtenerTodosLosPlanesAsync()
    {
        var planes = await _unitOfWork.PlanesSuscripcion.ObtenerTodosAsync();

        return planes
            .OrderBy(p => p.Precio)
            .Select(p => new EditarPlanSuscripcionDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio,
                VacantesIncluidas = p.VacantesIncluidas,
                DiasVigencia = p.DiasVigencia,
                Activo = p.Activo,
                IncluyePruebaPsicometrica = p.IncluyePruebaPsicometrica,
                IncluyeVideoCv = p.IncluyeVideoCv,
                PermiteVacantesDestacadas = p.PermiteVacantesDestacadas
            })
            .ToList();
    }

    public async Task ActualizarPlanAsync(EditarPlanSuscripcionDto datos, string adminId)
    {
        if (datos.Precio <= 0 || datos.DiasVigencia <= 0)
        {
            throw new ArgumentException("El precio y los días de vigencia deben ser valores positivos.");
        }

        if (datos.VacantesIncluidas.HasValue && datos.VacantesIncluidas <= 0)
        {
            throw new ArgumentException("La cantidad de vacantes incluidas debe ser positiva (déjalo vacío para 'ilimitado').");
        }

        var plan = await _unitOfWork.PlanesSuscripcion.ObtenerPorIdAsync(datos.Id)
            ?? throw new InvalidOperationException("Este plan no existe.");

        plan.Precio = datos.Precio;
        plan.VacantesIncluidas = datos.VacantesIncluidas;
        plan.DiasVigencia = datos.DiasVigencia;
        plan.Activo = datos.Activo;
        plan.IncluyePruebaPsicometrica = datos.IncluyePruebaPsicometrica;
        plan.IncluyeVideoCv = datos.IncluyeVideoCv;
        plan.PermiteVacantesDestacadas = datos.PermiteVacantesDestacadas;

        _unitOfWork.PlanesSuscripcion.Actualizar(plan);
        await _unitOfWork.GuardarCambiosAsync();

        await _auditoriaService.RegistrarAsync(
            adminId, "Editar", "PlanSuscripcion", plan.Id.ToString(),
            $"Nombre={plan.Nombre}, Precio={plan.Precio}, Vacantes={(plan.VacantesIncluidas?.ToString() ?? "ilimitado")}, Dias={plan.DiasVigencia}, Activo={plan.Activo}");
    }

    private static string BuscarValor(IEnumerable<ConfiguracionSistema> configuraciones, string clave, string valorPorDefecto) =>
        configuraciones.FirstOrDefault(c => c.Clave == clave)?.Valor ?? valorPorDefecto;
}
