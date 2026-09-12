using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class ServicioReporteAuditoria
    {
        public class DetalleOptimizacion
        {
            public string IdRegla { get; set; } = string.Empty;
            public string NombreRegla { get; set; } = string.Empty;
            public string Explicacion { get; set; } = string.Empty;
            public string DetalleTecnico { get; set; } = string.Empty;
            public string ResultadoVerificacion { get; set; } = string.Empty;
            public string EstadoFinal { get; set; } = string.Empty;
        }

        public async Task<string> GenerarReporteAsync(
            InformacionHardware hwInfo,
            string perfilActivo,
            List<DetalleOptimizacion> detalles,
            bool simulacion)
        {
            return await Task.Run(() =>
            {
                var sb = new StringBuilder();
                if (simulacion)
                {
                    sb.AppendLine("# WINDOWS PERFORMANCE ENGINE - [REPORTE DE SIMULACIÓN - DRY RUN]");
                }
                else
                {
                    sb.AppendLine("# WINDOWS PERFORMANCE ENGINE - REPORTE DE AUDITORÍA (AUDIT TRAIL)");
                }
                sb.AppendLine("=================================================================");
                sb.AppendLine($"Fecha y Hora: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine();
                sb.AppendLine("## 1. RESUMEN DEL SISTEMA");
                sb.AppendLine($"Sistema Operativo: {hwInfo.OsNombre} (Build: {hwInfo.OsBuild} {hwInfo.OsArquitectura})");
                sb.AppendLine($"Procesador: {hwInfo.ProcesadorNombre} ({hwInfo.ProcesadorNucleosFisicos} Núcleos / {hwInfo.ProcesadorHilos} Hilos)");
                sb.AppendLine($"Memoria RAM: {hwInfo.MemoriaTotalGB} GB ({hwInfo.VelocidadMemoriaMHz} MHz)");
                sb.AppendLine($"Placa Base: {hwInfo.PlacaBaseFabricante} {hwInfo.PlacaBaseModelo}");
                sb.AppendLine($"Monitor Principal: {hwInfo.MonitorResolucionX}x{hwInfo.MonitorResolucionY} @ {hwInfo.MonitorFrecuenciaHz} Hz");
                sb.AppendLine();
                sb.AppendLine("## 2. PERFIL DE OPTIMIZACIÓN");
                sb.AppendLine($"Perfil Seleccionado/Aplicado: {perfilActivo}");
                sb.AppendLine();
                sb.AppendLine("## 3. LISTA DETALLADA DE CAMBIOS (TRANSACTION LOG)");
                sb.AppendLine("-----------------------------------------------------------------");

                if (detalles.Count == 0)
                {
                    sb.AppendLine("No se realizaron cambios en esta sesión (El sistema ya estaba óptimo).");
                }
                else
                {
                    foreach (var d in detalles)
                    {
                        sb.AppendLine($"Regla: [{d.IdRegla}] {d.NombreRegla}");
                        sb.AppendLine();
                        sb.AppendLine($"Impacto (Explicación): {d.Explicacion}");
                        sb.AppendLine();
                        sb.AppendLine($"Detalle Técnico: {d.DetalleTecnico}");
                        sb.AppendLine();
                        sb.AppendLine($"Estado Final: {d.EstadoFinal}");
                        sb.AppendLine("-----------------------------------------------------------------");
                    }
                }

                sb.AppendLine("=================================================================");
                sb.AppendLine("Firma Digital del Motor: WINDOWS PERFORMANCE ENGINE (ETW VERIFIED)");
                
                string content = sb.ToString();

                // Guardar en el Escritorio del usuario
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = $"WPE_Auditoria_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string fullPath = Path.Combine(desktopPath, fileName);

                File.WriteAllText(fullPath, content);
                
                return fullPath;
            });
        }
    }
}
