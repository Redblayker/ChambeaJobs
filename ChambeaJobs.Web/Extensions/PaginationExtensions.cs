using Microsoft.AspNetCore.Mvc;

namespace ChambeaJobs.Web.Extensions;

public static class PaginationExtensions
{
    public const int TamanoPaginaPredeterminado = 10;

    public static List<T> Paginar<T>(this Controller controller, IEnumerable<T> elementos, int pagina)
    {
        var lista = elementos.ToList();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(lista.Count / (double)TamanoPaginaPredeterminado));
        var paginaActual = Math.Clamp(pagina, 1, totalPaginas);

        controller.ViewData["PaginaActual"] = paginaActual;
        controller.ViewData["TotalPaginas"] = totalPaginas;
        controller.ViewData["TotalElementos"] = lista.Count;

        return lista
            .Skip((paginaActual - 1) * TamanoPaginaPredeterminado)
            .Take(TamanoPaginaPredeterminado)
            .ToList();
    }
}
