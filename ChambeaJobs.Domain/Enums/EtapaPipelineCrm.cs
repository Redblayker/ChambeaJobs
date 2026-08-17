namespace ChambeaJobs.Domain.Enums;

public enum EtapaPipelineCrm
{
    Prospecto = 1,
    PrimerContacto = 2,
    PresentacionRealizada = 3,
    Demostracion = 4,
    Negociacion = 5,
    EnEspera = 6,
    ClienteActivo = 7,
    ClienteInactivo = 8,
    Cancelado = 9,
    EmpresaVerificada = 10,
    EmpresaActiva = 11,
    ClienteCancelado = 12
}

public enum TipoActividadCrm
{
    Llamada = 1,
    Correo = 2,
    Reunion = 3,
    Videollamada = 4,
    Visita = 5,
    Seguimiento = 6,
    Recordatorio = 7,
    Nota = 8
}
