using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="IEmpresaService"/>
public class EmpresaService : IEmpresaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly ChambeaJobs.Application.Eventos.IPublicadorEventos _publicadorEventos;

    public EmpresaService(IUnitOfWork unitOfWork, IFileStorageService fileStorage, ChambeaJobs.Application.Eventos.IPublicadorEventos publicadorEventos)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _publicadorEventos = publicadorEventos;
    }

    public async Task CrearPerfilInicialAsync(string usuarioId, string nombreEmpresa, string ruc, int ubicacionId, int planSuscripcionId)
    {
        if (await _unitOfWork.Empresas.ExisteParaUsuarioAsync(usuarioId))
        {
            return; // idempotente
        }

        var empresa = new Empresa
        {
            UsuarioId = usuarioId,
            NombreEmpresa = nombreEmpresa,
            RUC = ruc,
            UbicacionId = ubicacionId,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.Empresas.AgregarAsync(empresa);
        await _unitOfWork.GuardarCambiosAsync();

        // Se agrega automáticamente al pipeline del CRM interno como
        // "Cliente activo", para que el equipo comercial la vea sin
        // tener que capturarla a mano.
        await _publicadorEventos.PublicarAsync(new ChambeaJobs.Application.Eventos.EmpresaRegistradaEvento(empresa.Id));

        // 🎁 Primer mes gratis: se activa de inmediato, sin necesidad de pago
        // ni aprobación del admin. A partir del segundo ciclo, el sistema
        // genera automáticamente el cobro correspondiente al plan elegido
        // (ver RenovacionSuscripcionBackgroundService).
        var plan = await _unitOfWork.PlanesSuscripcion.ObtenerPorIdAsync(planSuscripcionId)
            ?? throw new InvalidOperationException("El plan seleccionado no es válido.");

        var paqueteGratis = new PaqueteEmpresa
        {
            EmpresaId = empresa.Id,
            PlanSuscripcionId = plan.Id,
            FechaCompra = DateTime.UtcNow,
            FechaVencimiento = DateTime.UtcNow.AddDays(plan.DiasVigencia),
            VacantesIncluidas = plan.VacantesIncluidas,
            VacantesConsumidas = 0,
            Estado = EstadosPaquete.Vigente,
            EsPruebaGratis = true,
            RenovacionAutomatica = true
        };

        await _unitOfWork.PaquetesEmpresa.AgregarAsync(paqueteGratis);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task<EmpresaPerfilDto?> ObtenerPerfilPorUsuarioIdAsync(string usuarioId)
    {
        var empresa = await _unitOfWork.Empresas.ObtenerPorUsuarioIdAsync(usuarioId);
        if (empresa is null) return null;

        return new EmpresaPerfilDto
        {
            Id = empresa.Id,
            NombreEmpresa = empresa.NombreEmpresa,
            RUC = empresa.RUC,
            LogoUrl = empresa.LogoArchivo?.RutaArchivo,
            Descripcion = empresa.Descripcion,
            SitioWeb = empresa.SitioWeb,
            UbicacionId = empresa.UbicacionId,
            UbicacionNombre = empresa.Ubicacion?.NombreCompleto ?? string.Empty
        };
    }

    public async Task ActualizarPerfilAsync(string usuarioId, EditarPerfilEmpresaDto datos)
    {
        var empresa = await _unitOfWork.Empresas.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró el perfil de esta empresa.");

        empresa.NombreEmpresa = datos.NombreEmpresa;
        empresa.Descripcion = datos.Descripcion;
        empresa.SitioWeb = datos.SitioWeb;
        empresa.UbicacionId = datos.UbicacionId;

        if (datos.Logo is not null)
        {
            var rutaLogo = await _fileStorage.GuardarArchivoAsync(datos.Logo, "logos");

            var archivoLogo = new Archivo
            {
                UsuarioId = usuarioId,
                TipoArchivo = "Logo",
                RutaArchivo = rutaLogo,
                NombreOriginal = datos.Logo.FileName,
                PesoBytes = (int)datos.Logo.Length,
                FechaSubida = DateTime.UtcNow
            };

            await _unitOfWork.Archivos.AgregarAsync(archivoLogo);
            await _unitOfWork.GuardarCambiosAsync();

            empresa.LogoArchivoId = archivoLogo.Id;
        }

        _unitOfWork.Empresas.Actualizar(empresa);
        await _unitOfWork.GuardarCambiosAsync();

        // "Perfil completo" = tiene descripción y logo — se considera
        // suficiente esfuerzo para avanzar de Prospecto a Empresa verificada.
        if (!string.IsNullOrWhiteSpace(empresa.Descripcion) && empresa.LogoArchivoId.HasValue)
        {
            await _publicadorEventos.PublicarAsync(new ChambeaJobs.Application.Eventos.PerfilActualizadoEvento(empresa.Id));
        }
    }

    public async Task ActualizarLogoAsync(string usuarioId, Microsoft.AspNetCore.Http.IFormFile logo)
    {
        var empresa = await _unitOfWork.Empresas.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró el perfil de esta empresa.");

        var rutaLogo = await _fileStorage.GuardarArchivoAsync(logo, "logos");

        var archivoLogo = new Archivo
        {
            UsuarioId = usuarioId,
            TipoArchivo = "Logo",
            RutaArchivo = rutaLogo,
            NombreOriginal = logo.FileName,
            PesoBytes = (int)logo.Length,
            FechaSubida = DateTime.UtcNow
        };

        await _unitOfWork.Archivos.AgregarAsync(archivoLogo);
        await _unitOfWork.GuardarCambiosAsync();

        empresa.LogoArchivoId = archivoLogo.Id;
        _unitOfWork.Empresas.Actualizar(empresa);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task<bool> ExisteRucAsync(string ruc) => await _unitOfWork.Empresas.ExisteRucAsync(ruc);

    public async Task<int?> ObtenerEmpresaIdPorUsuarioAsync(string usuarioId)
    {
        var empresa = await _unitOfWork.Empresas.ObtenerPorUsuarioIdAsync(usuarioId, incluirDetalle: false);
        return empresa?.Id;
    }

    // ---------- Perfil enriquecido ----------

    public async Task<EditarPerfilExtendidoEmpresaDto?> ObtenerPerfilExtendidoAsync(string usuarioId)
    {
        var empresa = await _unitOfWork.Empresas.ObtenerPorUsuarioIdAsync(usuarioId, incluirDetalle: false);
        if (empresa is null) return null;

        return new EditarPerfilExtendidoEmpresaDto
        {
            Historia = empresa.Historia,
            Mision = empresa.Mision,
            Vision = empresa.Vision,
            CulturaOrganizacional = empresa.CulturaOrganizacional,
            Beneficios = empresa.Beneficios,
            NumeroColaboradores = empresa.NumeroColaboradores,
            SectorEmpresarial = empresa.SectorEmpresarial,
            NombreContacto = empresa.NombreContacto,
            TelefonoContacto = empresa.TelefonoContacto,
            FacebookUrl = empresa.FacebookUrl,
            InstagramUrl = empresa.InstagramUrl,
            LinkedInUrl = empresa.LinkedInUrl,
            TiktokUrl = empresa.TiktokUrl
        };
    }

    public async Task ActualizarPerfilExtendidoAsync(string usuarioId, EditarPerfilExtendidoEmpresaDto datos)
    {
        var empresa = await _unitOfWork.Empresas.ObtenerPorUsuarioIdAsync(usuarioId, incluirDetalle: false)
            ?? throw new InvalidOperationException("No se encontró el perfil de esta empresa.");

        empresa.Historia = datos.Historia;
        empresa.Mision = datos.Mision;
        empresa.Vision = datos.Vision;
        empresa.CulturaOrganizacional = datos.CulturaOrganizacional;
        empresa.Beneficios = datos.Beneficios;
        empresa.NumeroColaboradores = datos.NumeroColaboradores;
        empresa.SectorEmpresarial = datos.SectorEmpresarial;
        empresa.NombreContacto = datos.NombreContacto;
        empresa.TelefonoContacto = datos.TelefonoContacto;
        empresa.FacebookUrl = datos.FacebookUrl;
        empresa.InstagramUrl = datos.InstagramUrl;
        empresa.LinkedInUrl = datos.LinkedInUrl;
        empresa.TiktokUrl = datos.TiktokUrl;

        _unitOfWork.Empresas.Actualizar(empresa);
        await _unitOfWork.GuardarCambiosAsync();

        // Estos datos alimentan directamente la ficha del CRM — así el
        // administrador no tiene que volver a escribirlos a mano.
        await _publicadorEventos.PublicarAsync(new ChambeaJobs.Application.Eventos.PerfilActualizadoEvento(empresa.Id));
    }

    public async Task<List<EmpresaResumenAsociadaDto>> ObtenerEmpresasAsociadasAsync()
    {
        var empresas = await _unitOfWork.Empresas.ObtenerTodasConDetalleAsync();

        return empresas
            .Select(e => new EmpresaResumenAsociadaDto
            {
                Id = e.Id,
                NombreEmpresa = e.NombreEmpresa,
                LogoUrl = e.LogoArchivo?.RutaArchivo,
                UbicacionNombre = e.Ubicacion is null ? string.Empty : $"{e.Ubicacion.Ciudad}, {e.Ubicacion.Departamento}",
                VacantesActivas = e.Vacantes.Count(v => v.Estado == "Activa")
            })
            .ToList();
    }

    public async Task<EmpresaPerfilPublicoDto?> ObtenerPerfilPublicoAsync(int empresaId)
    {
        var empresa = await _unitOfWork.Empresas.ObtenerConGaleriaAsync(empresaId);
        if (empresa is null) return null;

        return new EmpresaPerfilPublicoDto
        {
            Id = empresa.Id,
            NombreEmpresa = empresa.NombreEmpresa,
            LogoUrl = empresa.LogoArchivo?.RutaArchivo,
            Descripcion = empresa.Descripcion,
            SitioWeb = empresa.SitioWeb,
            UbicacionNombre = empresa.Ubicacion?.NombreCompleto ?? string.Empty,
            Historia = empresa.Historia,
            Mision = empresa.Mision,
            Vision = empresa.Vision,
            CulturaOrganizacional = empresa.CulturaOrganizacional,
            Beneficios = (empresa.Beneficios ?? string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList(),
            NumeroColaboradores = empresa.NumeroColaboradores,
            FacebookUrl = empresa.FacebookUrl,
            InstagramUrl = empresa.InstagramUrl,
            LinkedInUrl = empresa.LinkedInUrl,
            TiktokUrl = empresa.TiktokUrl,
            VacantesActivas = empresa.Vacantes.Count(v => v.Estado == EstadosVacante.Activa),
            Galeria = empresa.Galeria
                .OrderBy(g => g.Orden)
                .Select(g => new EmpresaGaleriaDto
                {
                    Id = g.Id,
                    TipoMedio = g.TipoMedio,
                    // Antes esto solo miraba UrlVideo para los videos, así
                    // que un video subido como archivo (ArchivoId) nunca
                    // aparecía. Ahora prioriza el archivo subido, y solo cae
                    // a UrlVideo para videos antiguos que se hayan guardado
                    // por enlace antes de este cambio.
                    Url = g.ArchivoId.HasValue ? (g.Archivo?.RutaArchivo ?? string.Empty) : (g.UrlVideo ?? string.Empty),
                    Titulo = g.Titulo
                })
                .ToList()
        };
    }

    // ---------- Galería ----------

    /// <summary>Límite de elementos (fotos + videos) por galería, para evitar abuso de almacenamiento.</summary>
    private const int MaximoElementosGaleria = 12;

    public async Task AgregarFotoGaleriaAsync(string usuarioId, Microsoft.AspNetCore.Http.IFormFile foto, string? titulo)
    {
        var empresa = await _unitOfWork.Empresas.ObtenerPorUsuarioIdAsync(usuarioId, incluirDetalle: false)
            ?? throw new InvalidOperationException("No se encontró el perfil de esta empresa.");

        await ValidarCupoGaleriaAsync(empresa.Id);

        var rutaArchivo = await _fileStorage.GuardarArchivoAsync(foto, "galeria-empresa");

        var archivo = new Archivo
        {
            UsuarioId = usuarioId,
            TipoArchivo = "GaleriaEmpresa",
            RutaArchivo = rutaArchivo,
            NombreOriginal = foto.FileName,
            PesoBytes = (int)foto.Length,
            FechaSubida = DateTime.UtcNow
        };

        await _unitOfWork.Archivos.AgregarAsync(archivo);
        await _unitOfWork.GuardarCambiosAsync();

        await _unitOfWork.EmpresaGalerias.AgregarAsync(new EmpresaGaleria
        {
            EmpresaId = empresa.Id,
            TipoMedio = EmpresaGaleria.TiposMedio.Foto,
            ArchivoId = archivo.Id,
            Titulo = titulo,
            FechaCreacion = DateTime.UtcNow
        });

        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task AgregarVideoGaleriaAsync(string usuarioId, Microsoft.AspNetCore.Http.IFormFile video, string? titulo)
    {
        var empresa = await _unitOfWork.Empresas.ObtenerPorUsuarioIdAsync(usuarioId, incluirDetalle: false)
            ?? throw new InvalidOperationException("No se encontró el perfil de esta empresa.");

        await ValidarCupoGaleriaAsync(empresa.Id);

        // Antes esto pedía una URL de "insertar/embed" de YouTube/Vimeo, lo
        // cual confundía a las empresas (había que saber sacar ese enlace
        // específico). Ahora se sube el video directo como archivo, igual
        // que ya funcionaba para las fotos — más simple para el usuario.
        var rutaArchivo = await _fileStorage.GuardarArchivoAsync(video, "videos-galeria-empresa");

        var archivo = new Archivo
        {
            UsuarioId = usuarioId,
            TipoArchivo = "GaleriaEmpresaVideo",
            RutaArchivo = rutaArchivo,
            NombreOriginal = video.FileName,
            PesoBytes = (int)video.Length,
            FechaSubida = DateTime.UtcNow
        };

        await _unitOfWork.Archivos.AgregarAsync(archivo);
        await _unitOfWork.GuardarCambiosAsync();

        await _unitOfWork.EmpresaGalerias.AgregarAsync(new EmpresaGaleria
        {
            EmpresaId = empresa.Id,
            TipoMedio = EmpresaGaleria.TiposMedio.Video,
            ArchivoId = archivo.Id,
            Titulo = titulo,
            FechaCreacion = DateTime.UtcNow
        });

        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task EliminarGaleriaAsync(string usuarioId, int galeriaId)
    {
        var empresa = await _unitOfWork.Empresas.ObtenerPorUsuarioIdAsync(usuarioId, incluirDetalle: false)
            ?? throw new InvalidOperationException("No se encontró el perfil de esta empresa.");

        var item = await _unitOfWork.EmpresaGalerias.ObtenerPorIdAsync(galeriaId)
            ?? throw new InvalidOperationException("Este elemento de la galería no existe.");

        if (item.EmpresaId != empresa.Id)
        {
            throw new InvalidOperationException("Este elemento no pertenece a tu empresa.");
        }

        _unitOfWork.EmpresaGalerias.Eliminar(item);
        await _unitOfWork.GuardarCambiosAsync();
    }

    /// <summary>
    /// Evita que una empresa suba fotos/videos ilimitados a su galería
    /// (protege el almacenamiento, especialmente relevante en planes de
    /// hosting gratuitos con cuota reducida).
    /// </summary>
    private async Task ValidarCupoGaleriaAsync(int empresaId)
    {
        var elementosActuales = (await _unitOfWork.EmpresaGalerias.ObtenerTodosAsync())
            .Count(g => g.EmpresaId == empresaId);

        if (elementosActuales >= MaximoElementosGaleria)
        {
            throw new InvalidOperationException(
                $"Alcanzaste el máximo de {MaximoElementosGaleria} elementos en tu galería. Elimina alguno antes de agregar uno nuevo.");
        }
    }
}
