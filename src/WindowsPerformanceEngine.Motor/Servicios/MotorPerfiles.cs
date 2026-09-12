using System.Collections.Generic;
using System.Linq;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;
using WindowsPerformanceEngine.Motor.Dominio.Interfaces;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class MotorPerfiles
    {
        public class ResultadoPerfil
        {
            public TipoPerfil PerfilSugerido { get; set; }
            public int Confianza { get; set; }
            public List<string> Razones { get; set; } = new();
            public List<string> SensoresUsados { get; set; } = new();
            public List<string> SensoresNoDisponibles { get; set; } = new();
        }

        public ResultadoPerfil DetectarPerfilAutomatico(InformacionHardware hw, ResultadosDiagnostico diag)
        {
            var resultado = new ResultadoPerfil();
            
            int maxPuntosPosibles = 0;
            int puntosObtenidos = 0;

            // 1. Detección de Portátil vs Desktop (WMI Win32_ComputerSystem)
            if (hw.EsPortatil)
            {
                resultado.SensoresUsados.Add("Win32_ComputerSystem (EsPortatil=True)");
                resultado.Razones.Add("Factor de forma: Laptop.");
                puntosObtenidos += 10;
                maxPuntosPosibles += 10;
            }
            else
            {
                resultado.SensoresUsados.Add("Win32_ComputerSystem (EsPortatil=False)");
            }

            // 2. Detección de GPU (Win32_VideoController)
            bool tieneGpuDedicada = hw.TarjetasGraficas.Any(g => g.Contains("NVIDIA", System.StringComparison.OrdinalIgnoreCase) || 
                                                                 g.Contains("Radeon", System.StringComparison.OrdinalIgnoreCase) || 
                                                                 g.Contains("Arc", System.StringComparison.OrdinalIgnoreCase));
            
            if (hw.TarjetasGraficas.Count > 0)
            {
                resultado.SensoresUsados.Add("Win32_VideoController");
                maxPuntosPosibles += 20;
                if (tieneGpuDedicada)
                {
                    puntosObtenidos += 20;
                    resultado.Razones.Add("GPU Dedicada detectada.");
                }
            }
            else
            {
                resultado.SensoresNoDisponibles.Add("Win32_VideoController");
            }

            // 3. Detección de CPU (Win32_Processor)
            if (hw.ProcesadorNucleosFisicos > 0)
            {
                resultado.SensoresUsados.Add("Win32_Processor");
                maxPuntosPosibles += 20;
                if (hw.ProcesadorNucleosFisicos >= 8)
                {
                    puntosObtenidos += 20;
                    resultado.Razones.Add($"CPU High-End ({hw.ProcesadorNucleosFisicos} núcleos).");
                }
            }
            else
            {
                resultado.SensoresNoDisponibles.Add("Win32_Processor");
            }

            // 4. Memoria RAM (Win32_PhysicalMemory)
            if (!string.IsNullOrEmpty(hw.MemoriaTotalGB))
            {
                resultado.SensoresUsados.Add("Win32_PhysicalMemory");
                maxPuntosPosibles += 10;
                if (hw.MemoriaTotalGB.Contains("32") || hw.MemoriaTotalGB.Contains("64") || hw.MemoriaTotalGB.Contains("128"))
                {
                    puntosObtenidos += 10;
                    resultado.Razones.Add("RAM Masiva (>=32GB).");
                }
            }
            else
            {
                resultado.SensoresNoDisponibles.Add("Win32_PhysicalMemory");
            }

            // Calcular Confianza real basada en cuántos sensores pudimos leer
            int totalSensores = resultado.SensoresUsados.Count + resultado.SensoresNoDisponibles.Count;
            resultado.Confianza = totalSensores == 0 ? 0 : (resultado.SensoresUsados.Count * 100) / totalSensores;

            // Determinar perfil
            if (hw.EsPortatil)
            {
                resultado.PerfilSugerido = tieneGpuDedicada ? TipoPerfil.LaptopPerformance : TipoPerfil.LaptopBattery;
            }
            else
            {
                if (puntosObtenidos >= 40)
                    resultado.PerfilSugerido = TipoPerfil.GamingStreaming;
                else if (puntosObtenidos >= 20)
                    resultado.PerfilSugerido = TipoPerfil.GamingBalanced;
                else
                    resultado.PerfilSugerido = TipoPerfil.Office;
            }

            return resultado;
        }
    }
}
