using System;
using System.Collections.Generic;
using System.Linq;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class MotorDecision
    {
        public DecisionOptimizacion EvaluarRegla(ReglaOptimizacion regla, InformacionHardware hwInfo)
        {
            // 1. Evaluar Evidencia
            if (regla.Evidencia.Nivel == NivelEvidencia.Insuficiente || regla.Evidencia.Confianza == 0)
            {
                return DecisionOptimizacion.NoHayEvidenciaSuficiente;
            }

            // 2. Evaluar Riesgo (Policy base - Se podría inyectar una política de usuario)
            if (regla.Riesgo == NivelRiesgo.Extreme)
            {
                return DecisionOptimizacion.RequiereConfirmacionManual;
            }

            if (regla.Riesgo == NivelRiesgo.Experimental && regla.Evidencia.Confianza < 80)
            {
                return DecisionOptimizacion.RiesgoDemasiadoAlto;
            }

            // 3. Evaluar Compatibilidad
            if (regla.VersionesWindowsSoportadas.Any() && !regla.VersionesWindowsSoportadas.Contains("All"))
            {
                bool buildSoportada = regla.VersionesWindowsSoportadas.Any(v => hwInfo.OsBuild.Contains(v));
                if (!buildSoportada) return DecisionOptimizacion.NoCompatible;
            }

            // 4. Evaluar Exclusiones
            foreach (var exclusion in regla.CondicionesExclusion)
            {
                if (exclusion.Equals("Laptop", StringComparison.OrdinalIgnoreCase) && hwInfo.EsPortatil)
                {
                    return DecisionOptimizacion.NoCompatible;
                }
                if (exclusion.Equals("Desktop", StringComparison.OrdinalIgnoreCase) && !hwInfo.EsPortatil)
                {
                    return DecisionOptimizacion.NoCompatible;
                }
                
                if (exclusion.Contains("SSD", StringComparison.OrdinalIgnoreCase) && 
                    hwInfo.Discos.Any(d => d.TipoMedio.Contains("SSD", StringComparison.OrdinalIgnoreCase)))
                {
                    return DecisionOptimizacion.NoCompatible;
                }
            }

            // 5. Reglas específicas
            if (regla.Id == "WIN-HAGS-001")
            {
                // WMI DriverVersion format: X.X.X.X. WDDM 2.7 = Windows 10 2004+ AND Driver Support.
                // We don't have driver WDDM string directly in WMI, only OS build and DriverVersion.
                // To be honest, we cannot accurately determine WDDM 2.7 from simple WMI queries without native API calls (DXGI).
                // So we MUST return NoDeterminado as ordered.
                return DecisionOptimizacion.NoDeterminado;
            }

            return DecisionOptimizacion.Optimizar;
        }
    }
}
