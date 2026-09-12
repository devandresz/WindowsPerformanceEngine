using System.Threading.Tasks;
using Xunit;
using WindowsPerformanceEngine.Motor.Servicios;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;
using System.Collections.Generic;

namespace WindowsPerformanceEngine.Pruebas
{
    public class MotorDecisionPruebas
    {
        [Fact]
        public void EvaluarRegla_EvidenciaInsuficiente_DeberiaRechazar()
        {
            var motor = new MotorDecision();
            var hwInfo = new InformacionHardware();
            
            var regla = new ReglaOptimizacion 
            {
                Evidencia = new Evidencia 
                { 
                    Nivel = NivelEvidencia.Insuficiente,
                    Confianza = 0
                }
            };

            var decision = motor.EvaluarRegla(regla, hwInfo);

            Assert.Equal(DecisionOptimizacion.NoHayEvidenciaSuficiente, decision);
        }

        [Fact]
        public void EvaluarRegla_RiesgoExperimental_DeberiaRechazarSiConfianzaBaja()
        {
            var motor = new MotorDecision();
            var hwInfo = new InformacionHardware();
            
            var regla = new ReglaOptimizacion 
            {
                Riesgo = NivelRiesgo.Experimental,
                Evidencia = new Evidencia 
                { 
                    Nivel = NivelEvidencia.Comunitaria,
                    Confianza = 50 // Baja confianza para experimental
                }
            };

            var decision = motor.EvaluarRegla(regla, hwInfo);

            Assert.Equal(DecisionOptimizacion.RiesgoDemasiadoAlto, decision);
        }

        [Fact]
        public void EvaluarRegla_ExclusionHardware_DeberiaRechazar()
        {
            var motor = new MotorDecision();
            var hwInfo = new InformacionHardware
            {
                Discos = new List<DiscoInfo> { new DiscoInfo { TipoMedio = "SSD" } }
            };
            
            var regla = new ReglaOptimizacion 
            {
                Riesgo = NivelRiesgo.Safe,
                Evidencia = new Evidencia { Nivel = NivelEvidencia.Oficial, Confianza = 100 },
                CondicionesExclusion = new List<string> { "SSD" }
            };

            var decision = motor.EvaluarRegla(regla, hwInfo);

            Assert.Equal(DecisionOptimizacion.NoCompatible, decision);
        }

        [Fact]
        public void EvaluarRegla_RiesgoExtreme_DeberiaPedirConfirmacion()
        {
            var motor = new MotorDecision();
            var hwInfo = new InformacionHardware();
            
            var regla = new ReglaOptimizacion 
            {
                Riesgo = NivelRiesgo.Extreme,
                Evidencia = new Evidencia 
                { 
                    Nivel = NivelEvidencia.Oficial,
                    Confianza = 100 // No importa cuán alta sea la confianza
                }
            };

            var decision = motor.EvaluarRegla(regla, hwInfo);

            Assert.Equal(DecisionOptimizacion.RequiereConfirmacionManual, decision);
        }
    }
}
