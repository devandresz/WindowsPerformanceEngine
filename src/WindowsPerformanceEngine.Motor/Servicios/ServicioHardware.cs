using System;
using System.Management;
using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Interfaces;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class ServicioHardware : IServicioHardware
    {
        private readonly IDiagnosticador _diagnosticador;
        private readonly MotorMonitor _motorMonitor;

        public ServicioHardware(IDiagnosticador diagnosticador)
        {
            _diagnosticador = diagnosticador;
            _motorMonitor = new MotorMonitor();
        }

        public async Task<InformacionHardware> ObtenerInformacionAsync()
        {
            return await Task.Run(() => 
            {
                var info = new InformacionHardware();
                
                try 
                {
                    // WMI: OS Info
                    using (var searcherOs = new ManagementObjectSearcher("SELECT Caption, BuildNumber, OSArchitecture FROM Win32_OperatingSystem"))
                    {
                        foreach (var obj in searcherOs.Get())
                        {
                            info.OsNombre = obj["Caption"]?.ToString() ?? "Windows";
                            info.OsBuild = obj["BuildNumber"]?.ToString() ?? "0";
                            info.OsArquitectura = obj["OSArchitecture"]?.ToString() ?? "x64";
                            break;
                        }
                    }

                    // WMI: Enclosure (Chassis) para Desktop vs Laptop
                    using (var searcherChassis = new ManagementObjectSearcher("SELECT ChassisTypes FROM Win32_SystemEnclosure"))
                    {
                        foreach (var obj in searcherChassis.Get())
                        {
                            var chassis = obj["ChassisTypes"] as ushort[];
                            if (chassis != null && chassis.Length > 0)
                            {
                                int type = chassis[0];
                                // 8=Portable, 9=Laptop, 10=Notebook, 11=HandHeld, 12=DockingStation, 14=SubNotebook, 30=Tablet
                                info.EsPortatil = (type >= 8 && type <= 14) || type == 30;
                            }
                            break;
                        }
                    }

                    // WMI: Procesador
                    using (var searcherCpu = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, Architecture, Manufacturer FROM Win32_Processor"))
                    {
                        foreach (var obj in searcherCpu.Get())
                        {
                            info.ProcesadorNombre = obj["Name"]?.ToString() ?? "Desconocido";
                            info.ProcesadorFabricante = obj["Manufacturer"]?.ToString() ?? "Desconocido";

                            info.ProcesadorNucleosFisicos = Convert.ToUInt32(obj["NumberOfCores"] ?? 0);
                            info.ProcesadorHilos = Convert.ToUInt32(obj["NumberOfLogicalProcessors"] ?? 0);
                            info.VelocidadBaseMHz = Convert.ToUInt32(obj["MaxClockSpeed"] ?? 0);
                            int arch = Convert.ToInt32(obj["Architecture"] ?? 9);
                            info.Arquitectura = arch == 9 ? "x64" : (arch == 12 ? "ARM64" : "x86");
                            break;
                        }
                    }
                    
                    var monitorConfig = _motorMonitor.ObtenerConfiguracionMonitorPrincipal();
                    info.MonitorResolucionX = monitorConfig.ResolucionX;
                    info.MonitorResolucionY = monitorConfig.ResolucionY;
                    info.MonitorFrecuenciaHz = monitorConfig.FrecuenciaActualHz;
                    
                    // WMI: RAM
                    using (var searcherRam = new ManagementObjectSearcher("SELECT Capacity, Speed FROM Win32_PhysicalMemory"))
                    {
                        ulong totalRam = 0;
                        uint maxSpeed = 0;
                        foreach (var obj in searcherRam.Get())
                        {
                            totalRam += Convert.ToUInt64(obj["Capacity"] ?? 0);
                            uint s = Convert.ToUInt32(obj["Speed"] ?? 0);
                            if (s > maxSpeed) maxSpeed = s;
                        }
                        if (totalRam > 0) info.MemoriaTotalGB = $"{totalRam / (1024 * 1024 * 1024)} GB";
                        info.VelocidadMemoriaMHz = maxSpeed;
                    }

                    using var searcherBase = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard");
                    var baseBoard = searcherBase.Get().Cast<ManagementObject>().FirstOrDefault();
                    if (baseBoard != null)
                    {
                        info.PlacaBaseFabricante = baseBoard["Manufacturer"]?.ToString() ?? "Desconocido";
                        info.PlacaBaseModelo = baseBoard["Product"]?.ToString() ?? "Desconocido";
                    }

                    // WMI: GPU
                    using (var searcherGpu = new ManagementObjectSearcher("SELECT Name, DriverVersion FROM Win32_VideoController"))
                    {
                        foreach (var obj in searcherGpu.Get())
                        {
                            string gpuName = obj["Name"]?.ToString() ?? "Desconocido";
                            info.TarjetasGraficas.Add(gpuName);
                            if (info.ControladorGrafico == "Desconocido")
                                info.ControladorGrafico = obj["DriverVersion"]?.ToString() ?? "Desconocido";
                        }
                    }

                    // WMI: Discos
                    using (var searcherDisk = new ManagementObjectSearcher("SELECT Model, InterfaceType, MediaType FROM Win32_DiskDrive"))
                    {
                        foreach (var obj in searcherDisk.Get())
                        {
                            info.Discos.Add(new DiscoInfo 
                            {
                                Modelo = obj["Model"]?.ToString() ?? "Desconocido",
                                Interfaz = obj["InterfaceType"]?.ToString() ?? "Desconocido",
                                TipoMedio = obj["MediaType"]?.ToString() ?? "Desconocido"
                            });
                        }
                    }

                    // WMI: Red (Activas)
                    using (var searcherNet = new ManagementObjectSearcher("SELECT Name, Speed FROM Win32_NetworkAdapter WHERE NetEnabled=True"))
                    {
                        foreach (var obj in searcherNet.Get())
                        {
                            info.AdaptadoresRed.Add(new RedInfo
                            {
                                Nombre = obj["Name"]?.ToString() ?? "Desconocido",
                                Velocidad = Convert.ToUInt64(obj["Speed"] ?? 0)
                            });
                        }
                    }
                }
                catch
                {
                    // Fallback si WMI falla masivamente
                }
                
                return info;
            });
        }
    }
}
