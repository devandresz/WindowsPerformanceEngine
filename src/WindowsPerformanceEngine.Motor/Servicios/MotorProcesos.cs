using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class MotorProcesos
    {
        public class ProcesoAnalizado
        {
            public string Nombre { get; set; } = string.Empty;
            public string Categoria { get; set; } = string.Empty;
            public double UsoCpuPorcentaje { get; set; }
            public long MemoriaMb { get; set; }
            public string Recomendacion { get; set; } = string.Empty;
        }

        public List<ProcesoAnalizado> AnalizarProcesosSegundoPlano()
        {
            var resultados = new List<ProcesoAnalizado>();
            var procesos = Process.GetProcesses();

            foreach (var p in procesos)
            {
                try
                {
                    string pName = p.ProcessName.ToLower();
                    if (pName == "idle" || pName == "system" || pName.Contains("svchost") || pName.Contains("csrss") || pName.Contains("smss") || pName.Contains("wininit") || pName.Contains("lsass"))
                    {
                        continue; // Excluir críticos
                    }

                    long mem = p.WorkingSet64 / (1024 * 1024);
                    
                    if (mem > 0)
                    {
                        resultados.Add(new ProcesoAnalizado
                        {
                            Nombre = p.ProcessName,
                            Categoria = "Proceso de Alto Consumo de RAM",
                            MemoriaMb = mem,
                            Recomendacion = "Verificar si es necesario mantenerlo en ejecución dado su alto consumo de memoria RAM."
                        });
                    }
                }
                catch
                {
                    // Acceso denegado
                }
            }

            // Agrupar por nombre para evitar duplicados múltiples (como multiples Chrome) y sumar su RAM
            var agrupados = resultados
                .GroupBy(r => r.Nombre)
                .Select(g => new ProcesoAnalizado
                {
                    Nombre = g.Key,
                    Categoria = g.First().Categoria,
                    MemoriaMb = g.Sum(x => x.MemoriaMb),
                    Recomendacion = g.First().Recomendacion
                })
                .OrderByDescending(r => r.MemoriaMb)
                .Take(5)
                .ToList();

            return agrupados;
        }
    }
}
