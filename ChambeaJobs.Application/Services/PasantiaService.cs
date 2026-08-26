using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="IPasantiaService"/>
public class PasantiaService : IPasantiaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificacionService _notificacionService;
    private readonly IAuditoriaService _auditoriaService;

    public PasantiaService(IUnitOfWork unitOfWork, INotificacionService notificacionService, IAuditoriaService auditoriaService)
    {
        _unitOfWork = unitOfWork;
        _notificacionService = notificacionService;
        _auditoriaService = auditoriaService;
    }

    public async Task<int> PublicarPasantiaAsync(int empresaId, PasantiaFormDto datos)
    {
        ValidarFechasYRemuneracion(datos);

        var pasantia = new Pasantia
        {
            EmpresaId = empresaId,
            CategoriaId = datos.CategoriaId,
            UbicacionId = datos.UbicacionId,
            Titulo = datos.Titulo,
            Descripcion = datos.Descripcion,
            Requisitos = datos.Requisitos,
            Modalidad = datos.Modalidad,
            DuracionMeses = datos.DuracionMeses,
            EsRemunerada = datos.EsRemunerada,
            MontoRemuneracion = datos.EsRemunerada ? datos.MontoRemuneracion : null,
            FechaPublicacion = DateTime.UtcNow,
            FechaCierre = datos.FechaCierre,
            Estado = EstadosPasantia.Activa
        };

        await _unitOfWork.Pasantias.AgregarAsync(pasantia);
        await _unitOfWork.GuardarCambiosAsync();
        return pasantia.Id;
    }

    public async Task ActualizarPasantiaAsync(int empresaId, PasantiaFormDto datos)
    {
        var pasantia = await ObtenerPasantiaDeEmpresaAsync(empresaId, datos.Id);

        ValidarFechasYRemuneracion(datos);

        pasantia.Titulo = datos.Titulo;
        pasantia.CategoriaId = datos.CategoriaId;
        pasantia.UbicacionId = datos.UbicacionId;
        pasantia.Modalidad = datos.Modalidad;
        pasantia.Descripcion = datos.Descripcion;
        pasantia.Requisitos = datos.Requisitos;
        pasantia.DuracionMeses = datos.DuracionMeses;
        pasantia.EsRemunerada = datos.EsRemunerada;
        pasantia.MontoRemuneracion = datos.EsRemunerada ? datos.MontoRemuneracion : null;
        pasantia.FechaCierre = datos.FechaCierre;

        _unitOfWork.Pasantias.Actualizar(pasantia);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task CerrarPasantiaAsync(int empresaId, int pasantiaId)
    {
        var pasantia = await ObtenerPasantiaDeEmpresaAsync(empresaId, pasantiaId);
        pasantia.Estado = EstadosPasantia.Cerrada;
        _unitOfWork.Pasantias.Actualizar(pasantia);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task EliminarPasantiaAsync(int empresaId, int pasantiaId)
    {
        var pasantia = await ObtenerPasantiaDeEmpresaAsync(empresaId, pasantiaId);
        _unitOfWork.Pasantias.Eliminar(pasantia);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task<List<PasantiaListItemDto>> ObtenerPasantiasDeEmpresaAsync(int empresaId)
    {
        //var pasantias = await _unitOfWork.Pasantias.ObtenerPorEmpresaAsync(empresaId);
        //var resultado = new List<PasantiaListItemDto>();

        //foreach (var p in pasantias.OrderByDescending(p => p.FechaPublicacion))
        //{
        //    var postulantes = await _unitOfWork.PostulacionesPasantia.ObtenerPorPasantiaAsync(p.Id);
        //    resultado.Add(new PasantiaListItemDto
        //    {
        //        Id = p.Id,
        //        Titulo = p.Titulo,
        //        CategoriaNombre = p.Categoria?.Nombre ?? string.Empty,
        //        FechaPublicacion = p.FechaPublicacion,
        //        Estado = p.Estado,
        //        NumeroPostulantes = postulantes.Count()
        //    });
        //}

        //return resultado;

        var pasantias = (await _unitOfWork.Pasantias.ObtenerPorEmpresaAsync(empresaId))
            .OrderByDescending(p => p.FechaPublicacion).ToList();
        var conteos = await _unitOfWork.PostulacionesPasantia.ContarPorPasantiasAsync(pasantias.Select(p => p.Id));
        return pasantias.Select(p => new PasantiaListItemDto
        {
            Id = p.Id,
            Titulo = p.Titulo,
            CategoriaNombre = p.Categoria?.Nombre ?? string.Empty,
            FechaPublicacion = p.FechaPublicacion,
            Estado = p.Estado,
            NumeroPostulantes = conteos.TryGetValue(p.Id, out var count) ? count : 0
        }).ToList();

    }

    public async Task<PasantiaFormDto?> ObtenerParaEditarAsync(int empresaId, int pasantiaId)
    {
        var pasantia = await ObtenerPasantiaDeEmpresaAsync(empresaId, pasantiaId, lanzarSiNoExiste: false);
        if (pasantia is null) return null;

        return new PasantiaFormDto
        {
            Id = pasantia.Id,
            Titulo = pasantia.Titulo,
            CategoriaId = pasantia.CategoriaId,
            UbicacionId = pasantia.UbicacionId,
            Modalidad = pasantia.Modalidad,
            Descripcion = pasantia.Descripcion,
            Requisitos = pasantia.Requisitos,
            DuracionMeses = pasantia.DuracionMeses,
            EsRemunerada = pasantia.EsRemunerada,
            MontoRemuneracion = pasantia.MontoRemuneracion,
            FechaCierre = pasantia.FechaCierre
        };
    }

    public async Task<PasantiaDetalleDto?> ObtenerDetalleAsync(int pasantiaId)
    {
        var pasantia = await _unitOfWork.Pasantias.ObtenerConDetalleAsync(pasantiaId);
        return pasantia is null ? null : MapearDetalle(pasantia);
    }

    public async Task<List<PasantiaDetalleDto>> BuscarAsync(string? palabraClave, int? categoriaId, int? ubicacionId, string? modalidad)
    {
        var resultados = await _unitOfWork.Pasantias.BuscarAsync(palabraClave, categoriaId, ubicacionId, modalidad);
        return resultados.Select(MapearDetalle).ToList();
    }

    // ---------- Postulaciones a pasantías ----------

    public async Task PostularAsync(int candidatoId, int pasantiaId)
    {
        if (await _unitOfWork.PostulacionesPasantia.ExistePostulacionAsync(candidatoId, pasantiaId))
        {
            throw new InvalidOperationException("Ya te has postulado a esta pasantía.");
        }

        var pasantia = await _unitOfWork.Pasantias.ObtenerConDetalleAsync(pasantiaId)
            ?? throw new InvalidOperationException("Esta pasantía no existe.");

        if (pasantia.Estado != EstadosPasantia.Activa)
        {
            throw new InvalidOperationException("Esta pasantía ya no acepta postulaciones.");
        }

        var candidato = await _unitOfWork.Candidatos.ObtenerConDetallePorIdAsync(candidatoId)
            ?? throw new InvalidOperationException("No se encontró tu perfil de candidato.");

        // Misma regla que en Vacantes: solo se puede postular a la carrera que se estudió.
        var estudioLaCarrera = candidato.Educaciones.Any(e => e.CategoriaId == pasantia.CategoriaId);
        if (!estudioLaCarrera)
        {
            throw new InvalidOperationException(
                $"Esta pasantía es de la categoría \"{pasantia.Categoria?.Nombre}\", pero ninguno de tus estudios corresponde a esa carrera. " +
                "Solo puedes postularte a pasantías relacionadas con lo que estudiaste.");
        }

        var postulacion = new PostulacionPasantia
        {
            CandidatoId = candidatoId,
            PasantiaId = pasantiaId,
            FechaPostulacion = DateTime.UtcNow,
            Estado = EstadosPostulacionPasantia.Postulado
        };

        await _unitOfWork.PostulacionesPasantia.AgregarAsync(postulacion);
        await _unitOfWork.GuardarCambiosAsync();

        if (!string.IsNullOrWhiteSpace(pasantia.Empresa?.UsuarioId))
        {
            var nombreCandidato = $"{candidato.Nombres} {candidato.Apellidos}".Trim();

            await _notificacionService.CrearAsync(
                pasantia.Empresa!.UsuarioId,
                "NuevaPostulacionPasantia",
                $"🎓 {nombreCandidato} se postuló a tu pasantía \"{pasantia.Titulo}\".",
                $"/Empresa/PostulantesPasantia?pasantiaId={pasantiaId}");
        }
    }

    public async Task<bool> YaPostuloAsync(int candidatoId, int pasantiaId) =>
        await _unitOfWork.PostulacionesPasantia.ExistePostulacionAsync(candidatoId, pasantiaId);

    public async Task<List<PostulantePasantiaDto>> ObtenerPostulantesAsync(int empresaId, int pasantiaId)
    {
        var pasantia = await ObtenerPasantiaDeEmpresaAsync(empresaId, pasantiaId);
        var postulaciones = await _unitOfWork.PostulacionesPasantia.ObtenerPorPasantiaAsync(pasantia.Id);

        return postulaciones
            .OrderByDescending(p => p.FechaPostulacion)
            .Select(p => new PostulantePasantiaDto
            {
                PostulacionId = p.Id,
                CandidatoId = p.CandidatoId,
                UsuarioId = p.Candidato?.UsuarioId ?? string.Empty,
                NombreCompleto = $"{p.Candidato?.Nombres} {p.Candidato?.Apellidos}".Trim(),
                FotoUrl = p.Candidato?.FotoUrl,
                CvUrl = p.Candidato?.CvArchivo?.RutaArchivo,
                FechaPostulacion = p.FechaPostulacion,
                Estado = p.Estado
            })
            .ToList();
    }

    public async Task<List<PostulacionPasantiaCandidatoDto>> ObtenerMisPostulacionesAsync(int candidatoId)
    {
        var postulaciones = await _unitOfWork.PostulacionesPasantia.ObtenerPorCandidatoAsync(candidatoId);

        return postulaciones
            .OrderByDescending(p => p.FechaPostulacion)
            .Select(p => new PostulacionPasantiaCandidatoDto
            {
                Id = p.Id,
                PasantiaId = p.PasantiaId,
                PasantiaTitulo = p.Pasantia?.Titulo ?? string.Empty,
                EmpresaNombre = p.Pasantia?.Empresa?.NombreEmpresa ?? string.Empty,
                FechaPostulacion = p.FechaPostulacion,
                Estado = p.Estado
            })
            .ToList();
    }

    public async Task CambiarEstadoPostulacionAsync(int empresaId, int postulacionId, string nuevoEstado, string? nota)
    {
        var postulacion = await _unitOfWork.PostulacionesPasantia.ObtenerConDetalleAsync(postulacionId)
            ?? throw new InvalidOperationException("No se encontró esta postulación.");

        if (postulacion.Pasantia?.EmpresaId != empresaId)
        {
            throw new InvalidOperationException("Esta postulación no corresponde a una pasantía de tu empresa.");
        }

        postulacion.Estado = nuevoEstado;
        postulacion.NotaEmpresa = nota;
        _unitOfWork.PostulacionesPasantia.Actualizar(postulacion);
        await _unitOfWork.GuardarCambiosAsync();

        if (!string.IsNullOrWhiteSpace(postulacion.Candidato?.UsuarioId))
        {
            await _notificacionService.CrearAsync(
                postulacion.Candidato!.UsuarioId,
                "CambioEstadoPostulacionPasantia",
                $"🔔 Tu postulación a la pasantía \"{postulacion.Pasantia?.Titulo}\" cambió a: {nuevoEstado}.",
                "/Candidato/MisPostulacionesPasantias");
        }
    }

    // ---------- Administración ----------

    public async Task<List<PasantiaAdminDto>> ObtenerTodasAsync()
    {
        //var pasantias = await _unitOfWork.Pasantias.ObtenerTodasConDetalleAsync();
        //var resultado = new List<PasantiaAdminDto>();

        //foreach (var p in pasantias.OrderByDescending(p => p.FechaPublicacion))
        //{
        //    var postulantes = await _unitOfWork.PostulacionesPasantia.ObtenerPorPasantiaAsync(p.Id);
        //    resultado.Add(new PasantiaAdminDto
        //    {
        //        Id = p.Id,
        //        Titulo = p.Titulo,
        //        EmpresaNombre = p.Empresa?.NombreEmpresa ?? string.Empty,
        //        CategoriaNombre = p.Categoria?.Nombre ?? string.Empty,
        //        Estado = p.Estado,
        //        FechaPublicacion = p.FechaPublicacion,
        //        NumeroPostulantes = postulantes.Count()
        //    });
        //}

        //return resultado;

        //var pasantias = await _unitOfWork.Pasantias.ObtenerTodasConDetalleAsync();
        //var postulantes = await _unitOfWork.PostulacionesPasantia.ObtenerPorPasantiaAsync(select  .Id);

        var pasantias = (await _unitOfWork.Pasantias.ObtenerTodasConDetalleAsync())
           .OrderByDescending(p => p.Id).ToList();
        var conteos = await _unitOfWork.PostulacionesPasantia.ObtenerPorPasantiaAsync(pasantias.Select(p => p.Id));
        return pasantias
            .OrderByDescending(p => p.FechaPublicacion)
            .Select(p => new PasantiaAdminDto
            {
                Id = p.Id,
                Titulo = p.Titulo,
                EmpresaNombre = p.Empresa?.NombreEmpresa ?? string.Empty,
                CategoriaNombre = p.Categoria?.Nombre ?? string.Empty,
                Estado = p.Estado,
                FechaPublicacion = p.FechaPublicacion,
                NumeroPostulantes = conteos.TryGetValue(p.Id, out var count) ? count : 0
            }).ToList();
            


    }

    public async Task DespublicarAdminAsync(int pasantiaId, string adminId)
    {
        var pasantia = await _unitOfWork.Pasantias.ObtenerPorIdAsync(pasantiaId)
            ?? throw new InvalidOperationException("Esta pasantía no existe.");

        pasantia.Estado = EstadosPasantia.Cerrada;
        _unitOfWork.Pasantias.Actualizar(pasantia);
        await _unitOfWork.GuardarCambiosAsync();

        await _auditoriaService.RegistrarAsync(adminId, "Despublicar", "Pasantia", pasantiaId.ToString());
    }

    public async Task EliminarAdminAsync(int pasantiaId, string adminId)
    {
        var pasantia = await _unitOfWork.Pasantias.ObtenerPorIdAsync(pasantiaId)
            ?? throw new InvalidOperationException("Esta pasantía no existe.");

        _unitOfWork.Pasantias.Eliminar(pasantia);
        await _unitOfWork.GuardarCambiosAsync();

        await _auditoriaService.RegistrarAsync(adminId, "Eliminar", "Pasantia", pasantiaId.ToString());
    }

    // ---------- Auxiliares privados ----------

    private async Task<Pasantia> ObtenerPasantiaDeEmpresaAsync(int empresaId, int pasantiaId, bool lanzarSiNoExiste = true)
    {
        var pasantia = await _unitOfWork.Pasantias.ObtenerConDetalleAsync(pasantiaId);

        if (pasantia is null || pasantia.EmpresaId != empresaId)
        {
            if (lanzarSiNoExiste)
            {
                throw new InvalidOperationException("Esta pasantía no existe o no te pertenece.");
            }
            return null!;
        }

        return pasantia;
    }

    private static void ValidarFechasYRemuneracion(PasantiaFormDto datos)
    {
        if (datos.FechaCierre.Date <= DateTime.UtcNow.Date)
        {
            throw new ArgumentException("La fecha de cierre debe ser posterior a hoy.");
        }

        if (datos.EsRemunerada && (!datos.MontoRemuneracion.HasValue || datos.MontoRemuneracion <= 0))
        {
            throw new ArgumentException("Indica el monto de la remuneración si marcaste la pasantía como remunerada.");
        }
    }

    private static PasantiaDetalleDto MapearDetalle(Pasantia p) => new()
    {
        Id = p.Id,
        Titulo = p.Titulo,
        EmpresaId = p.EmpresaId,
        EmpresaNombre = p.Empresa?.NombreEmpresa ?? string.Empty,
        EmpresaLogoUrl = p.Empresa?.LogoArchivo?.RutaArchivo,
        CategoriaNombre = p.Categoria?.Nombre ?? string.Empty,
        UbicacionNombre = p.Ubicacion?.NombreCompleto ?? string.Empty,
        Modalidad = p.Modalidad,
        DuracionMeses = p.DuracionMeses,
        EsRemunerada = p.EsRemunerada,
        MontoRemuneracion = p.MontoRemuneracion,
        Descripcion = p.Descripcion,
        Requisitos = p.Requisitos,
        FechaPublicacion = p.FechaPublicacion,
        FechaCierre = p.FechaCierre,
        Estado = p.Estado
    };
}
