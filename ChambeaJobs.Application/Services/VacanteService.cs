using ChambeaJobs.Application.DTOs;
using ChambeaJobs.Application.Interfaces;
using ChambeaJobs.Domain.Entities;

namespace ChambeaJobs.Application.Services;

/// <inheritdoc cref="IVacanteService"/>
public class VacanteService : IVacanteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaqueteEmpresaService _paqueteService;
    private readonly ChambeaJobs.Application.Eventos.IPublicadorEventos _publicadorEventos;

    public VacanteService(IUnitOfWork unitOfWork, IPaqueteEmpresaService paqueteService, ChambeaJobs.Application.Eventos.IPublicadorEventos publicadorEventos)
    {
        _unitOfWork = unitOfWork;
        _paqueteService = paqueteService;
        _publicadorEventos = publicadorEventos;
    }

    public async Task<int> PublicarVacanteAsync(int empresaId, VacanteFormDto datos)
    {
        ValidarFechasYSalarios(datos);

        // Regla de negocio central del proyecto: sin paquete vigente con cupo,
        // no se puede publicar. Se re-verifica aquí en el servidor aunque la
        // vista ya deshabilite el botón, porque la UI nunca es la única defensa.
        var paquete = await _unitOfWork.PaquetesEmpresa.ObtenerVigentePorEmpresaAsync(empresaId);
        if (paquete is null || !paquete.TieneCupoDisponible())
        {
            throw new InvalidOperationException(
                "No tienes un paquete de publicación vigente con cupos disponibles. Compra o renueva tu paquete para publicar esta vacante.");
        }

        // "Destacada" solo se respeta si el plan de la empresa lo permite —
        // si alguien manda el campo en true sin tener el plan correcto (ej.
        // saltándose la casilla oculta en el formulario), se ignora en
        // silencio en vez de bloquear toda la publicación de la vacante.
        var permiteDestacadas = paquete.PlanSuscripcion?.PermiteVacantesDestacadas ?? false;

        var vacante = new Vacante
        {
            EmpresaId = empresaId,
            CategoriaId = datos.CategoriaId,
            CarreraId = datos.CarreraId,
            UbicacionId = datos.UbicacionId,
            PaqueteEmpresaId = paquete.Id,
            Titulo = datos.Titulo,
            Descripcion = datos.Descripcion,
            Requisitos = datos.Requisitos,
            Modalidad = datos.Modalidad,
            ExperienciaRequerida = datos.ExperienciaRequerida,
            SalarioMin = datos.SalarioMin,
            SalarioMax = datos.SalarioMax,
            FechaPublicacion = DateTime.UtcNow,
            FechaCierre = datos.FechaCierre,
            Estado = EstadosVacante.Activa,
            EsDestacada = datos.EsDestacada && permiteDestacadas
        };

        await _unitOfWork.Vacantes.AgregarAsync(vacante);

        // Consumir un cupo del paquete y marcarlo Agotado si ya no quedan.
        // Nota: si VacantesIncluidas es null (plan Empresarial), esta comparación
        // siempre da "false" en C# (comparar con null nunca es >=), así que un
        // plan ilimitado nunca se marca como Agotado — es el comportamiento correcto.
        paquete.VacantesConsumidas += 1;
        if (paquete.VacantesConsumidas >= paquete.VacantesIncluidas)
        {
            paquete.Estado = EstadosPaquete.Agotado;
        }
        _unitOfWork.PaquetesEmpresa.Actualizar(paquete);

        await _unitOfWork.GuardarCambiosAsync();

        await _publicadorEventos.PublicarAsync(new ChambeaJobs.Application.Eventos.VacantePublicadaEvento(empresaId, vacante.Id));

        return vacante.Id;
    }

    public async Task ActualizarVacanteAsync(int empresaId, VacanteFormDto datos)
    {
        var vacante = await ObtenerVacanteDeEmpresaAsync(empresaId, datos.Id);

        ValidarFechasYSalarios(datos);

        vacante.Titulo = datos.Titulo;
        vacante.CategoriaId = datos.CategoriaId;
        vacante.CarreraId = datos.CarreraId;
        vacante.UbicacionId = datos.UbicacionId;
        vacante.Modalidad = datos.Modalidad;
        vacante.ExperienciaRequerida = datos.ExperienciaRequerida;
        vacante.Descripcion = datos.Descripcion;
        vacante.Requisitos = datos.Requisitos;
        vacante.SalarioMin = datos.SalarioMin;
        vacante.SalarioMax = datos.SalarioMax;
        vacante.FechaCierre = datos.FechaCierre;

        var paqueteVigente = await _unitOfWork.PaquetesEmpresa.ObtenerVigentePorEmpresaAsync(empresaId);
        var permiteDestacadas = paqueteVigente?.PlanSuscripcion?.PermiteVacantesDestacadas ?? false;
        vacante.EsDestacada = datos.EsDestacada && permiteDestacadas;

        _unitOfWork.Vacantes.Actualizar(vacante);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task CerrarVacanteAsync(int empresaId, int vacanteId)
    {
        var vacante = await ObtenerVacanteDeEmpresaAsync(empresaId, vacanteId);
        vacante.Estado = EstadosVacante.Cerrada;
        _unitOfWork.Vacantes.Actualizar(vacante);
        await _unitOfWork.GuardarCambiosAsync();
    }

    public async Task EliminarVacanteAsync(int empresaId, int vacanteId)
    {
        var vacante = await ObtenerVacanteDeEmpresaAsync(empresaId, vacanteId);
        _unitOfWork.Vacantes.Eliminar(vacante);
        await _unitOfWork.GuardarCambiosAsync();

        // Nota: no se libera el cupo consumido del paquete al eliminar,
        // ya que el cupo se considera "usado" desde el momento de publicar
        // (regla de negocio consistente con "10 vacantes por paquete",
        // no "10 vacantes activas simultáneas").
    }

    public async Task<List<VacanteListItemDto>> ObtenerVacantesDeEmpresaAsync(int empresaId)
    {
        var vacantes = await _unitOfWork.Vacantes.ObtenerPorEmpresaAsync(empresaId);
        var resultado = new List<VacanteListItemDto>();

        foreach (var v in vacantes.OrderByDescending(v => v.FechaPublicacion))
        {
            var postulantes = await _unitOfWork.Postulaciones.ObtenerPorVacanteAsync(v.Id);
            resultado.Add(new VacanteListItemDto
            {
                Id = v.Id,
                Titulo = v.Titulo,
                CategoriaNombre = v.Categoria?.Nombre ?? string.Empty,
                FechaPublicacion = v.FechaPublicacion,
                Estado = v.Estado,
                NumeroPostulantes = postulantes.Count(),
                EsDestacada = v.EsDestacada
            });
        }

        return resultado;
    }

    public async Task<VacanteFormDto?> ObtenerParaEditarAsync(int empresaId, int vacanteId)
    {
        var vacante = await ObtenerVacanteDeEmpresaAsync(empresaId, vacanteId, lanzarSiNoExiste: false);
        if (vacante is null) return null;

        return new VacanteFormDto
        {
            Id = vacante.Id,
            Titulo = vacante.Titulo,
            CategoriaId = vacante.CategoriaId,
            CarreraId = vacante.CarreraId,
            UbicacionId = vacante.UbicacionId,
            Modalidad = vacante.Modalidad,
            ExperienciaRequerida = vacante.ExperienciaRequerida,
            Descripcion = vacante.Descripcion,
            Requisitos = vacante.Requisitos,
            SalarioMin = vacante.SalarioMin,
            SalarioMax = vacante.SalarioMax,
            FechaCierre = vacante.FechaCierre,
            EsDestacada = vacante.EsDestacada
        };
    }

    public async Task<VacanteDetalleDto?> ObtenerDetalleAsync(int vacanteId)
    {
        var vacante = await _unitOfWork.Vacantes.ObtenerConDetalleAsync(vacanteId);
        if (vacante is null) return null;

        return MapearDetalle(vacante);
    }

    public async Task<List<VacanteDetalleDto>> BuscarAsync(string? palabraClave, int? categoriaId, int? ubicacionId, string? modalidad)
    {
        var resultados = await _unitOfWork.Vacantes.BuscarAsync(palabraClave, categoriaId, ubicacionId, modalidad);
        return resultados.Select(MapearDetalle).ToList();
    }

    // ---------- Auxiliares privados ----------

    private async Task<Vacante> ObtenerVacanteDeEmpresaAsync(int empresaId, int vacanteId, bool lanzarSiNoExiste = true)
    {
        var vacante = await _unitOfWork.Vacantes.ObtenerConDetalleAsync(vacanteId);

        if (vacante is null || vacante.EmpresaId != empresaId)
        {
            if (lanzarSiNoExiste)
            {
                throw new InvalidOperationException("Esta vacante no existe o no te pertenece.");
            }
            return null!;
        }

        return vacante;
    }

    private static void ValidarFechasYSalarios(VacanteFormDto datos)
    {
        if (datos.FechaCierre.Date <= DateTime.UtcNow.Date)
        {
            throw new ArgumentException("La fecha de cierre debe ser posterior a hoy.");
        }

        if (datos.SalarioMin.HasValue && datos.SalarioMax.HasValue && datos.SalarioMax < datos.SalarioMin)
        {
            throw new ArgumentException("El salario máximo no puede ser menor al salario mínimo.");
        }
    }

    private static VacanteDetalleDto MapearDetalle(Vacante v) => new()
    {
        Id = v.Id,
        Titulo = v.Titulo,
        EmpresaId = v.EmpresaId,
        EmpresaNombre = v.Empresa?.NombreEmpresa ?? string.Empty,
        EmpresaLogoUrl = v.Empresa?.LogoArchivo?.RutaArchivo,
        CategoriaNombre = v.Categoria?.Nombre ?? string.Empty,
        UbicacionNombre = v.Ubicacion?.NombreCompleto ?? string.Empty,
        Modalidad = v.Modalidad,
        ExperienciaRequerida = v.ExperienciaRequerida,
        SalarioMin = v.SalarioMin,
        SalarioMax = v.SalarioMax,
        Descripcion = v.Descripcion,
        Requisitos = v.Requisitos,
        FechaPublicacion = v.FechaPublicacion,
        FechaCierre = v.FechaCierre,
        Estado = v.Estado,
        EsDestacada = v.EsDestacada
    };
}
