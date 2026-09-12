using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;
using WindowsPerformanceEngine.Motor.Dominio.Interfaces;
using System.Linq;
using System;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class MotorOptimizacion
    {
        private readonly IGestorTransacciones _gestorTransacciones;
        private readonly IRegistroWindows _registro;

        public MotorOptimizacion(IGestorTransacciones gestorTransacciones, IRegistroWindows registro)
        {
            _gestorTransacciones = gestorTransacciones;
            _registro = registro;
        }

        public async Task<(bool Exito, string Mensaje, string DetallesTecnicos)> AplicarReglaDinamicaAsync(ReglaOptimizacion regla, bool simulacion)
        {
            if (!regla.Acciones.Any())
            {
                return (false, "La regla no tiene acciones definidas o es de confianza 0.", "");
            }

            var tx = await _gestorTransacciones.RegistrarInicioAsync(regla.Id, simulacion);
            string detallesTecnicos = "";
            string valoresNuevosStr = "";

            try
            {
                foreach (var accion in regla.Acciones)
                {
                    var claveExistia = await _registro.VerificarClaveAsync(accion.RutaRegistro);
                    var valorAnteriorObj = await _registro.LeerValorAsync(accion.RutaRegistro, accion.NombreValor);
                    
                    tx.AccionesReversa.Add(new Transaccion.AccionReversa
                    {
                        RutaRegistro = accion.RutaRegistro,
                        NombreValor = accion.NombreValor,
                        ValorAnterior = valorAnteriorObj,
                        ExistiaPreviamente = valorAnteriorObj != null,
                        ClaveExistiaPreviamente = claveExistia
                    });

                    detallesTecnicos += $"{accion.NombreValor} ({valorAnteriorObj ?? "null"} -> {accion.ValorDeseado}) ";
                    valoresNuevosStr += $"{accion.NombreValor}={accion.ValorDeseado} ";

                    if (!simulacion)
                    {
                        await _registro.EscribirValorAsync(accion.RutaRegistro, accion.NombreValor, accion.ValorDeseado, simulacion: false);
                    }
                }

                tx.ValorNuevo = valoresNuevosStr.Trim();
                await _gestorTransacciones.ConfirmarAsync(tx);
                
                string modo = simulacion ? "[DRY-RUN]" : "[APLICADO]";
                return (true, $"{modo} La regla '{regla.Nombre}' ha sido aplicada con éxito.", detallesTecnicos.Trim());
            }
            catch (Exception ex)
            {
                await _gestorTransacciones.RevertirEspecificaAsync(tx.IdTransaccion);
                return (false, $"Error aplicando la regla: {ex.Message}", detallesTecnicos.Trim());
            }
        }
        
        public async Task<bool> YaEstaOptimoAsync(ReglaOptimizacion regla)
        {
            foreach (var accion in regla.Acciones)
            {
                var obj = await _registro.LeerValorAsync(accion.RutaRegistro, accion.NombreValor);
                string valorActual = obj?.ToString() ?? string.Empty;
                
                if (valorActual != accion.ValorDeseado)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
