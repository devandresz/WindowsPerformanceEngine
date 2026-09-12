using System.Collections.Generic;

namespace WindowsPerformanceEngine.Motor.Dominio.Modelos
{
    public class ViewModelOptimizacion
    {
        public string ReglaId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Decision { get; set; } = string.Empty;
        public bool MostrarBotonAutorizar { get; set; }
        public bool EstaAutorizada { get; set; }
        public ReglaOptimizacion ReglaOriginal { get; set; } = new();
    }
}
