using System;
using System.Collections.Generic;

namespace WindowsPerformanceEngine.Motor.Dominio.Modelos
{
    public enum NivelRiesgo { Safe, Advanced, Experimental, Extreme }
    
    public enum NivelEvidencia 
    { 
        Oficial, 
        Tecnica, 
        Academica, 
        Benchmark, 
        Experimental, 
        Comunitaria, 
        Insuficiente 
    }
    
    public enum DecisionOptimizacion
    {
        Optimizar,
        NoOptimizar,
        NoHayEvidenciaSuficiente,
        YaEstaOptimo,
        NoCompatible,
        NoDeterminado,
        RiesgoDemasiadoAlto,
        RequiereConfirmacionManual
    }

    public class FuenteEvidencia
    {
        public string Url { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
    }

    public class Evidencia
    {
        public string Explicacion { get; set; } = string.Empty;
        public List<FuenteEvidencia> Fuentes { get; set; } = new();
        public NivelEvidencia Nivel { get; set; } = NivelEvidencia.Insuficiente;
        public int Confianza { get; set; } // 0 a 100
        public DateTime FechaRevision { get; set; }
    }

    public class AccionRegistro
    {
        public string RutaRegistro { get; set; } = string.Empty;
        public string NombreValor { get; set; } = string.Empty;
        public string ValorDeseado { get; set; } = string.Empty;
        public string TipoValor { get; set; } = "DWord"; 
    }

    public class ReglaOptimizacion
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        
        public Evidencia Evidencia { get; set; } = new();
        public NivelRiesgo Riesgo { get; set; }
        
        public string ImpactoEsperado { get; set; } = string.Empty;
        public List<string> VersionesWindowsSoportadas { get; set; } = new();
        public List<string> CondicionesHardware { get; set; } = new();
        public List<string> CondicionesExclusion { get; set; } = new();
        
        public bool RequiereReinicio { get; set; }
        
        // Acciones declarativas para el Engine (sin hardcodeo)
        public List<AccionRegistro> Acciones { get; set; } = new();
    }
}
