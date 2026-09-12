using System.Threading.Tasks;
using Xunit;
using WindowsPerformanceEngine.Motor.Servicios;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;
using System.Collections.Generic;

namespace WindowsPerformanceEngine.Pruebas
{
    public class MotorOptimizacionPruebas : IAsyncLifetime
    {
        private const string TEST_KEY = "HKEY_CURRENT_USER\\Software\\WPE_TEST_TEMP";
        private ServicioRegistro _registro;

        public MotorOptimizacionPruebas()
        {
            _registro = new ServicioRegistro();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            // Cleanup post-test
            try
            {
                var colmena = Microsoft.Win32.Registry.CurrentUser;
                colmena.DeleteSubKeyTree("Software\\WPE_TEST_TEMP", false);
            }
            catch { }
            await Task.CompletedTask;
        }

        [Fact]
        public async Task AplicarRegla_Simulacion_NoEscribeRegistro()
        {
            var txManager = new GestorTransacciones(_registro);
            var motor = new MotorOptimizacion(txManager, _registro);
            
            var regla = new ReglaOptimizacion 
            {
                Id = "TEST-DRY",
                Acciones = new List<AccionRegistro> 
                { 
                    new AccionRegistro { RutaRegistro = TEST_KEY, NombreValor = "A", ValorDeseado = "1" } 
                }
            };

            var resultado = await motor.AplicarReglaDinamicaAsync(regla, simulacion: true);

            Assert.True(resultado.Exito);
            Assert.Contains("[DRY-RUN]", resultado.Mensaje);
            
            var valorLeido = await _registro.LeerValorAsync(TEST_KEY, "A");
            Assert.Null(valorLeido); // No debió escribir
        }

        [Fact]
        public async Task Rollback_ValorInexistente_LoElimina()
        {
            var txManager = new GestorTransacciones(_registro);
            var motor = new MotorOptimizacion(txManager, _registro);
            
            var regla = new ReglaOptimizacion 
            {
                Id = "TEST-ROLLBACK-NEW",
                Acciones = new List<AccionRegistro> 
                { 
                    new AccionRegistro { RutaRegistro = TEST_KEY, NombreValor = "NuevoValor", ValorDeseado = "42" } 
                }
            };

            // Aseguramos que no existe antes
            await _registro.EliminarValorAsync(TEST_KEY, "NuevoValor");

            var resultado = await motor.AplicarReglaDinamicaAsync(regla, simulacion: false);
            Assert.True(resultado.Exito);
            
            // Verificamos que se escribió
            var valorEscrito = await _registro.LeerValorAsync(TEST_KEY, "NuevoValor");
            Assert.Equal("42", valorEscrito?.ToString());

            // Revertir
            bool rollbackExito = await txManager.RevertirUltimaAsync();
            Assert.True(rollbackExito);

            // Verificamos que se ELIMINÓ (restaurando su ausencia)
            var valorDespuesRollback = await _registro.LeerValorAsync(TEST_KEY, "NuevoValor");
            Assert.Null(valorDespuesRollback);
        }

        [Fact]
        public async Task Rollback_ValorExistente_LoRestaura()
        {
            var txManager = new GestorTransacciones(_registro);
            var motor = new MotorOptimizacion(txManager, _registro);
            
            // Escribimos un valor inicial
            await _registro.EscribirValorAsync(TEST_KEY, "ValorExistente", "ValorOriginal", false);

            var regla = new ReglaOptimizacion 
            {
                Id = "TEST-ROLLBACK-EXISTING",
                Acciones = new List<AccionRegistro> 
                { 
                    new AccionRegistro { RutaRegistro = TEST_KEY, NombreValor = "ValorExistente", ValorDeseado = "ValorModificado" } 
                }
            };

            var resultado = await motor.AplicarReglaDinamicaAsync(regla, simulacion: false);
            Assert.True(resultado.Exito);

            // Revertir
            bool rollbackExito = await txManager.RevertirUltimaAsync();
            Assert.True(rollbackExito);

            // Verificamos que se restauró al original
            var valorDespuesRollback = await _registro.LeerValorAsync(TEST_KEY, "ValorExistente");
            Assert.Equal("ValorOriginal", valorDespuesRollback?.ToString());
        }

        [Fact]
        public async Task Rollback_ClaveInexistente_LaElimina()
        {
            var txManager = new GestorTransacciones(_registro);
            var motor = new MotorOptimizacion(txManager, _registro);
            
            string subKeyNew = TEST_KEY + "\\SubKeyNew";
            
            // Nos aseguramos de que no exista
            try { Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree("Software\\WPE_TEST_TEMP\\SubKeyNew", false); } catch { }

            var regla = new ReglaOptimizacion 
            {
                Id = "TEST-ROLLBACK-KEY",
                Acciones = new List<AccionRegistro> 
                { 
                    new AccionRegistro { RutaRegistro = subKeyNew, NombreValor = "A", ValorDeseado = "1" } 
                }
            };

            var resultado = await motor.AplicarReglaDinamicaAsync(regla, simulacion: false);
            Assert.True(resultado.Exito);

            // Verificamos que se creó
            var existeKey = await _registro.VerificarClaveAsync(subKeyNew);
            Assert.True(existeKey);

            // Revertir
            bool rollbackExito = await txManager.RevertirUltimaAsync();
            Assert.True(rollbackExito);

            // Verificamos que la KEY desapareció
            var existeKeyDespues = await _registro.VerificarClaveAsync(subKeyNew);
            Assert.False(existeKeyDespues);
        }
    }
}
