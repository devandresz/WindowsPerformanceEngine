using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Interfaces;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;
using System.Collections.Generic;
using System.Linq;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class MotorPruebasRendimiento : IMotorPruebasRendimiento
    {
        private readonly IDiagnosticador _diagnosticador;
        private readonly MotorFpsEtw _motorFps;

        public MotorPruebasRendimiento(IDiagnosticador diagnosticador, MotorFpsEtw motorFps)
        {
            _diagnosticador = diagnosticador;
            _motorFps = motorFps;
        }

        public async Task<ResultadoPrueba> EjecutarPruebaRapidaAsync(string pingHost = "8.8.8.8")
        {
            return await Task.Run(async () => 
            {
                var resultado = new ResultadoPrueba();
                
                // Iniciar captura ETW asíncrona
                _motorFps.IniciarCaptura();

                var sw = System.Diagnostics.Stopwatch.StartNew();
                // Dar tiempo a ETW para capturar eventos de Presentación
                await Task.Delay(2000); 
                sw.Stop();
                
                _motorFps.DetenerCaptura();
                
                var (fps, ft, lows, zeroOneLows) = _motorFps.ObtenerResultados();

                if (fps > 0)
                {
                    resultado.FpsPromedio = fps;
                    resultado.FrametimePromedioMs = ft;
                    resultado.Fps1Porciento = lows;
                    resultado.Fps01Porciento = zeroOneLows; // Puede ser -1 si no hay muestras suficientes
                }
                else
                {
                    resultado.FpsPromedio = -1;
                    resultado.FrametimePromedioMs = -1;
                    resultado.Fps1Porciento = -1;
                    resultado.Fps01Porciento = -1;
                }
                
                resultado.TiempoCpuMs = sw.ElapsedMilliseconds;

                // Test DNS 
                var dnsTime = await MedirResolucionDnsAsync(pingHost);
                
                // Test de red intensivo para jitter y packet loss
                var (latencia, jitter, loss) = await MedirRedAvanzadaAsync(pingHost);
                resultado.LatenciaRedMs = latencia;
                resultado.JitterMs = jitter;
                resultado.PacketLossPorcentaje = loss;
                resultado.DnsLookupMs = dnsTime; // Se debe añadir a ResultadoPrueba.cs

                return resultado;
            });
        }

        public async Task<double> MedirResolucionDnsAsync(string dominio)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var hostEntry = await Dns.GetHostEntryAsync(dominio);
                sw.Stop();
                return sw.ElapsedMilliseconds;
            }
            catch
            {
                return -1;
            }
        }

        public async Task<double> MedirLatenciaRedAsync(string host = "8.8.8.8")
        {
            var (latencia, _, _) = await MedirRedAvanzadaAsync(host, 4);
            return latencia;
        }
        
        public async Task<(double Latencia, double Jitter, double PacketLoss)> MedirRedAvanzadaAsync(string host = "8.8.8.8", int intentos = 10)
        {
            try
            {
                using var ping = new Ping();
                List<long> pings = new();
                int fallos = 0;

                for (int i = 0; i < intentos; i++)
                {
                    var reply = await ping.SendPingAsync(host, 1000);
                    if (reply.Status == IPStatus.Success)
                    {
                        pings.Add(reply.RoundtripTime);
                    }
                    else
                    {
                        fallos++;
                    }
                }

                if (pings.Count == 0) return (-1, 0, 100);

                double promedio = pings.Average();
                double jitter = 0;
                if (pings.Count > 1)
                {
                    double variance = pings.Select(p => Math.Pow(p - promedio, 2)).Average();
                    jitter = Math.Sqrt(variance);
                }
                
                double loss = ((double)fallos / intentos) * 100.0;

                return (promedio, jitter, loss);
            }
            catch
            {
                return (-1, 0, 100);
            }
        }

    }
}
