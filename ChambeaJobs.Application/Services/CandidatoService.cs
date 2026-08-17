using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="ICandidatoService"/>
public class CandidatoService : ICandidatoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly INotificacionService _notificacionService;

    public CandidatoService(IUnitOfWork unitOfWork, IFileStorageService fileStorage, INotificacionService notificacionService)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _notificacionService = notificacionService;
    }

    public async Task<CandidatoPerfilDto?> ObtenerPerfilPorUsuarioIdAsync(string usuarioId)
    {
        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId);
        return candidato is null ? null : MapearPerfil(candidato);
    }

    public async Task<CandidatoPerfilDto?> ObtenerPerfilPorIdAsync(int candidatoId)
    {
        var candidato = await _unitOfWork.Candidatos.ObtenerConDetallePorIdAsync(candidatoId);
        return candidato is null ? null : MapearPerfil(candidato);
    }

    private static CandidatoPerfilDto MapearPerfil(Candidato candidato) => new()
    {
        Id = candidato.Id,
        Nombres = candidato.Nombres,
        Apellidos = candidato.Apellidos,
        FotoUrl = candidato.FotoUrl,
        Direccion = candidato.Direccion,
        Disponibilidad = candidato.Disponibilidad,
        CvNombreOriginal = candidato.CvArchivo?.NombreOriginal,
        CvUrl = candidato.CvArchivo?.RutaArchivo,
        VideoCvUrl = candidato.VideoCvArchivo?.RutaArchivo,
        PorcentajeCompletitud = candidato.CalcularPorcentajeCompletitud(),
        Experiencias = candidato.Experiencias
            .OrderByDescending(e => e.FechaInicio)
            .Select(e => new ExperienciaDto
            {
                Id = e.Id,
                NombreEmpresa = e.NombreEmpresa,
                Puesto = e.Puesto,
                FechaInicio = e.FechaInicio,
                FechaFin = e.FechaFin,
                Descripcion = e.Descripcion
            }).ToList(),
        Educaciones = candidato.Educaciones
            .OrderByDescending(e => e.FechaInicio)
            .Select(e => new EducacionDto
            {
                Id = e.Id,
                Institucion = e.Institucion,
                TituloObtenido = e.TituloObtenido,
                NivelEducativo = e.NivelEducativo,
                FechaInicio = e.FechaInicio,
                FechaFin = e.FechaFin,
                CategoriaId = e.CategoriaId,
                CategoriaNombre = e.Categoria?.Nombre,
                CarreraId = e.CarreraId,
                CarreraNombre = e.Carrera?.Nombre
            }).ToList(),
        Habilidades = candidato.Habilidades
            .Select(ch => new HabilidadCandidatoDto
            {
                HabilidadId = ch.HabilidadId,
                Nombre = ch.Habilidad?.Nombre ?? string.Empty,
                NivelDominio = ch.NivelDominio
            }).ToList(),
        Idiomas = candidato.Idiomas
            .Select(ci => new IdiomaCandidatoDto
            {
                IdiomaId = ci.IdiomaId,
                Nombre = ci.Idioma?.Nombre ?? string.Empty,
                Nivel = ci.Nivel
            }).ToList(),
        Certificados = candidato.Certificados
            .OrderByDescending(c => c.FechaObtencion)
            .Select(c => new CertificadoDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                InstitucionEmisora = c.InstitucionEmisora,
                FechaObtencion = c.FechaObtencion,
                ArchivoUrl = c.Archivo?.RutaArchivo ?? string.Empty,
                TipoDocumento = c.TipoDocumento
            }).ToList(),
        Cursos = candidato.Cursos
            .OrderByDescending(c => c.FechaFinalizacion)
            .Select(c => new CursoDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Institucion = c.Institucion,
                HorasDuracion = c.HorasDuracion,
                FechaFinalizacion = c.FechaFinalizacion,
                ArchivoUrl = c.Archivo?.RutaArchivo
            }).ToList()
    };

    public async Task CrearPerfilInicialAsync(string usuarioId, string nombres, string apellidos)
    {
        if (await _unitOfWork.Candidatos.ExisteParaUsuarioAsync(usuarioId))
        {
            return; // Idempotente: evita duplicar el perfil si ya existe.
        }

        var candidato = new Candidato
        {
            UsuarioId = usuarioId,
            Nombres = nombres,
            Apellidos = apellidos,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.Candidatos.AgregarAsync(candidato);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task ActualizarDatosBasicosAsync(string usuarioId, EditarPerfilCandidatoDto datos)
    {
        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró el perfil de este candidato.");

        candidato.Nombres = datos.Nombres;
        candidato.Apellidos = datos.Apellidos;
        candidato.Direccion = datos.Direccion;
        candidato.Disponibilidad = datos.Disponibilidad;

        // 🔔 "Video Curriculum disponible": solo se notifica cuando el candidato
        // agrega el video por primera vez o lo reemplaza (no en cada guardado del perfil).
        var videoCvCambio = false;

        if (datos.VideoCv is not null)
        {
            var rutaVideo = await _fileStorage.GuardarArchivoAsync(datos.VideoCv, "videos-cv");

            var archivoVideo = new Archivo
            {
                UsuarioId = usuarioId,
                TipoArchivo = "VideoCv",
                RutaArchivo = rutaVideo,
                NombreOriginal = datos.VideoCv.FileName,
                PesoBytes = (int)datos.VideoCv.Length,
                FechaSubida = DateTime.UtcNow
            };

            await _unitOfWork.Archivos.AgregarAsync(archivoVideo);
            await _unitOfWork.GuardarCambiosAsync(); // Para obtener el Id generado antes de asignarlo.

            candidato.VideoCvArchivoId = archivoVideo.Id;
            candidato.VideoCvFechaSubida = DateTime.UtcNow;
            videoCvCambio = true;
        }

        if (datos.Foto is not null)
        {
            var rutaFoto = await _fileStorage.GuardarArchivoAsync(datos.Foto, "fotos");
            candidato.FotoUrl = rutaFoto;
        }

        if (datos.Cv is not null)
        {
            var rutaCv = await _fileStorage.GuardarArchivoAsync(datos.Cv, "cv");

            var archivoCv = new Archivo
            {
                UsuarioId = usuarioId,
                TipoArchivo = "CV",
                RutaArchivo = rutaCv,
                NombreOriginal = datos.Cv.FileName,
                PesoBytes = (int)datos.Cv.Length,
                FechaSubida = DateTime.UtcNow
            };

            await _unitOfWork.Archivos.AgregarAsync(archivoCv);
            await _unitOfWork.GuardarCambiosAsync(); // Para obtener el Id generado antes de asignarlo.

            candidato.CvArchivoId = archivoCv.Id;
        }

        _unitOfWork.Candidatos.Actualizar(candidato);
        await _unitOfWork.GuardarCambiosAsync();

        if (videoCvCambio)
        {
            await NotificarVideoCvDisponibleAsync(candidato);
        }
    }

    /// <summary>🔔 "Video Curriculum disponible": avisa a las empresas donde el candidato tiene postulaciones activas (no rechazadas).</summary>
    private async Task NotificarVideoCvDisponibleAsync(Candidato candidato)
    {
        var postulaciones = await _unitOfWork.Postulaciones.ObtenerPorCandidatoAsync(candidato.Id);
        var nombreCandidato = $"{candidato.Nombres} {candidato.Apellidos}".Trim();

        var empresasNotificadas = new HashSet<string>();
        foreach (var p in postulaciones)
        {
            if (p.EstadoPostulacion?.Nombre == EstadoPostulacion.Nombres.Rechazado) continue;

            var vacante = await _unitOfWork.Vacantes.ObtenerConDetalleAsync(p.VacanteId);
            var usuarioEmpresaId = vacante?.Empresa?.UsuarioId;
            if (string.IsNullOrWhiteSpace(usuarioEmpresaId) || !empresasNotificadas.Add(usuarioEmpresaId)) continue;

            await _notificacionService.CrearAsync(
                usuarioEmpresaId,
                Notificacion.Tipos.VideoCvDisponible,
                $"🔔 Video Curriculum disponible: {nombreCandidato} actualizó su video.",
                $"/Empresa/CandidatosPostulados?vacanteId={p.VacanteId}");
        }
    }

    public async Task<int> AgregarExperienciaAsync(string usuarioId, ExperienciaDto experiencia)
    {
        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId, incluirDetalle: false)
            ?? throw new InvalidOperationException("No se encontró el perfil de este candidato.");

        if (experiencia.FechaFin.HasValue && experiencia.FechaFin < experiencia.FechaInicio)
        {
            throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.");
        }

        var nuevaExperiencia = new ExperienciaLaboral
        {
            CandidatoId = candidato.Id,
            NombreEmpresa = experiencia.NombreEmpresa,
            Puesto = experiencia.Puesto,
            FechaInicio = experiencia.FechaInicio,
            FechaFin = experiencia.FechaFin,
            Descripcion = experiencia.Descripcion
        };

        await _unitOfWork.Experiencias.AgregarAsync(nuevaExperiencia);
        await _unitOfWork.GuardarCambiosAsync();
        return nuevaExperiencia.Id;
    }

    public async Task EliminarExperienciaAsync(string usuarioId, int experienciaId)
    {
        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró el perfil de este candidato.");

        var experiencia = candidato.Experiencias.FirstOrDefault(e => e.Id == experienciaId)
            ?? throw new InvalidOperationException("Esta experiencia no pertenece a tu perfil.");

        _unitOfWork.Experiencias.Eliminar(experiencia);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task<int> AgregarEducacionAsync(string usuarioId, EducacionDto educacion)
    {
        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId, incluirDetalle: false)
            ?? throw new InvalidOperationException("No se encontró el perfil de este candidato.");

        if (educacion.FechaFin.HasValue && educacion.FechaFin < educacion.FechaInicio)
        {
            throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.");
        }

        // Nota: CategoriaId/CarreraId son opcionales a propósito — un
        // registro de Bachillerato (secundaria) no tiene una carrera/área
        // profesional asociada, y aun así debe poder guardarse. La
        // validación de "solo postularse a vacantes de tu carrera" ya no
        // depende de este campo (ver PostulacionService, que usa CarreraId
        // únicamente cuando la vacante lo exige).

        var nuevaEducacion = new Educacion
        {
            CandidatoId = candidato.Id,
            Institucion = educacion.Institucion,
            InstitucionId = educacion.InstitucionId,
            TituloObtenido = educacion.TituloObtenido,
            NivelEducativo = educacion.NivelEducativo,
            FechaInicio = educacion.FechaInicio,
            FechaFin = educacion.FechaFin,
            CategoriaId = educacion.CategoriaId,
            CarreraId = educacion.CarreraId
        };

        // Si eligió una institución del catálogo, el nombre que se guarda y
        // se muestra en todo el resto del sistema es el oficial del
        // catálogo (no lo que haya quedado en el campo de texto, que en la
        // vista se deshabilita/oculta justo cuando se elige del catálogo).
        if (educacion.InstitucionId.HasValue)
        {
            var institucionCatalogo = await _unitOfWork.Instituciones.ObtenerPorIdAsync(educacion.InstitucionId.Value);
            if (institucionCatalogo is not null)
            {
                nuevaEducacion.Institucion = institucionCatalogo.Nombre;
            }
        }

        await _unitOfWork.Educaciones.AgregarAsync(nuevaEducacion);
        await _unitOfWork.GuardarCambiosAsync();
        return nuevaEducacion.Id;
    }

    public async Task EliminarEducacionAsync(string usuarioId, int educacionId)
    {
        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró el perfil de este candidato.");

        var educacion = candidato.Educaciones.FirstOrDefault(e => e.Id == educacionId)
            ?? throw new InvalidOperationException("Esta educación no pertenece a tu perfil.");

        _unitOfWork.Educaciones.Eliminar(educacion);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task AgregarHabilidadAsync(string usuarioId, string nombreHabilidad, string? nivelDominio)
    {
        if (string.IsNullOrWhiteSpace(nombreHabilidad))
        {
            throw new ArgumentException("El nombre de la habilidad es obligatorio.");
        }

        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró el perfil de este candidato.");

        var nombreNormalizado = nombreHabilidad.Trim();
        var habilidadExistente = (await _unitOfWork.Habilidades.ObtenerTodosAsync())
            .FirstOrDefault(h => h.Nombre.Equals(nombreNormalizado, StringComparison.OrdinalIgnoreCase));

        if (habilidadExistente is null)
        {
            habilidadExistente = new Habilidad { Nombre = nombreNormalizado };
            await _unitOfWork.Habilidades.AgregarAsync(habilidadExistente);
            await _unitOfWork.GuardarCambiosAsync();
        }

        var yaAsignada = candidato.Habilidades.Any(ch => ch.HabilidadId == habilidadExistente.Id);
        if (yaAsignada) return;

        await _unitOfWork.CandidatoHabilidades.AgregarAsync(new CandidatoHabilidad
        {
            CandidatoId = candidato.Id,
            HabilidadId = habilidadExistente.Id,
            NivelDominio = nivelDominio
        });

        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task EliminarHabilidadAsync(string usuarioId, int habilidadId)
    {
        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró el perfil de este candidato.");

        var relacion = candidato.Habilidades.FirstOrDefault(ch => ch.HabilidadId == habilidadId)
            ?? throw new InvalidOperationException("Esta habilidad no pertenece a tu perfil.");

        _unitOfWork.CandidatoHabilidades.Eliminar(relacion);
        await _unitOfWork.GuardarCambiosAsync();
    }

    // ---------- Idiomas ----------

    public async Task<List<IdiomaOptionDto>> ObtenerIdiomasDisponiblesAsync()
    {
        var idiomas = await _unitOfWork.Idiomas.ObtenerTodosAsync();
        return idiomas
            .OrderBy(i => i.Nombre)
            .Select(i => new IdiomaOptionDto { Id = i.Id, Nombre = i.Nombre })
            .ToList();
    }

    public async Task AgregarIdiomaAsync(string usuarioId, int idiomaId, string nivel)
    {
        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró el perfil de este candidato.");

        if (await _unitOfWork.Idiomas.ObtenerPorIdAsync(idiomaId) is null)
        {
            throw new InvalidOperationException("El idioma seleccionado no existe.");
        }

        if (!CandidatoIdioma.Niveles.Todos.Contains(nivel))
        {
            throw new InvalidOperationException("El nivel seleccionado no es válido.");
        }

        var existente = candidato.Idiomas.FirstOrDefault(ci => ci.IdiomaId == idiomaId);
        if (existente is not null)
        {
            existente.Nivel = nivel; // ya lo tenía: solo actualiza el nivel
            _unitOfWork.CandidatoIdiomas.Actualizar(existente);
        }
        else
        {
            await _unitOfWork.CandidatoIdiomas.AgregarAsync(new CandidatoIdioma
            {
                CandidatoId = candidato.Id,
                IdiomaId = idiomaId,
                Nivel = nivel
            });
        }

        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task EliminarIdiomaAsync(string usuarioId, int idiomaId)
    {
        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró el perfil de este candidato.");

        var relacion = candidato.Idiomas.FirstOrDefault(ci => ci.IdiomaId == idiomaId)
            ?? throw new InvalidOperationException("Este idioma no pertenece a tu perfil.");

        _unitOfWork.CandidatoIdiomas.Eliminar(relacion);
        await _unitOfWork.GuardarCambiosAsync();
    }

    // ---------- Certificados (PDF) ----------

    public async Task AgregarCertificadoAsync(string usuarioId, string nombre, string? institucionEmisora, DateTime? fechaObtencion, Microsoft.AspNetCore.Http.IFormFile archivoPdf, string tipoDocumento)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del documento es obligatorio.");
        }
        if (archivoPdf is null || archivoPdf.Length == 0)
        {
            throw new ArgumentException("Debes adjuntar el archivo PDF del documento.");
        }
        if (fechaObtencion.HasValue && fechaObtencion.Value.Date > DateTime.UtcNow.Date)
        {
            throw new ArgumentException("La fecha de obtención no puede ser futura.");
        }
        if (string.IsNullOrWhiteSpace(tipoDocumento) || !TiposDocumento.Todos.Contains(tipoDocumento))
        {
            throw new ArgumentException("Selecciona un tipo de documento válido.");
        }

        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró el perfil de este candidato.");

        var rutaArchivo = await _fileStorage.GuardarArchivoAsync(archivoPdf, "certificados");

        var archivo = new Archivo
        {
            UsuarioId = usuarioId,
            TipoArchivo = "Certificado",
            RutaArchivo = rutaArchivo,
            NombreOriginal = archivoPdf.FileName,
            PesoBytes = (int)archivoPdf.Length,
            FechaSubida = DateTime.UtcNow
        };

        await _unitOfWork.Archivos.AgregarAsync(archivo);
        await _unitOfWork.GuardarCambiosAsync(); // para obtener el Id generado

        await _unitOfWork.Certificados.AgregarAsync(new Certificado
        {
            CandidatoId = candidato.Id,
            Nombre = nombre.Trim(),
            InstitucionEmisora = institucionEmisora,
            FechaObtencion = fechaObtencion,
            ArchivoId = archivo.Id,
            TipoDocumento = tipoDocumento,
            FechaCreacion = DateTime.UtcNow
        });

        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task EliminarCertificadoAsync(string usuarioId, int certificadoId)
    {
        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró el perfil de este candidato.");

        var certificado = candidato.Certificados.FirstOrDefault(c => c.Id == certificadoId)
            ?? throw new InvalidOperationException("Este certificado no pertenece a tu perfil.");

        _unitOfWork.Certificados.Eliminar(certificado);
        await _unitOfWork.GuardarCambiosAsync();
    }

    // ---------- Cursos ----------

    public async Task AgregarCursoAsync(string usuarioId, string nombre, string? institucion, int? horasDuracion, DateTime? fechaFinalizacion, Microsoft.AspNetCore.Http.IFormFile? archivoPdf)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del curso es obligatorio.");
        }
        if (horasDuracion.HasValue && horasDuracion.Value <= 0)
        {
            throw new ArgumentException("Las horas de duración deben ser un número positivo.");
        }
        if (fechaFinalizacion.HasValue && fechaFinalizacion.Value.Date > DateTime.UtcNow.Date)
        {
            throw new ArgumentException("La fecha de finalización no puede ser futura.");
        }

        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró el perfil de este candidato.");

        int? archivoId = null;
        if (archivoPdf is not null)
        {
            var rutaArchivo = await _fileStorage.GuardarArchivoAsync(archivoPdf, "cursos");

            var archivo = new Archivo
            {
                UsuarioId = usuarioId,
                TipoArchivo = "Curso",
                RutaArchivo = rutaArchivo,
                NombreOriginal = archivoPdf.FileName,
                PesoBytes = (int)archivoPdf.Length,
                FechaSubida = DateTime.UtcNow
            };

            await _unitOfWork.Archivos.AgregarAsync(archivo);
            await _unitOfWork.GuardarCambiosAsync();
            archivoId = archivo.Id;
        }

        await _unitOfWork.Cursos.AgregarAsync(new Curso
        {
            CandidatoId = candidato.Id,
            Nombre = nombre.Trim(),
            Institucion = institucion,
            HorasDuracion = horasDuracion,
            FechaFinalizacion = fechaFinalizacion,
            ArchivoId = archivoId
        });

        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task EliminarCursoAsync(string usuarioId, int cursoId)
    {
        var candidato = await _unitOfWork.Candidatos.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw new InvalidOperationException("No se encontró el perfil de este candidato.");

        var curso = candidato.Cursos.FirstOrDefault(c => c.Id == cursoId)
            ?? throw new InvalidOperationException("Este curso no pertenece a tu perfil.");

        _unitOfWork.Cursos.Eliminar(curso);
        await _unitOfWork.GuardarCambiosAsync();
    }
}
