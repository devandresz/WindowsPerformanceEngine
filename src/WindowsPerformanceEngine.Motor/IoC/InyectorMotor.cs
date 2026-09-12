using Microsoft.Extensions.DependencyInjection;
using WindowsPerformanceEngine.Motor.Dominio.Interfaces;
using WindowsPerformanceEngine.Motor.Servicios;
using WindowsPerformanceEngine.Motor.Reglas;

namespace WindowsPerformanceEngine.Motor.IoC
{
    public static class InyectorMotor
    {
        public static IServiceCollection AgregarMotor(this IServiceCollection services)
        {
            // Servicios Core
            services.AddSingleton<IGestorTransacciones, GestorTransacciones>();
            services.AddSingleton<IServicioHardware, ServicioHardware>();
            services.AddSingleton<IRegistroWindows, ServicioRegistro>();
            services.AddSingleton<IDiagnosticador, MotorDiagnostico>();
            services.AddSingleton<IMotorPruebasRendimiento, MotorPruebasRendimiento>();
            services.AddSingleton<IBaseDatosOptimizacion, LectorBaseDatosOptimizacion>();
            
            services.AddTransient<MotorDecision>();
            services.AddTransient<MotorOptimizacion>();
            services.AddTransient<MotorPerfiles>();
            services.AddTransient<MotorMonitor>();
            services.AddTransient<MotorRendimientoGpu>();
            services.AddTransient<MotorFpsEtw>();
            services.AddTransient<MotorProcesos>();
            services.AddTransient<MotorEnergiaTermico>();
            services.AddTransient<MotorRegresion>();
            services.AddTransient<ServicioReporteAuditoria>();

            // Registro Automático de Reglas (Normalmente usaríamos reflexión o Assembly Scanning)
            services.AddTransient<IAplicadorRegla, OptimizacionGameMode>();

            return services;
        }
    }
}
