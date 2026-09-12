using System;
using System.Collections.Generic;

namespace WindowsPerformanceEngine.Motor.Dominio.Modelos
{
    public class InformacionHardware 
    {
        // CPU
        public string ProcesadorNombre { get; set; } = "Desconocido";
        public string ProcesadorFabricante { get; set; } = "Desconocido";
        public uint ProcesadorNucleosFisicos { get; set; }
        public uint ProcesadorHilos { get; set; }
        public uint VelocidadBaseMHz { get; set; }
        public string Arquitectura { get; set; } = "Desconocida";
        
        // RAM
        public string MemoriaTotalGB { get; set; } = "0 GB";
        public uint VelocidadMemoriaMHz { get; set; }
        
        // GPU & Monitor
        public List<string> TarjetasGraficas { get; set; } = new();
        public string ControladorGrafico { get; set; } = "Desconocido";
        
        // Placa Base
        public string PlacaBaseFabricante { get; set; } = "Desconocido";
        public string PlacaBaseModelo { get; set; } = "Desconocido";

        // Monitor
        public int MonitorResolucionX { get; set; } = 1920;
        public int MonitorResolucionY { get; set; } = 1080;
        public int MonitorFrecuenciaHz { get; set; } = 60;
        
        // Almacenamiento
        public List<DiscoInfo> Discos { get; set; } = new();
        
        // OS
        public string OsNombre { get; set; } = "Windows";
        public string OsBuild { get; set; } = "Desconocido";
        public string OsArquitectura { get; set; } = "Desconocida";
        public bool EsPortatil { get; set; } = false;

        // Red
        public List<RedInfo> AdaptadoresRed { get; set; } = new();
    }

    public class DiscoInfo
    {
        public string Modelo { get; set; } = string.Empty;
        public string Interfaz { get; set; } = string.Empty; // IDE, SCSI, NVMe
        public string TipoMedio { get; set; } = string.Empty; // SSD, HDD
    }

    public class RedInfo
    {
        public string Nombre { get; set; } = string.Empty;
        public ulong Velocidad { get; set; }
    }
}
