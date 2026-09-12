using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WindowsPerformanceEngine.Motor.IoC;

namespace WindowsPerformanceEngine
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            
            // Inyectar capa del motor
            services.AgregarMotor();
            
            // Inyectar UI
            services.AddSingleton<MainWindow>();
            
            _serviceProvider = services.BuildServiceProvider();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }
    }
}
