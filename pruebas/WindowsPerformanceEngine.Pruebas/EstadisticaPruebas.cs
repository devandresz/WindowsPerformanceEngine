using System.Threading.Tasks;
using Xunit;
using WindowsPerformanceEngine.Motor.Servicios;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;

namespace WindowsPerformanceEngine.Pruebas
{
    public class EstadisticaPruebas
    {
        [Fact]
        public void EsMejoraSignificativa_RechazaMejoraFicticia()
        {
            var baseline = new ResultadoPrueba { FpsPromedio = 100 };
            var post = new ResultadoPrueba { FpsPromedio = 101 }; // Mejora de 1% (dentro del ruido de +/- 3%)
            
            Assert.False(post.EsMejoraSignificativa(baseline));
        }

        [Fact]
        public void EsMejoraSignificativa_AceptaMejoraReal()
        {
            var baseline = new ResultadoPrueba { FpsPromedio = 100 };
            var post = new ResultadoPrueba { FpsPromedio = 105 }; // Mejora de 5% (supera ruido 3%)
            
            Assert.True(post.EsMejoraSignificativa(baseline));
        }
    }
}
