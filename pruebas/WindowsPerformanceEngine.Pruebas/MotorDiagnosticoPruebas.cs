using System.Threading.Tasks;
using Xunit;
using WindowsPerformanceEngine.Motor.Servicios;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;

namespace WindowsPerformanceEngine.Pruebas
{
    public class MotorDiagnosticoPruebas
    {
        [Fact]
        public async Task EjecutarDiagnosticoActivoAsync_DeberiaDevolverResultadosReales()
        {
            var diagnostico = new MotorDiagnostico();
            
            var resultado = await diagnostico.EjecutarDiagnosticoActivoAsync();
            
            Assert.NotNull(resultado);
            Assert.True(resultado.PorcentajeUsoCpu >= 0 && resultado.PorcentajeUsoCpu <= 100, "El uso de CPU debe ser un porcentaje válido.");
            Assert.True(resultado.MemoriaDisponibleMB > 0, "La RAM disponible no puede ser cero en un sistema operativo funcional.");
        }
    }
}
