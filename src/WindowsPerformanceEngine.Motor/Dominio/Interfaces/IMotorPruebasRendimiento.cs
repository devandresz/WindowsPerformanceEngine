using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;

namespace WindowsPerformanceEngine.Motor.Dominio.Interfaces
{
    public interface IMotorPruebasRendimiento
    {
        Task<ResultadoPrueba> EjecutarPruebaRapidaAsync(string pingHost = "8.8.8.8");
        Task<double> MedirLatenciaRedAsync(string host = "8.8.8.8");
    }
}
