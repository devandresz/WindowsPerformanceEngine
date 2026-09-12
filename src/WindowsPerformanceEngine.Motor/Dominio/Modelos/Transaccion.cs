using System;

namespace WindowsPerformanceEngine.Motor.Dominio.Modelos
{
    public class Transaccion
    {
        public string IdTransaccion { get; set; } = Guid.NewGuid().ToString("N");
        public string IdRegla { get; set; } = string.Empty;
        public DateTime FechaEjecucion { get; set; } = DateTime.Now;
        public string AccionRealizada { get; set; } = string.Empty;
        public string ValorAnterior { get; set; } = string.Empty;
        public string ValorNuevo { get; set; } = string.Empty;
        public bool Simulacion { get; set; }
        public bool FueRevertida { get; set; }
        
        public class AccionReversa
        {
            public string RutaRegistro { get; set; } = string.Empty;
            public string NombreValor { get; set; } = string.Empty;
            public object? ValorAnterior { get; set; }
            public bool ExistiaPreviamente { get; set; }
            public bool ClaveExistiaPreviamente { get; set; }
        }
        
        public System.Collections.Generic.List<AccionReversa> AccionesReversa { get; set; } = new();
    }
}
