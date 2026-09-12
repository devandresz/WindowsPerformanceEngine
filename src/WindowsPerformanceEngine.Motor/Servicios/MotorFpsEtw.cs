using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class MotorFpsEtw : IDisposable
    {
        private TraceEventSession? _session;
        private readonly List<double> _frameTimesMs = new();
        private long _lastPresentTime = 0;
        private bool _isListening = false;

        public void IniciarCaptura()
        {
            if (_isListening) return;
            
            _frameTimesMs.Clear();
            _lastPresentTime = 0;
            _isListening = true;

            Task.Run(() =>
            {
                try
                {
                    var sessionName = "WPE-Etw-Fps-Session-" + Guid.NewGuid().ToString();
                    using (_session = new TraceEventSession(sessionName))
                    {
                        // Enable Microsoft-Windows-DxgKrnl
                        _session.EnableProvider(new Guid("802ec45a-1e99-4b83-9920-87c98277ba9d"), Microsoft.Diagnostics.Tracing.TraceEventLevel.Informational, 0x1); // 0x1 for Present events if applicable, or wildcard
                        
                        _session.Source.Dynamic.All += delegate (Microsoft.Diagnostics.Tracing.TraceEvent data)
                        {
                            if (!_isListening) return;

                            // DXGKrnl Present/PresentHistory Event (EventID 2 or similar depending on OS version, commonly we filter by event name)
                            if (data.EventName.Contains("Present", StringComparison.OrdinalIgnoreCase))
                            {
                                long currentTime = data.TimeStamp.Ticks;
                                if (_lastPresentTime > 0)
                                {
                                    // 1 tick = 100 ns. ms = ticks / 10000.0
                                    double deltaMs = (currentTime - _lastPresentTime) / 10000.0;
                                    if (deltaMs > 0 && deltaMs < 1000) // Filter out noise or initial gaps
                                    {
                                        lock (_frameTimesMs)
                                        {
                                            _frameTimesMs.Add(deltaMs);
                                        }
                                    }
                                }
                                _lastPresentTime = currentTime;
                            }
                        };
                        _session.Source.Process();
                    }
                }
                catch
                {
                    // Catch ETW errors (e.g. requires Admin)
                }
            });
        }

        public void DetenerCaptura()
        {
            _isListening = false;
            _session?.Stop();
            _session?.Dispose();
            _session = null;
        }

        public (double FpsPromedio, double FrametimePromedio, double Fps1Porciento, double Fps01Porciento) ObtenerResultados()
        {
            lock (_frameTimesMs)
            {
                if (_frameTimesMs.Count < 2) return (-1, -1, -1, -1);

                double avgFrametime = _frameTimesMs.Average();
                double fpsPromedio = 1000.0 / avgFrametime;

                // Orden descendente (los frametimes más altos = los fps más bajos)
                var sorted = _frameTimesMs.OrderByDescending(x => x).ToList();
                
                // 1% Lows
                int onePercentCount = Math.Max(1, (int)(sorted.Count * 0.01));
                double avg1PercentHighestFrametimes = sorted.Take(onePercentCount).Average();
                double fps1Porciento = 1000.0 / avg1PercentHighestFrametimes;

                // 0.1% Lows (solo si existen al menos 1000 muestras, de lo contrario -1)
                double fps01Porciento = -1;
                if (sorted.Count >= 1000)
                {
                    int zeroOneCount = Math.Max(1, (int)(sorted.Count * 0.001));
                    double avg01PercentHighestFrametimes = sorted.Take(zeroOneCount).Average();
                    fps01Porciento = 1000.0 / avg01PercentHighestFrametimes;
                }

                return (fpsPromedio, avgFrametime, fps1Porciento, fps01Porciento);
            }
        }

        public void Dispose()
        {
            DetenerCaptura();
        }
    }
}
