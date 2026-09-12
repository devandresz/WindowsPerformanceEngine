using System.Threading.Tasks;
using Xunit;
using WindowsPerformanceEngine.Motor.Servicios;
using WindowsPerformanceEngine.Motor.Reglas;

namespace WindowsPerformanceEngine.Pruebas
{
    public class ReglasPruebas
    {
        [Fact]
        public async Task OptimizacionGameMode_SimulacionNoAfectaRegistro()
        {
            var registro = new ServicioRegistro(); // Real registry access but with simulation flag
            var regla = new OptimizacionGameMode(registro);
            
            // Simulamos (Dry Run)
            var transaccion = await regla.AplicarAsync(simulacion: true);
            
            Assert.True(transaccion.Simulacion);
            Assert.Equal("WIN-GAMEMODE-001", transaccion.IdRegla);
            Assert.Equal("1", transaccion.ValorNuevo);
        }
    }
}
