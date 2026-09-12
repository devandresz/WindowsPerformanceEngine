using System.Threading.Tasks;
using System.Collections.Generic;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;

namespace WindowsPerformanceEngine.Motor.Dominio.Interfaces
{
    public interface IGestorTransacciones
    {
        Task<Transaccion> RegistrarInicioAsync(string idRegla, bool simulacion);
        Task ConfirmarAsync(Transaccion transaccion);
        Task<bool> RevertirUltimaAsync();
        Task<bool> RevertirEspecificaAsync(string idTransaccion);
        Task<IReadOnlyList<Transaccion>> ObtenerHistorialAsync();
    }
}
