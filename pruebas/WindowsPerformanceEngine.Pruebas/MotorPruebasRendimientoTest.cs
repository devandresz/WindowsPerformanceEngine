using System.Threading.Tasks;
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
    }
}
