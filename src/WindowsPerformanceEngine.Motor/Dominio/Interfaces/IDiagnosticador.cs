using System.Threading.Tasks;

namespace WindowsPerformanceEngine.Motor.Dominio.Interfaces
{
    public class ResultadosDiagnostico
    {
        public float PorcentajeUsoCpu { get; set; }
        public float MemoriaDisponibleMB { get; set; }
        public bool EsLimitadoPorCpu { get; set; }
        public bool EsLimitadoPorRam { get; set; }
        public string ConclusionDiagnostico { get; set; } = string.Empty;
    }

    public interface IDiagnosticador
    {
        Task<ResultadosDiagnostico> EjecutarDiagnosticoActivoAsync();
    }
}
