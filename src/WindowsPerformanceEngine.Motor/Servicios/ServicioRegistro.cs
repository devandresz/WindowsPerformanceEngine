using System;
using System.Threading.Tasks;
using Microsoft.Win32;
using WindowsPerformanceEngine.Motor.Dominio.Interfaces;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class ServicioRegistro : IRegistroWindows
    {
        private RegistryKey? ObtenerColmena(string rutaCompleta, out string subRuta)
        {
            if (rutaCompleta.StartsWith("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase) || 
                rutaCompleta.StartsWith("HKCU", StringComparison.OrdinalIgnoreCase))
            {
                subRuta = rutaCompleta.Replace("HKEY_CURRENT_USER\\", "", StringComparison.OrdinalIgnoreCase)
                                      .Replace("HKCU\\", "", StringComparison.OrdinalIgnoreCase);
                return Registry.CurrentUser;
            }
            else if (rutaCompleta.StartsWith("HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase) || 
                     rutaCompleta.StartsWith("HKLM", StringComparison.OrdinalIgnoreCase))
            {
                subRuta = rutaCompleta.Replace("HKEY_LOCAL_MACHINE\\", "", StringComparison.OrdinalIgnoreCase)
                                      .Replace("HKLM\\", "", StringComparison.OrdinalIgnoreCase);
                return Registry.LocalMachine;
            }
            
            // Fallback por defecto si no se especifica
            subRuta = rutaCompleta;
            return Registry.LocalMachine; 
        }

        public Task<object?> LeerValorAsync(string rutaClave, string nombreValor)
        {
            try
            {
                var colmena = ObtenerColmena(rutaClave, out string subRuta);
                using var clave = colmena?.OpenSubKey(subRuta);
                return Task.FromResult(clave?.GetValue(nombreValor));
            }
            catch
            {
                return Task.FromResult<object?>(null);
            }
        }

        public Task<bool> EscribirValorAsync(string rutaClave, string nombreValor, object valor, bool simulacion)
        {
            if (simulacion)
            {
                // En modo simulación no alteramos el sistema
                return Task.FromResult(true);
            }

            try
            {
                var colmena = ObtenerColmena(rutaClave, out string subRuta);
                using var clave = colmena?.CreateSubKey(subRuta);
                clave?.SetValue(nombreValor, valor);
                return Task.FromResult(true);
            }
            catch
            {
                // Fallback / Log
                return Task.FromResult(false);
            }
        }

        public Task<bool> EliminarValorAsync(string rutaClave, string nombreValor)
        {
            try
            {
                var colmena = ObtenerColmena(rutaClave, out string subRuta);
                using var clave = colmena?.OpenSubKey(subRuta, true);
                if (clave != null)
                {
                    clave.DeleteValue(nombreValor, false);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
        public Task<bool> VerificarClaveAsync(string rutaClave)
        {
            try
            {
                var colmena = ObtenerColmena(rutaClave, out string subRuta);
                using var clave = colmena?.OpenSubKey(subRuta, false);
                return Task.FromResult(clave != null);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> EliminarClaveSiVaciaAsync(string rutaClave)
        {
            try
            {
                var colmena = ObtenerColmena(rutaClave, out string subRuta);
                using var clave = colmena?.OpenSubKey(subRuta, false);
                
                if (clave != null)
                {
                    if (clave.SubKeyCount == 0 && clave.ValueCount == 0)
                    {
                        clave.Dispose(); // Cerramos antes de borrar
                        colmena?.DeleteSubKey(subRuta, false);
                        return Task.FromResult(true);
                    }
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}
