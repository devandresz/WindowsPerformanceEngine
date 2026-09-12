using System;
using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Interfaces;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;

namespace WindowsPerformanceEngine.Motor.Reglas
{
    public class OptimizacionGameMode : IAplicadorRegla
    {
        private readonly IRegistroWindows _registro;
        private const string RUTA = @"SOFTWARE\Microsoft\GameBar";
        private const string VALOR = "AutoGameModeEnabled";

        public OptimizacionGameMode(IRegistroWindows registro)
        {
            _registro = registro;
        }

        public string IdReglaSoportada => "WIN-GAMEMODE-001";

        public async Task<bool> EsAplicableAsync()
        {
            var valor = await _registro.LeerValorAsync(RUTA, VALOR);
            // Aplicable si no está habilitado
            return valor == null || (int)valor == 0;
        }

        public async Task<Transaccion> AplicarAsync(bool simulacion)
        {
            var tx = new Transaccion 
            { 
                IdRegla = IdReglaSoportada, 
                Simulacion = simulacion,
                AccionRealizada = "Activar Windows Game Mode"
            };

            var valorActual = await _registro.LeerValorAsync(RUTA, VALOR);
            tx.ValorAnterior = valorActual?.ToString() ?? "null";
            tx.ValorNuevo = "1";

            await _registro.EscribirValorAsync(RUTA, VALOR, 1, simulacion);
            return tx;
        }

        public async Task<bool> RevertirAsync(Transaccion transaccion)
        {
            if (transaccion.IdRegla != IdReglaSoportada) return false;
            
            int valorPrevio = transaccion.ValorAnterior == "null" ? 0 : int.Parse(transaccion.ValorAnterior);
            return await _registro.EscribirValorAsync(RUTA, VALOR, valorPrevio, false);
        }
    }
}
