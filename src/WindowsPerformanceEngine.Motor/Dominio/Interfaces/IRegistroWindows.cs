using System.Threading.Tasks;

namespace WindowsPerformanceEngine.Motor.Dominio.Interfaces
{
    public interface IRegistroWindows
    {
        Task<object?> LeerValorAsync(string rutaClave, string nombreValor);
        Task<bool> EscribirValorAsync(string rutaClave, string nombreValor, object valor, bool simulacion);
        Task<bool> EliminarValorAsync(string rutaClave, string nombreValor);
        Task<bool> VerificarClaveAsync(string rutaClave);
        Task<bool> EliminarClaveSiVaciaAsync(string rutaClave);
    }
}
