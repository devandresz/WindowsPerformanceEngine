using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using Xunit;
using WindowsPerformanceEngine.Motor.Servicios;

namespace WindowsPerformanceEngine.Pruebas
{
    public class MotorPruebasRendimientoTest
    {
        [Fact]
        public async Task EjecutarPruebaRapidaAsync_DeberiaDevolverResultadosSinteticosValidos()
        {
            var diagnosticador = new MotorDiagnostico();
            var motorFps = new MotorFpsEtw();
            var motor = new MotorPruebasRendimiento(diagnosticador, motorFps);
            
            var resultado = await motor.EjecutarPruebaRapidaAsync();
            
            Assert.NotNull(resultado);
            // Al quitar el fallback sintético, sin una aplicación 3D corriendo, ETW devolverá -1
            Assert.True(resultado.FpsPromedio == -1 || resultado.FpsPromedio > 0);
            Assert.True(resultado.Fps1Porciento == -1 || resultado.Fps1Porciento > 0);
            Assert.True(resultado.FrametimePromedioMs == -1 || resultado.FrametimePromedioMs > 0);
            
            // Latencia de red puede ser -1 si no hay internet (ICMP bloqueado), pero nunca nula
            Assert.True(resultado.LatenciaRedMs >= -1);
        }

        [Fact]
        public void MotorFpsEtw_CalculoLows_NoUsaFormulaFalsa_Y_RespetaMuestrasInsuficientes()
        {
            var motorFps = new MotorFpsEtw();
            
            // Usar reflexión para inyectar muestras falsas en _frameTimesMs
            var field = typeof(MotorFpsEtw).GetField("_frameTimesMs", BindingFlags.NonPublic | BindingFlags.Instance);
            var frameTimes = (List<double>)field.GetValue(motorFps);
            
            // Inyectar 100 muestras (insuficiente para 0.1% Lows, que requiere >= 1000)
            // 99 muestras de 16.6ms (60 FPS) y 1 muestra de 33.3ms (30 FPS)
            for (int i = 0; i < 99; i++) frameTimes.Add(16.6);
            frameTimes.Add(33.3);
            
            var resultados = motorFps.ObtenerResultados();
            
            // Muestras = 100
            Assert.Equal(100, resultados.MuestrasValidas);
            
            // Promedio debe estar muy cerca de 60 FPS
            Assert.True(resultados.FpsPromedio > 59 && resultados.FpsPromedio < 61);
            
            // 1% de 100 es 1 muestra. La peor muestra es 33.3ms, que son 30 FPS.
            // Si usara una fórmula falsa como avg * 0.8, daría ~48 FPS.
            // Al ser cálculo real, debe dar ~30 FPS.
            Assert.True(resultados.Fps1Porciento > 29 && resultados.Fps1Porciento < 31);
            
            // 0.1% requiere 1000 muestras, como solo hay 100, debe devolver -1
            Assert.Equal(-1, resultados.Fps01Porciento);
        }

        [Fact]
        public void MotorFpsEtw_01Porciento_DevuelveValorConSuficientesMuestras()
        {
            var motorFps = new MotorFpsEtw();
            var field = typeof(MotorFpsEtw).GetField("_frameTimesMs", BindingFlags.NonPublic | BindingFlags.Instance);
            var frameTimes = (List<double>)field.GetValue(motorFps);
            
            // Inyectar 1000 muestras
            for (int i = 0; i < 999; i++) frameTimes.Add(16.6); // 60 FPS
            frameTimes.Add(50.0); // Peor muestra = 20 FPS
            
            var resultados = motorFps.ObtenerResultados();
            
            Assert.Equal(1000, resultados.MuestrasValidas);
            // 0.1% de 1000 = 1 muestra (la de 50ms) -> 20 FPS
            Assert.True(resultados.Fps01Porciento > 19 && resultados.Fps01Porciento < 21);
        }
    }
}
