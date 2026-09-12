using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;

namespace WindowsPerformanceEngine.Motor.Dominio.Interfaces
{
    public interface IBaseDatosOptimizacion
    {
        Task<(List<ReglaOptimizacion> Reglas, bool CargadoDesdeDisco)> CargarReglasAsync();
        Task<List<ReglaOptimizacion>> RestaurarReglasPorDefectoAsync();
    }
}
