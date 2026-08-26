using ChambeaJobs.Domain.Entities;
using System.Collections;

namespace ChambeaJobs.Application.Interfaces;

public interface IPasantiaRepository : IRepository<Pasantia>
{
    Task<Pasantia?> ObtenerConDetalleAsync(int id);
    Task<IEnumerable<Pasantia>> ObtenerPorEmpresaAsync(int empresaId);
    Task<IEnumerable<Pasantia>> ObtenerTodasConDetalleAsync();
    Task<IEnumerable<Pasantia>> BuscarAsync(string? palabraClave, int? categoriaId, int? ubicacionId, string? modalidad);
}

public interface IPostulacionPasantiaRepository : IRepository<PostulacionPasantia>
{
    Task<bool> ExistePostulacionAsync(int candidatoId, int pasantiaId);
    Task<IEnumerable<PostulacionPasantia>> ObtenerPorCandidatoAsync(int candidatoId);
    Task<IEnumerable<PostulacionPasantia>> ObtenerPorPasantiaAsync(int pasantiaId);
    Task<PostulacionPasantia?> ObtenerConDetalleAsync(int id);

    //Agregado por allan para correcion de metodos con patrones N+1
    Task<Dictionary<int,int>> ContarPorPasantiasAsync(IEnumerable<int> pasantiaIds);
    Task<Dictionary<int,int>> ObtenerPorPasantiaAsync(IEnumerable<int> pasantiaId);
}
