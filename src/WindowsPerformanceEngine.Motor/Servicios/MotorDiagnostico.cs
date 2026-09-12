using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Interfaces;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class MotorDiagnostico : IDiagnosticador
    {
        public async Task<ResultadosDiagnostico> EjecutarDiagnosticoActivoAsync()
        {
            return await Task.Run(async () => 
            {
                var resultados = new ResultadosDiagnostico();
                try 
                {
                    using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    using var ramCounter = new PerformanceCounter("Memory", "Available MBytes");

                    // PerformanceCounter necesita leerse 2 veces con delay para mostrar valor real
                    cpuCounter.NextValue();
                    await Task.Delay(1000); 
                    
                    resultados.PorcentajeUsoCpu = cpuCounter.NextValue();
                    resultados.MemoriaDisponibleMB = ramCounter.NextValue();

                    // Lógica real de diagnóstico básico
                    if (resultados.PorcentajeUsoCpu > 85)
                    {
                        resultados.EsLimitadoPorCpu = true;
                        resultados.ConclusionDiagnostico += "El sistema muestra alta contención de CPU. ";
                    }
                    if (resultados.MemoriaDisponibleMB < 2048) // Menos de 2GB
                    {
                        resultados.EsLimitadoPorRam = true;
                        resultados.ConclusionDiagnostico += "Existe alta presión de memoria (RAM). ";
                    }
                    
                    if (!resultados.EsLimitadoPorCpu && !resultados.EsLimitadoPorRam)
                    {
                        resultados.ConclusionDiagnostico = "Estado del sistema óptimo en reposo.";
                    }
                }
                catch (Exception ex)
                {
                    resultados.ConclusionDiagnostico = "Error al leer contadores: " + ex.Message;
                }
                return resultados;
            });
        }
    }
}
