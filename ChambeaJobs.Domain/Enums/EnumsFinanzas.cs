namespace ChambeaJobs.Domain.Enums;

public enum TipoCategoriaFinanciera
{
    Ingreso = 1,
    Gasto = 2,
    Activo = 3,
    Pasivo = 4,
    Patrimonio = 5
}

/// <summary>Un ingreso/gasto activo cuenta en los reportes; uno anulado
/// queda guardado (nunca se borra) pero deja de sumar.</summary>
public enum EstadoMovimientoFinanciero
{
    Activo = 1,
    Anulado = 2
}

public enum TipoPeriodoFinanciero
{
    Mensual = 1,
    Anual = 2
}

public enum MonedaFinanciera
{
    USD = 1,
    NIO = 2
}

public enum TipoEntidadAjuste
{
    Ingreso = 1,
    Gasto = 2
}
