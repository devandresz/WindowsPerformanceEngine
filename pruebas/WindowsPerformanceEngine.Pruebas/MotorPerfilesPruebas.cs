using System.Collections.Generic;
using Xunit;
using WindowsPerformanceEngine.Motor.Servicios;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;
using WindowsPerformanceEngine.Motor.Dominio.Interfaces;

namespace WindowsPerformanceEngine.Pruebas
{
    public class MotorPerfilesPruebas
    {
        private readonly MotorPerfiles _motorPerfiles = new MotorPerfiles();

        [Fact]
        public void DetectarPerfilAutomatico_DeberiaDetectarLaptopPerformance()
        {
            var motor = new MotorPerfiles();
            var hw = new InformacionHardware 
            { 
                EsPortatil = true,
                Discos = new List<DiscoInfo> { new DiscoInfo { TipoMedio = "Laptop HDD" } },
                TarjetasGraficas = new List<string> { "NVIDIA GeForce RTX 4060 Laptop" }
            };
            var diag = new ResultadosDiagnostico();

            var resultado = motor.DetectarPerfilAutomatico(hw, diag);

            Assert.Equal(TipoPerfil.LaptopPerformance, resultado.PerfilSugerido);
        }

        [Fact]
        public void DetectarPerfilAutomatico_DeberiaDetectarGamingStreaming()
        {
            var hwInfo = new InformacionHardware
            {
                Discos = new List<DiscoInfo> { new DiscoInfo { TipoMedio = "SSD" } },
                ProcesadorNombre = "AMD Ryzen 9 5900X",
                ProcesadorNucleosFisicos = 12,
                MemoriaTotalGB = "32 GB",
                TarjetasGraficas = new List<string> { "NVIDIA GeForce RTX 3080" }
            };

            var resultado = _motorPerfiles.DetectarPerfilAutomatico(hwInfo, new ResultadosDiagnostico());

            Assert.Equal(TipoPerfil.GamingStreaming, resultado.PerfilSugerido);
        }
    }
}
