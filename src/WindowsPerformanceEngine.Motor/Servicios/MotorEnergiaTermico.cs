using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class MotorEnergiaTermico
    {
        public class DiagnosticoTermico
        {
            public string PlanEnergiaActivo { get; set; } = "Desconocido";
            public bool ExisteThermalThrottling { get; set; }
            public double TemperaturaTermicaAcpi { get; set; } = -1;
            public string Recomendacion { get; set; } = string.Empty;
        }

        public DiagnosticoTermico AnalizarEstadoTermico()
        {
            var diag = new DiagnosticoTermico();

            // 1. Detectar Plan de Energía activo vía WMI o powercfg (Win32_PowerPlan)
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\cimv2\power", "SELECT ElementName FROM Win32_PowerPlan WHERE IsActive = True");
                var activePlan = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (activePlan != null)
                {
                    diag.PlanEnergiaActivo = activePlan["ElementName"]?.ToString() ?? "Desconocido";
                }
            }
            catch
            {
                // Fallback
            }

            // 2. Detectar Temperatura y Throttling
            // Win32_PerfFormattedData_Counters_ThermalZoneInformation
            try
            {
                using var searcherTermal = new ManagementObjectSearcher("SELECT HighPrecisionTemperature, PercentPassiveLimit FROM Win32_PerfFormattedData_Counters_ThermalZoneInformation");
                var thermalZones = searcherTermal.Get().Cast<ManagementObject>().ToList();
                
                if (thermalZones.Count > 0)
                {
                    double maxTempK = 0;
                    double totalPassiveLimit = 0;

                    foreach (var zone in thermalZones)
                    {
                        double kelvin = Convert.ToDouble(zone["HighPrecisionTemperature"] ?? 2732) / 10.0;
                        if (kelvin > maxTempK) maxTempK = kelvin;
                        
                        totalPassiveLimit += Convert.ToDouble(zone["PercentPassiveLimit"] ?? 0);
                    }

                    diag.TemperaturaTermicaAcpi = (maxTempK - 273.15); // Convertir Kelvin a Celsius
                    
                    // Si el sistema está limitando la CPU de forma pasiva debido al calor (umbral de 90°C)
                    if (diag.TemperaturaTermicaAcpi >= 90.0)
                    {
                        diag.ExisteThermalThrottling = true;
                        diag.Recomendacion = "¡ALERTA TÉRMICA! El sistema está sufriendo estrangulamiento térmico (Throttling). Las optimizaciones que aumenten el uso de CPU serán bloqueadas para proteger tu hardware.";
                    }
                    else
                    {
                        diag.Recomendacion = "Temperaturas de la zona térmica ACPI bajo control.";
                    }
                }
                else
                {
                    // Fallback para WMI restringido
                    diag.TemperaturaTermicaAcpi = -1; // Sensor no disponible
                    diag.Recomendacion = "Sensores térmicos WMI requieren privilegios elevados o soporte ACPI directo.";
                }
            }
            catch
            {
                diag.TemperaturaTermicaAcpi = -1;
            }

            return diag;
        }
    }
}
