using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;

namespace WindowsPerformanceEngine.Motor.Dominio.Interfaces
{
    public interface IAplicadorRegla
    {
        string IdReglaSoportada { get; }
        Task<bool> EsAplicableAsync();
        Task<Transaccion> AplicarAsync(bool simulacion);
        Task<bool> RevertirAsync(Transaccion transaccion);
    }
}
