namespace WindowsPerformanceEngine.Motor.Dominio.Modelos
{
    public class ResultadoPrueba
    {
        public double FpsPromedio { get; set; }
        public double FpsMinimo { get; set; }
        public double Fps1Porciento { get; set; }
        public double Fps01Porciento { get; set; }
        public double FrametimePromedioMs { get; set; }
        
        // Uso y Tiempos
        public double TiempoCpuMs { get; set; }
        public double TiempoGpuMs { get; set; }
        public double UtilizacionCpuPorcentaje { get; set; }
        public double UtilizacionGpuPorcentaje { get; set; }
        public double TemperaturaCpuC { get; set; }
        
        // Memoria
        public double RamUsadaMB { get; set; }
        public double VRamUsadaMB { get; set; }
        
        // Red
        public double LatenciaRedMs { get; set; }
        public double JitterMs { get; set; }
        public double PacketLossPorcentaje { get; set; }
        public double ThroughputMbps { get; set; }
        public double DnsLookupMs { get; set; }
        
        // --- Nuevos campos para Benchmark Gaming ---
        public int PidObjetivo { get; set; } = 0;
        public string ProcesoObjetivo { get; set; } = string.Empty;
        public int MuestrasValidas { get; set; } = 0;
        public double DuracionSegundos { get; set; } = 0;
        
        // Análisis de Ruido
        public bool EsMejoraSignificativa(ResultadoPrueba baseline)
        {
            // Margen de error estadístico base = 3%
            double umbralSignificancia = baseline.FpsPromedio * 0.03;
            double deltaAbsoluto = this.FpsPromedio - baseline.FpsPromedio;
            
            return deltaAbsoluto > umbralSignificancia;
        }
    }
}
