using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Interfaces;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class LectorBaseDatosOptimizacion : IBaseDatosOptimizacion
    {
        private readonly string _rutaArchivo = Path.Combine(AppContext.BaseDirectory, "optimizaciones.json");

        public async Task<(List<ReglaOptimizacion> Reglas, bool CargadoDesdeDisco)> CargarReglasAsync()
        {
            if (!File.Exists(_rutaArchivo))
            {
                var reglasPorDefecto = ObtenerReglasPorDefecto();
                try 
                {
                    var options = new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                    };
                    string json = JsonSerializer.Serialize(reglasPorDefecto, options);
                    await File.WriteAllTextAsync(_rutaArchivo, json);
                }
                catch { /* Si falla la escritura por permisos, al menos devolvemos las reglas en memoria */ }
                
                return (reglasPorDefecto, false);
            }

            try
            {
                string jsonString = await File.ReadAllTextAsync(_rutaArchivo);
                var reglas = JsonSerializer.Deserialize<List<ReglaOptimizacion>>(jsonString, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                });
                
                return (reglas ?? new List<ReglaOptimizacion>(), true);
            }
            catch (Exception ex)
            {
                // No silenciar el error. Devolver una regla falsa con el error para que la UI lo muestre,
                // o relanzar la excepción. Según la instrucción, la UI o consola debe mostrar el Exception.Message.
                // Como MainWindow espera una lista, lanzaremos una excepción con el mensaje real.
                throw new Exception($"Error al cargar optimizaciones.json: {ex.Message}");
            }
        }

        public async Task<List<ReglaOptimizacion>> RestaurarReglasPorDefectoAsync()
        {
            var reglasPorDefecto = ObtenerReglasPorDefecto();
            try 
            {
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };
                string json = JsonSerializer.Serialize(reglasPorDefecto, options);
                await File.WriteAllTextAsync(_rutaArchivo, json);
            }
            catch { /* Si falla la escritura por permisos, al menos devolvemos las reglas en memoria */ }
            return reglasPorDefecto;
        }

        private List<ReglaOptimizacion> ObtenerReglasPorDefecto()
        {
            return new List<ReglaOptimizacion>
            {
                new ReglaOptimizacion
                {
                    Id = "WIN-GAMEMODE-001",
                    Nombre = "Forzar Game Mode Estricto",
                    Categoria = "CPU",
                    Descripcion = "Habilita el Game Mode a nivel de kernel, priorizando hilos de juegos sobre procesos en segundo plano.",
                    Riesgo = NivelRiesgo.Safe,
                    RequiereReinicio = false,
                    Evidencia = new Evidencia
                    {
                        Explicacion = "El Game Mode evita preempts en threads del juego, reduciendo el stuttering.",
                        Nivel = NivelEvidencia.Oficial,
                        Confianza = 95,
                        Fuentes = new List<FuenteEvidencia> { new FuenteEvidencia { Url = "https://docs.microsoft.com", Descripcion = "Documentación Oficial de Microsoft", Autor = "Microsoft" } }
                    },
                    Acciones = new List<AccionRegistro>
                    {
                        new AccionRegistro { RutaRegistro = "HKEY_CURRENT_USER\\Software\\Microsoft\\GameBar", NombreValor = "AutoGameModeEnabled", ValorDeseado = "1", TipoValor = "DWord" }
                    }
                },
                new ReglaOptimizacion
                {
                    Id = "WIN-MONITOR-001",
                    Nombre = "Desactivar Ahorro Energía PCIe (Inyección ASPM)",
                    Categoria = "GPU",
                    Riesgo = NivelRiesgo.Advanced,
                    RequiereReinicio = true,
                    Descripcion = "Inyección de registro ASPM PCIe. (Datos reales de refresco/VRR del monitor NO detectables por este motor).",
                    Evidencia = new Evidencia
                    {
                        Explicacion = "Previene estados de bajo consumo en el bus PCIe. Sin confirmación de monitor activo.",
                        Nivel = NivelEvidencia.Tecnica,
                        Confianza = 60,
                        Fuentes = new List<FuenteEvidencia> { new FuenteEvidencia { Url = "https://reddit.com/r/optimizations", Descripcion = "PCIe Latency Guide", Autor = "Comunidad" } }
                    },
                    Acciones = new List<AccionRegistro>
                    {
                        new AccionRegistro { RutaRegistro = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Power\\PowerSettings\\501a4d13-42af-4429-9fd1-a8218c268e20\\ee12f906-d277-404b-b6da-e5fa1a576df5", NombreValor = "Attributes", ValorDeseado = "0", TipoValor = "DWord" }
                    }
                },
                new ReglaOptimizacion
                {
                    Id = "WIN-TCPAUTOTUNING-001",
                    Nombre = "Optimizar TCP Auto-Tuning",
                    Categoria = "Red",
                    Riesgo = NivelRiesgo.Safe,
                    RequiereReinicio = false,
                    Descripcion = "Ajusta la ventana de recepción TCP para reducir jitter en conexiones de banda ancha.",
                    Evidencia = new Evidencia
                    {
                        Explicacion = "Windows restringe el buffer de red por defecto, afectando la estabilidad del ping.",
                        Nivel = NivelEvidencia.Oficial,
                        Confianza = 90,
                        Fuentes = new List<FuenteEvidencia> { new FuenteEvidencia { Url = "https://docs.microsoft.com", Descripcion = "TCP AutoTuningLevel", Autor = "Microsoft" } }
                    },
                    Acciones = new List<AccionRegistro>
                    {
                        new AccionRegistro { RutaRegistro = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters", NombreValor = "TcpWindowSize", ValorDeseado = "65535", TipoValor = "DWord" }
                    }
                },
                new ReglaOptimizacion
                {
                    Id = "WIN-USBPOWER-001",
                    Nombre = "Desactivar Suspensión Selectiva USB",
                    Categoria = "Dispositivos",
                    Riesgo = NivelRiesgo.Advanced,
                    RequiereReinicio = false,
                    Descripcion = "Previene que el SO apague puertos USB, eliminando micro-cortes en ratones de 1000Hz+.",
                    Evidencia = new Evidencia
                    {
                        Explicacion = "Suspender puertos interrumpe el polling rate del mouse, añadiendo ms de input lag.",
                        Nivel = NivelEvidencia.Tecnica,
                        Confianza = 88,
                        Fuentes = new List<FuenteEvidencia> { new FuenteEvidencia { Url = "https://blurbusters.com", Descripcion = "Blur Busters Mouse Polling Guide", Autor = "Blur Busters" } }
                    },
                    Acciones = new List<AccionRegistro>
                    {
                        new AccionRegistro { RutaRegistro = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\USB", NombreValor = "DisableSelectiveSuspend", ValorDeseado = "1", TipoValor = "DWord" }
                    }
                },
                new ReglaOptimizacion
                {
                    Id = "WIN-HAGS-001",
                    Nombre = "Hardware-Accelerated GPU Scheduling (HAGS)",
                    Categoria = "GPU",
                    Riesgo = NivelRiesgo.Safe,
                    RequiereReinicio = true,
                    Descripcion = "Delega la gestión de memoria de video al procesador de la GPU.",
                    Evidencia = new Evidencia
                    {
                        Explicacion = "Mejora el 1% lows en sistemas con cuello de botella de CPU al liberar la carga de scheduling.",
                        Nivel = NivelEvidencia.Oficial,
                        Confianza = 92,
                        Fuentes = new List<FuenteEvidencia> { new FuenteEvidencia { Url = "https://nvidia.com", Descripcion = "NVIDIA HAGS Performance Analysis", Autor = "NVIDIA" } }
                    },
                    Acciones = new List<AccionRegistro>
                    {
                        new AccionRegistro { RutaRegistro = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers", NombreValor = "HwSchMode", ValorDeseado = "2", TipoValor = "DWord" }
                    }
                },
                new ReglaOptimizacion
                {
                    Id = "WIN-INDEXING-001",
                    Nombre = "Desactivar Windows Search Indexer",
                    Categoria = "Sistema",
                    Riesgo = NivelRiesgo.Advanced,
                    RequiereReinicio = true,
                    Descripcion = "Desactiva el indexador de búsqueda. ADVERTENCIA: Solo aplicar si no usas la búsqueda local frecuente, ya que la romperá. Condición de beneficio no medible dinámicamente.",
                    Evidencia = new Evidencia
                    {
                        Explicacion = "El indexador causa I/O. Beneficio solo demostrable en discos HDD o SSD muy saturados. Rollback disponible.",
                        Nivel = NivelEvidencia.Comunitaria,
                        Confianza = 50,
                        Fuentes = new List<FuenteEvidencia> { new FuenteEvidencia { Url = "https://forums.guru3d.com", Descripcion = "Windows Services optimization", Autor = "Comunidad" } }
                    },
                    Acciones = new List<AccionRegistro>
                    {
                        new AccionRegistro { RutaRegistro = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\WSearch", NombreValor = "Start", ValorDeseado = "4", TipoValor = "DWord" }
                    }
                },
                new ReglaOptimizacion
                {
                    Id = "WIN-DEFENDER-001",
                    Nombre = "Desactivar Windows Defender (Experimental)",
                    Categoria = "Seguridad",
                    Riesgo = NivelRiesgo.Extreme,
                    RequiereReinicio = true,
                    Descripcion = "Deshabilita la protección en tiempo real de Microsoft Defender. ¡EXTREMO PELIGRO! REQUIERE CONFIRMACIÓN EXPLÍCITA. NO SE APLICA AUTOMÁTICAMENTE.",
                    Evidencia = new Evidencia
                    {
                        Explicacion = "MsMpEng.exe escanea archivos en tiempo real. Esta acción te dejará expuesto. Confirmar bajo tu propio riesgo. Rollback automático disponible.",
                        Nivel = NivelEvidencia.Tecnica,
                        Confianza = 70,
                        Fuentes = new List<FuenteEvidencia> { new FuenteEvidencia { Url = "https://security.microsoft.com", Descripcion = "Antimalware Scan Interface", Autor = "Microsoft" } }
                    },
                    Acciones = new List<AccionRegistro>
                    {
                        new AccionRegistro { RutaRegistro = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Defender", NombreValor = "DisableAntiSpyware", ValorDeseado = "1", TipoValor = "DWord" }
                    }
                },
                new ReglaOptimizacion
                {
                    Id = "WIN-MMCSS-001",
                    Nombre = "Optimización SystemResponsiveness",
                    Categoria = "Kernel",
                    Riesgo = NivelRiesgo.Safe,
                    RequiereReinicio = true,
                    Descripcion = "Reduce el % de CPU reservado para servicios de baja prioridad.",
                    Evidencia = new Evidencia
                    {
                        Explicacion = "Permite que el hilo del juego ocupe el 90-100% en lugar del 80% máximo por defecto.",
                        Nivel = NivelEvidencia.Oficial,
                        Confianza = 90,
                        Fuentes = new List<FuenteEvidencia> { new FuenteEvidencia { Url = "https://docs.microsoft.com", Descripcion = "MMCSS Docs", Autor = "Microsoft" } }
                    },
                    Acciones = new List<AccionRegistro>
                    {
                        new AccionRegistro { RutaRegistro = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile", NombreValor = "SystemResponsiveness", ValorDeseado = "10", TipoValor = "DWord" }
                    }
                }
            };
        }
    }
}
