using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Interfaces;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class GestorTransacciones : IGestorTransacciones
    {
        private readonly List<Transaccion> _historial = new();
        private readonly IRegistroWindows _registro;

        public GestorTransacciones(IRegistroWindows registro)
        {
            _registro = registro;
        }

        public Task<Transaccion> RegistrarInicioAsync(string idRegla, bool simulacion)
        {
            var tx = new Transaccion 
            { 
                IdRegla = idRegla,
                Simulacion = simulacion 
            };
            return Task.FromResult(tx);
        }

        public Task ConfirmarAsync(Transaccion transaccion)
        {
            _historial.Add(transaccion);
            // Aqui persistir en JSON
            return Task.CompletedTask;
        }

        public async Task<bool> RevertirUltimaAsync()
        {
            if (_historial.Count == 0) return false;
            var ultima = _historial[_historial.Count - 1];
            return await RevertirEspecificaAsync(ultima.IdTransaccion);
        }

        public async Task<bool> RevertirEspecificaAsync(string idTransaccion)
        {
            var tx = _historial.Find(t => t.IdTransaccion == idTransaccion);
            if (tx == null || tx.FueRevertida || tx.Simulacion) return false;

            bool exitoTotal = true;
            foreach (var rev in tx.AccionesReversa)
            {
                if (rev.ExistiaPreviamente && rev.ValorAnterior != null)
                {
                    bool ok = await _registro.EscribirValorAsync(rev.RutaRegistro, rev.NombreValor, rev.ValorAnterior, false);
                    if (!ok) exitoTotal = false;
                }
                else if (!rev.ExistiaPreviamente)
                {
                    bool ok = await _registro.EliminarValorAsync(rev.RutaRegistro, rev.NombreValor);
                    if (!ok) exitoTotal = false;

                    if (!rev.ClaveExistiaPreviamente)
                    {
                        await _registro.EliminarClaveSiVaciaAsync(rev.RutaRegistro);
                    }
                }
            }

            bool verificacionOk = true;
            if (exitoTotal)
            {
                // Verificar post-rollback
                foreach (var rev in tx.AccionesReversa)
                {
                    var valorActualObj = await _registro.LeerValorAsync(rev.RutaRegistro, rev.NombreValor);
                    if (rev.ExistiaPreviamente)
                    {
                        if (valorActualObj == null || !valorActualObj.Equals(rev.ValorAnterior)) verificacionOk = false;
                    }
                    else
                    {
                        if (valorActualObj != null) verificacionOk = false;
                    }

                    if (!rev.ClaveExistiaPreviamente)
                    {
                        var claveAunExiste = await _registro.VerificarClaveAsync(rev.RutaRegistro);
                        if (claveAunExiste) verificacionOk = false;
                    }
                }
            }

            if (exitoTotal && verificacionOk) 
            {
                tx.FueRevertida = true;
                return true;
            }
            return false;
        }

        public Task<IReadOnlyList<Transaccion>> ObtenerHistorialAsync()
        {
            return Task.FromResult<IReadOnlyList<Transaccion>>(_historial.AsReadOnly());
        }
    }
}
