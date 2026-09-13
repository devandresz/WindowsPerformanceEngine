using System;
using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;
using WindowsPerformanceEngine.Motor.Dominio.Interfaces;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class MotorRegresion
    {
        private readonly IGestorTransacciones _gestorTransacciones;

        public MotorRegresion(IGestorTransacciones gestorTransacciones)
        {
            _gestorTransacciones = gestorTransacciones;
        }

        public class InformeRegresion
        {
            public bool SeDetectoRegresion { get; set; }
            public string Detalle { get; set; } = string.Empty;
            public bool ReversionExitosa { get; set; }
        }

        public async Task<InformeRegresion> EvaluarYRevertirSiEsNecesarioAsync(ResultadoPrueba antes, ResultadoPrueba despues, Transaccion transaccionActiva)
        {
            var informe = new InformeRegresion { SeDetectoRegresion = false };

            if (antes.FpsPromedio == -1 || despues.FpsPromedio == -1)
            {
                informe.Detalle = "Sin datos gráficos para evaluar regresión. Optimización conservada.";
                return informe;
            }

            // Margen de error tolerado: -3% (Cualquier caída peor que 3% = Regresión)
            double umbralFps = antes.FpsPromedio * 0.97; 
            double umbralLows = antes.Fps1Porciento * 0.95; // 5% de tolerancia para lows (son más volátiles)
            double umbralFrametime = antes.FrametimePromedioMs * 1.05; // 5% peor en MS
            
            bool regresionPunta = despues.FpsPromedio < umbralFps;
            bool regresionLows = despues.Fps1Porciento < umbralLows;
            bool regresionFrametime = despues.FrametimePromedioMs > umbralFrametime;

            bool esGaming = despues.PidObjetivo > 0;
            string etiquetaContexto = esGaming ? $"Gaming Real ({despues.ProcesoObjetivo} - PID {despues.PidObjetivo})" : "Estabilidad DWM";
            string etiquetaMetrica = esGaming ? "Juego" : "DWM";

            if (regresionPunta || regresionLows || regresionFrametime)
            {
                informe.SeDetectoRegresion = true;
                informe.Detalle = $"REGRESIÓN DETECTADA EN {etiquetaContexto} (Fuera del Margen Estadístico):\n" +
                                  $"FPS Promedio ({etiquetaMetrica}): {antes.FpsPromedio:F1} -> {despues.FpsPromedio:F1}\n" +
                                  $"1% Lows ({etiquetaMetrica}): {antes.Fps1Porciento:F1} -> {despues.Fps1Porciento:F1}\n" +
                                  $"Frametime: {antes.FrametimePromedioMs:F2}ms -> {despues.FrametimePromedioMs:F2}ms\n" +
                                  $"Iniciando Auto-Rollback...";

                // Auto-Rollback Transaccional
                informe.ReversionExitosa = await _gestorTransacciones.RevertirEspecificaAsync(transaccionActiva.IdTransaccion);
            }
            else
            {
                informe.Detalle = $"{etiquetaContexto} validada estadísticamente. Rendimiento estable o mejorado.\n" +
                                  $"1% Lows ({etiquetaMetrica}): {antes.Fps1Porciento:F1} -> {despues.Fps1Porciento:F1}";
            }

            return informe;
        }
    }
}
