using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;

namespace WindowsPerformanceEngine.Motor.Dominio.Interfaces
{
    public interface IServicioHardware
    {
        Task<InformacionHardware> ObtenerInformacionAsync();
    }
}
