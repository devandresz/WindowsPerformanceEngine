using System;
using System.Diagnostics;
using System.Linq;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class MotorRendimientoGpu
    {
        // En Windows, las GPUs 3D exponen PerformanceCounters bajo "GPU Engine"
        public (double UtilizacionGpu, double UtilizacionVram) MedirUtilizacionActual()
        {
            try
            {
                var category = new PerformanceCounterCategory("GPU Engine");
                var instanceNames = category.GetInstanceNames().Where(i => i.Contains("engtype_3D")).ToList();
                
                double gpuUtil = 0;
                foreach (var instance in instanceNames)
                {
                    using var pc = new PerformanceCounter("GPU Engine", "Utilization Percentage", instance, true);
                    pc.NextValue(); // Primer valor es siempre 0
                    System.Threading.Thread.Sleep(100);
                    gpuUtil += pc.NextValue();
                }

                double vramGb = -1;
                try
                {
                    using var baseKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\VIDEO");
                    if (baseKey != null)
                    {
                        var video0 = baseKey.GetValue(@"\Device\Video0") as string;
                        if (!string.IsNullOrEmpty(video0))
                        {
                            // video0 is usually like \Registry\Machine\System\CurrentControlSet\Control\Video\{GUID}\0000
                            string targetPath = video0.Replace(@"\Registry\Machine\", "", StringComparison.OrdinalIgnoreCase);
                            using var vramKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(targetPath);
                            if (vramKey != null)
                            {
                                var qwMem = vramKey.GetValue("HardwareInformation.qwMemorySize");
                                if (qwMem != null)
                                {
                                    double bytes = Convert.ToDouble(qwMem);
                                    vramGb = bytes / (1024 * 1024 * 1024);
                                }
                                else
                                {
                                    var dwMem = vramKey.GetValue("HardwareInformation.MemorySize");
                                    if (dwMem != null)
                                    {
                                        double bytes = Convert.ToDouble(dwMem);
                                        vramGb = bytes / (1024 * 1024 * 1024);
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }

                return (gpuUtil, vramGb);
            }
            catch
            {
                return (-1, -1);
            }
        }
    }
}
