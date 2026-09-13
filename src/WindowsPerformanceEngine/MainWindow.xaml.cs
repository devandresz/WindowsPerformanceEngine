using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WindowsPerformanceEngine.Motor.Dominio.Interfaces;
using WindowsPerformanceEngine.Motor.Dominio.Modelos;
using WindowsPerformanceEngine.Motor.Servicios;

namespace WindowsPerformanceEngine
{
    public partial class MainWindow : Window
    {
        private readonly IServicioHardware _servicioHardware;
        private readonly IGestorTransacciones _gestorTransacciones;
        private readonly IDiagnosticador _diagnosticador;
        private readonly IMotorPruebasRendimiento _motorPruebas;
        private readonly IBaseDatosOptimizacion _baseDatos;
        private readonly MotorDecision _motorDecision;
        private readonly MotorOptimizacion _motorOptimizacion;
        private readonly MotorPerfiles _motorPerfiles;
        private readonly MotorRendimientoGpu _motorGpu;
        private readonly MotorProcesos _motorProcesos;
        private readonly MotorEnergiaTermico _motorTermico;
        private readonly MotorRegresion _motorRegresion;
        private readonly ServicioReporteAuditoria _servicioAuditoria;
        private List<ReglaOptimizacion> _reglasEnMemoria = new();
        private List<ReglaOptimizacion> _reglasAAplicar = new();
        private ResultadoPrueba? _baselinePrueba = null;
        private CancellationTokenSource? _benchmarkCts;
        private int _benchmarkTargetPid = 0;
        private string _benchmarkTargetName = string.Empty;

        public MainWindow(
            IServicioHardware servicioHardware, 
            IGestorTransacciones gestorTransacciones,
            IDiagnosticador diagnosticador,
            IMotorPruebasRendimiento motorPruebas,
            IBaseDatosOptimizacion baseDatos,
            MotorDecision motorDecision,
            MotorOptimizacion motorOptimizacion,
            MotorPerfiles motorPerfiles,
            MotorRendimientoGpu motorGpu,
            MotorProcesos motorProcesos,
            MotorEnergiaTermico motorTermico,
            MotorRegresion motorRegresion,
            ServicioReporteAuditoria servicioAuditoria)
        {
            InitializeComponent();
            _servicioHardware = servicioHardware;
            _gestorTransacciones = gestorTransacciones;
            _diagnosticador = diagnosticador;
            _motorPruebas = motorPruebas;
            _baseDatos = baseDatos;
            _motorDecision = motorDecision;
            _motorOptimizacion = motorOptimizacion;
            _motorPerfiles = motorPerfiles;
            _motorGpu = motorGpu;
            _motorProcesos = motorProcesos;
            _motorTermico = motorTermico;
            _motorRegresion = motorRegresion;
            _servicioAuditoria = servicioAuditoria;
            
            CargarHardwareAsync();
        }

        private async void CargarHardwareAsync()
        {
            var info = await _servicioHardware.ObtenerInformacionAsync();
            
            TxtProcesador.Text = $"{info.ProcesadorNombre}\n" +
                                 $"{info.ProcesadorNucleosFisicos} Núcleos / {info.ProcesadorHilos} Hilos\n" +
                                 $"{info.VelocidadBaseMHz} MHz - {info.Arquitectura}";
                                 
            TxtRam.Text = $"{info.MemoriaTotalGB} ({info.VelocidadMemoriaMHz} MHz)";
            
            TxtGpu.Text = info.TarjetasGraficas.Count > 0 
                ? string.Join("\n", info.TarjetasGraficas) 
                : "No detectada";

            TxtGpu.Text += $"\nMonitor Principal: {info.MonitorResolucionX}x{info.MonitorResolucionY} @ {info.MonitorFrecuenciaHz} Hz";
                
            TxtOs.Text = $"{info.OsNombre} (Build: {info.OsBuild} {info.OsArquitectura})";
            
            var estadoTermico = _motorTermico.AnalizarEstadoTermico();
            TxtOs.Text += $"\nPlan Energía: {estadoTermico.PlanEnergiaActivo}";
            if (estadoTermico.ExisteThermalThrottling)
            {
                TxtOs.Text += $"\n[!] THROTTLING DETECTADO ({estadoTermico.TemperaturaTermicaAcpi:F1}°C)";
                TxtOs.Foreground = System.Windows.Media.Brushes.Red;
            }

            var perfil = _motorPerfiles.DetectarPerfilAutomatico(info, new ResultadosDiagnostico());
            this.Title = $"Windows Performance Engine - Perfil Recomendado: {perfil.PerfilSugerido}";
        }

        private void ResaltarBotonActivo(object sender)
        {
            var botones = new[] { BtnDashboard, BtnProcesos, BtnOptimizaciones, BtnPruebas, BtnHistorial };
            foreach (var btn in botones)
            {
                if (btn != null)
                {
                    btn.Background = System.Windows.Media.Brushes.Transparent;
                    btn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#94A3B8"));
                    btn.FontWeight = FontWeights.Normal;
                }
            }

            if (sender is System.Windows.Controls.Button botonActivo)
            {
                botonActivo.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#24283B"));
                botonActivo.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#64FFDA"));
                botonActivo.FontWeight = FontWeights.SemiBold;
            }
        }

        private void AnimarTransicionVista()
        {
            if (ContenedorVistas == null) return;
            
            var fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            var slideUp = new DoubleAnimation
            {
                From = 20,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            ContenedorVistas.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            if (ContenedorVistas.RenderTransform is System.Windows.Media.TranslateTransform tt)
            {
                tt.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideUp);
            }
        }

        private void OcultarPaneles()
        {
            PnlDashboard.Visibility = Visibility.Collapsed;
            PnlOptimizacionesWrapper.Visibility = Visibility.Collapsed;
            PnlProcesosWrapper.Visibility = Visibility.Collapsed;
            PnlHistorialWrapper.Visibility = Visibility.Collapsed;
            PnlGraficosBenchmark.Visibility = Visibility.Collapsed;
            BtnAplicarOptimizaciones.Visibility = Visibility.Collapsed;
            PanelWhyEngine.Visibility = Visibility.Collapsed;
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            ResaltarBotonActivo(sender);
            OcultarPaneles();
            PnlDashboard.Visibility = Visibility.Visible;
            TxtConsola.Text = "Dashboard activo. Monitoreo de Hardware principal en curso.";
            AnimarTransicionVista();
        }

        private void BtnOptimizaciones_Click(object sender, RoutedEventArgs e)
        {
            ResaltarBotonActivo(sender);
            OcultarPaneles();
            PnlOptimizacionesWrapper.Visibility = Visibility.Visible;
            LstOptimizaciones.Visibility = Visibility.Collapsed;
            BtnEvaluarSistema.Visibility = Visibility.Visible;
            TxtConsola.Text = "Presiona 'EVALUAR SISTEMA' para escanear el equipo y determinar optimizaciones aplicables.";
            AnimarTransicionVista();
        }

        private async void BtnEvaluarSistema_Click(object sender, RoutedEventArgs e)
        {
            BtnEvaluarSistema.Visibility = Visibility.Collapsed;
            LstOptimizaciones.Visibility = Visibility.Visible;
            TxtConsola.Text = "Escaneando configuración actual (WMI)...\n";
            
            var hwInfo = await _servicioHardware.ObtenerInformacionAsync();
            
            try 
            {
                var resultadoDb = await _baseDatos.CargarReglasAsync();
                _reglasEnMemoria = resultadoDb.Reglas;
                
                if (resultadoDb.CargadoDesdeDisco)
                {
                    TxtConsola.Text += "INFO: optimizaciones.json cargado exitosamente desde disco físico.\n";
                }
                else
                {
                    TxtConsola.Text += "WARNING: optimizaciones.json no encontrado en disco. Se inyectó Fallback en memoria.\n";
                }
                
                if (_reglasEnMemoria.Count == 0)
                {
                    TxtConsola.Text += "No se encontraron reglas en la base de datos de conocimiento.\n";
                    return;
                }
            }
            catch (Exception ex)
            {
                TxtConsola.Text += $"ERROR LEYENDO BASE DE DATOS:\n{ex.Message}\nInyectando reglas por defecto como fallback...";
                _reglasEnMemoria = await _baseDatos.RestaurarReglasPorDefectoAsync();
            }

            TxtConsola.Text += "\nEjecutando Motor de Decisión...\n";
            var items = new List<ViewModelOptimizacion>();
            _reglasAAplicar.Clear();
            int countOptimizado = 0;

            foreach (var r in _reglasEnMemoria)
            {
                var decision = _motorDecision.EvaluarRegla(r, hwInfo);
                string textoDecision = "";
                
                if (decision == DecisionOptimizacion.Optimizar)
                {
                    bool yaOptimo = await _motorOptimizacion.YaEstaOptimoAsync(r);
                    if (yaOptimo)
                    {
                        textoDecision = "DECISIÓN: YA ESTÁ ÓPTIMO. No se requiere acción.";
                    }
                    else
                    {
                        string reinicioInfo = r.RequiereReinicio ? " [REINICIO REQUERIDO]" : "";
                        string compatInfo = r.Id == "WIN-HAGS-001" ? " [COMPATIBILIDAD NO DETERMINADA]" : "";
                        textoDecision = $"DECISIÓN: OPTIMIZAR.{reinicioInfo}{compatInfo} (Evidencia: {r.Evidencia.Nivel})";
                        _reglasAAplicar.Add(r);
                        countOptimizado++;
                    }
                }
                else if (decision == DecisionOptimizacion.RequiereConfirmacionManual)
                {
                    textoDecision = "DECISIÓN: BLOQUEADO (Riesgo Extremo). Requiere autorización manual.";
                }
                else if (decision == DecisionOptimizacion.NoDeterminado)
                {
                    textoDecision = "DECISIÓN: NO APLICABLE (Compatibilidad no determinada).";
                }
                else
                {
                    textoDecision = $"DECISIÓN RECHAZADA: {decision}";
                }
                
                items.Add(new ViewModelOptimizacion 
                { 
                    ReglaId = r.Id, 
                    Nombre = r.Nombre, 
                    Decision = textoDecision, 
                    MostrarBotonAutorizar = (decision == DecisionOptimizacion.RequiereConfirmacionManual),
                    ReglaOriginal = r 
                });
            }

            TxtConsola.Text += $"Evaluación completada. Optimizaciones aplicables: {countOptimizado}. Seleccione 'Why Engine' para inspeccionar la evidencia de cada regla.";
            LstOptimizaciones.ItemsSource = items;
            
            BtnAplicarOptimizaciones.Visibility = Visibility.Visible;
            if (countOptimizado == 0)
            {
                BtnAplicarOptimizaciones.Content = "APLICAR Y AUDITAR (SISTEMA YA ÓPTIMO)";
                BtnAplicarOptimizaciones.IsEnabled = false; 
            }
            else
            {
                BtnAplicarOptimizaciones.Content = "APLICAR Y AUDITAR (EXPORTAR REPORTE)";
                BtnAplicarOptimizaciones.IsEnabled = true;
            }
        }

        private void BtnWhyEngine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                var regla = _reglasEnMemoria.FirstOrDefault(r => r.Id == id);
                if (regla != null)
                {
                    WhyTitulo.Text = regla.Nombre;
                    WhyDescripcion.Text = regla.Descripcion;
                    WhyExplicacion.Text = regla.Evidencia.Explicacion;
                    WhyConfianza.Text = $"{regla.Evidencia.Confianza}%";
                    WhyRiesgo.Text = regla.Riesgo.ToString().ToUpper();
                    WhyFuentes.ItemsSource = regla.Evidencia.Fuentes;
                    
                    PanelWhyEngine.Visibility = Visibility.Visible;
                }
            }
        }

        private void BtnCerrarWhyEngine_Click(object sender, RoutedEventArgs e)
        {
            PanelWhyEngine.Visibility = Visibility.Collapsed;
        }

        private async void BtnAplicarOptimizaciones_Click(object sender, RoutedEventArgs e)
        {
            BtnAplicarOptimizaciones.IsEnabled = false;
            bool simulacion = ChkSimulacion.IsChecked ?? true;
            
            TxtConsola.Text = simulacion 
                ? "Iniciando ciclo de optimización en MODO SIMULACIÓN...\n" 
                : "Iniciando ciclo de optimización REAL...\n";
                
            var detallesReporte = new List<ServicioReporteAuditoria.DetalleOptimizacion>();

            foreach(var regla in _reglasAAplicar)
            {
                TxtConsola.Text += $"- Aplicando: {regla.Nombre}...\n";
                
                var (fueAplicado, msg, detallesTecnicos) = await _motorOptimizacion.AplicarReglaDinamicaAsync(regla, simulacion);
                
                string estadoFinalStr = fueAplicado 
                    ? (simulacion ? "SIMULADO (NO ESCRITO)" : "CONSERVADO") 
                    : "REVERTIDO / NO APLICADO";
                
                var detalle = new ServicioReporteAuditoria.DetalleOptimizacion
                {
                    IdRegla = regla.Id,
                    NombreRegla = regla.Nombre,
                    Explicacion = regla.Evidencia.Explicacion ?? regla.Descripcion,
                    DetalleTecnico = detallesTecnicos,
                    ResultadoVerificacion = msg,
                    EstadoFinal = estadoFinalStr
                };
                
                detallesReporte.Add(detalle);
            }

            TxtConsola.Text += "\nOptimizaciones aplicadas. Generando Reporte de Auditoría...\n";
            
            var hwInfo = await _servicioHardware.ObtenerInformacionAsync();
            var resultadoPerfil = _motorPerfiles.DetectarPerfilAutomatico(hwInfo, new ResultadosDiagnostico());
            
            string rutaReporte = await _servicioAuditoria.GenerarReporteAsync(hwInfo, resultadoPerfil.PerfilSugerido.ToString(), detallesReporte, simulacion);
            
            TxtConsola.Text += $"¡Reporte generado exitosamente!\nGuardado en: {rutaReporte}";
            BtnAplicarOptimizaciones.Visibility = Visibility.Collapsed;
            BtnAplicarOptimizaciones.IsEnabled = true;
        }

        private void ChkAutorizar_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk && chk.Tag is string reglaId)
            {
                var reglaInfo = ((List<ViewModelOptimizacion>)LstOptimizaciones.ItemsSource).FirstOrDefault(r => r.ReglaId == reglaId);
                if (reglaInfo != null)
                {
                    reglaInfo.EstaAutorizada = chk.IsChecked ?? false;
                    
                    if (reglaInfo.EstaAutorizada && !_reglasAAplicar.Any(r => r.Id == reglaId))
                    {
                        _reglasAAplicar.Add(reglaInfo.ReglaOriginal);
                        TxtConsola.Text += $"\n[!] ADVERTENCIA: Has autorizado manualmente la regla '{reglaInfo.Nombre}'. Se aplicará bajo tu propia responsabilidad.";
                    }
                    else if (!reglaInfo.EstaAutorizada && _reglasAAplicar.Any(r => r.Id == reglaId))
                    {
                        _reglasAAplicar.RemoveAll(r => r.Id == reglaId);
                        TxtConsola.Text += $"\nSe ha retirado la autorización para la regla '{reglaInfo.Nombre}'.";
                    }

                    // Actualizar estado del botón
                    if (_reglasAAplicar.Count > 0)
                    {
                        BtnAplicarOptimizaciones.Content = "APLICAR Y AUDITAR (EXPORTAR REPORTE)";
                    }
                }
            }
        }
        private void BtnActualizarProcesos_Click(object sender, RoutedEventArgs e)
        {
            CargarProcesosComboBox();
        }

        private void CargarProcesosComboBox()
        {
            try
            {
                var procesos = Process.GetProcesses()
                    .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                    .OrderBy(p => p.ProcessName)
                    .Select(p => new { Display = $"{p.ProcessName}.exe — PID {p.Id}", Pid = p.Id, Nombre = p.ProcessName })
                    .ToList();

                CmbProcesos.ItemsSource = procesos;
                CmbProcesos.DisplayMemberPath = "Display";
                CmbProcesos.SelectedValuePath = "Pid";
                if (procesos.Count > 0) CmbProcesos.SelectedIndex = 0;
                
                TxtConsola.Text = $"Se encontraron {procesos.Count} procesos con ventana activa.\n";
            }
            catch (Exception ex)
            {
                TxtConsola.Text = $"Error al obtener procesos: {ex.Message}\n";
            }
        }

        private async void BtnIniciarBenchmark_Click(object sender, RoutedEventArgs e)
        {
            if (CmbProcesos.SelectedValue == null)
            {
                TxtConsola.Text = "Por favor, selecciona un proceso objetivo primero.\n";
                return;
            }

            dynamic selectedProcess = CmbProcesos.SelectedItem;
            _benchmarkTargetPid = selectedProcess.Pid;
            _benchmarkTargetName = selectedProcess.Nombre;

            double durationSeconds = 0;
            switch (CmbDuracion.SelectedIndex)
            {
                case 1: durationSeconds = 30; break;
                case 2: durationSeconds = 60; break;
                case 3: durationSeconds = 300; break;
            }

            BtnIniciarBenchmark.IsEnabled = false;
            BtnActualizarProcesos.IsEnabled = false;
            CmbProcesos.IsEnabled = false;
            CmbDuracion.IsEnabled = false;
            BtnDetenerBenchmark.IsEnabled = true;
            BtnDetenerBenchmark.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkRed);

            _benchmarkCts = new CancellationTokenSource();
            _motorPruebas.IniciarBenchmarkGaming(_benchmarkTargetPid);
            
            TxtEstadoBenchmark.Text = $"Estado: Capturando PID {_benchmarkTargetPid}...";
            TxtConsola.Text = $"Iniciando Benchmark ETW para {_benchmarkTargetName} (PID: {_benchmarkTargetPid})...\n";

            try
            {
                var sw = Stopwatch.StartNew();
                while (!_benchmarkCts.Token.IsCancellationRequested)
                {
                    await Task.Delay(1000, _benchmarkCts.Token);
                    TxtDuracionValor.Text = $"{sw.Elapsed.TotalSeconds:F1} s";

                    // Verificar si el proceso sigue vivo
                    try
                    {
                        var p = Process.GetProcessById(_benchmarkTargetPid);
                        if (p.HasExited) throw new Exception("HasExited");
                    }
                    catch
                    {
                        TxtEstadoBenchmark.Text = "Estado: Interrumpido (Proceso cerrado)";
                        TxtConsola.Text += "El proceso objetivo se ha cerrado. Deteniendo captura...\n";
                        FinalizarBenchmark(sw.Elapsed.TotalSeconds);
                        return;
                    }

                    if (durationSeconds > 0 && sw.Elapsed.TotalSeconds >= durationSeconds)
                    {
                        FinalizarBenchmark(sw.Elapsed.TotalSeconds);
                        return;
                    }
                }
            }
            catch (TaskCanceledException) { }
        }

        private void BtnDetenerBenchmark_Click(object sender, RoutedEventArgs e)
        {
            if (_benchmarkCts != null && !_benchmarkCts.IsCancellationRequested)
            {
                _benchmarkCts.Cancel();
                FinalizarBenchmark(0); // El valor exacto se calculará o actualizará en la UI ya
            }
        }

        private void FinalizarBenchmark(double duracionReal)
        {
            if (_benchmarkCts != null)
            {
                _benchmarkCts.Cancel();
                _benchmarkCts.Dispose();
                _benchmarkCts = null;
            }

            TxtEstadoBenchmark.Text = "Estado: Finalizando...";
            
            var resultado = _motorPruebas.DetenerBenchmarkGaming(_benchmarkTargetName, duracionReal);

            BtnIniciarBenchmark.IsEnabled = true;
            BtnActualizarProcesos.IsEnabled = true;
            CmbProcesos.IsEnabled = true;
            CmbDuracion.IsEnabled = true;
            BtnDetenerBenchmark.IsEnabled = false;
            BtnDetenerBenchmark.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4A5568"));

            TxtEstadoBenchmark.Text = "Estado: Completado";
            TxtDuracionValor.Text = $"{resultado.DuracionSegundos:F1} s";
            TxtMuestrasValor.Text = $"{resultado.MuestrasValidas}";

            if (resultado.MuestrasValidas < 2)
            {
                TxtEstadoBenchmark.Text = "Estado: Datos insuficientes";
                TxtConsola.Text += "Muestras insuficientes para calcular resultados reales.\n";
                PbFps.Value = 0; TxtFpsValor.Text = "NO DISP.";
                PbLows.Value = 0; TxtLowsValor.Text = "NO DISP.";
                PbZeroOneLows.Value = 0; TxtZeroOneLowsValor.Text = "NO DISP.";
                PbFrametime.Value = 0; TxtFrametimeValor.Text = "NO DISP.";
                return;
            }

            PbFps.Value = Math.Max(0, Math.Min(resultado.FpsPromedio, 1000));
            TxtFpsValor.Text = $"{resultado.FpsPromedio:F1} fps";
            
            PbLows.Value = Math.Max(0, Math.Min(resultado.Fps1Porciento, 1000));
            TxtLowsValor.Text = $"{resultado.Fps1Porciento:F1} fps";

            if (resultado.Fps01Porciento > 0)
            {
                PbZeroOneLows.Value = Math.Max(0, Math.Min(resultado.Fps01Porciento, 1000));
                TxtZeroOneLowsValor.Text = $"{resultado.Fps01Porciento:F1} fps";
            }
            else
            {
                PbZeroOneLows.Value = 0;
                TxtZeroOneLowsValor.Text = "NO DISP.";
            }
            
            PbFrametime.Value = Math.Max(0, Math.Min(resultado.FrametimePromedioMs, 100));
            TxtFrametimeValor.Text = $"{resultado.FrametimePromedioMs:F2} ms";

            TxtConsola.Text += $"--- DIAGNÓSTICO GAMING ---\n" +
                             $"Proceso: {resultado.ProcesoObjetivo} (PID: {resultado.PidObjetivo})\n" +
                             $"Fuente: ETW / DxgKrnl / Present (Aislado por PID)\n" +
                             $"FPS Promedio: {resultado.FpsPromedio:F1} FPS\n" +
                             $"1% Lows: {resultado.Fps1Porciento:F1} FPS\n" +
                             $"0.1% Lows: {(resultado.Fps01Porciento > 0 ? resultado.Fps01Porciento.ToString("F1") + " FPS" : "NO DISP. (Requiere > 1000 muestras)")}\n" +
                             $"Frametime Promedio: {resultado.FrametimePromedioMs:F2} ms\n" +
                             $"Muestras Válidas: {resultado.MuestrasValidas}\n\n";
        }

        private void BtnPruebas_Click(object sender, RoutedEventArgs e)
        {
            ResaltarBotonActivo(sender);
            OcultarPaneles();
            PnlGraficosBenchmark.Visibility = Visibility.Visible;
            AnimarTransicionVista();
            CargarProcesosComboBox();
        }
        public class ViewModelHistorial
        {
            public string IdTransaccion { get; set; } = string.Empty;
            public string ReglaId { get; set; } = string.Empty;
            public string NombreRegla { get; set; } = string.Empty;
            public DateTime FechaEjecucion { get; set; }
            public string ValorAnterior { get; set; } = string.Empty;
            public string ValorNuevo { get; set; } = string.Empty;
            public bool FueRevertida { get; set; }
            
            public string TextoBoton => FueRevertida ? "REVERTIDO" : "REVERTIR";
            public bool PuedeRevertir => !FueRevertida;
        }

        private async void BtnHistorial_Click(object sender, RoutedEventArgs e)
        {
            ResaltarBotonActivo(sender);
            OcultarPaneles();
            PnlHistorialWrapper.Visibility = Visibility.Visible;
            
            TxtConsola.Text = "Cargando historial de transacciones (Rollback Engine)...";
            AnimarTransicionVista();
            
            var transacciones = await _gestorTransacciones.ObtenerHistorialAsync();
            var viewModels = new List<ViewModelHistorial>();
            
            if (_reglasEnMemoria == null || _reglasEnMemoria.Count == 0)
            {
                var resultadoDb = await _baseDatos.CargarReglasAsync();
                _reglasEnMemoria = resultadoDb.Reglas;
            }

            foreach(var t in transacciones.OrderByDescending(x => x.FechaEjecucion))
            {
                var reglaInfo = _reglasEnMemoria.FirstOrDefault(r => r.Id == t.IdRegla);
                string nombre = reglaInfo != null ? reglaInfo.Nombre : $"Desconocida ({t.IdRegla})";
                
                viewModels.Add(new ViewModelHistorial
                {
                    IdTransaccion = t.IdTransaccion,
                    ReglaId = t.IdRegla,
                    NombreRegla = nombre,
                    FechaEjecucion = t.FechaEjecucion,
                    ValorAnterior = t.ValorAnterior,
                    ValorNuevo = t.ValorNuevo,
                    FueRevertida = t.FueRevertida
                });
            }
            
            LstHistorial.ItemsSource = viewModels;
            
            TxtConsola.Text = $"Se encontraron {transacciones.Count} transacciones en el historial local.";
        }

        private async void BtnRevertir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string idTx)
            {
                TxtConsola.Text = $"Revirtiendo transacción: {idTx} ...";
                var exito = await _gestorTransacciones.RevertirEspecificaAsync(idTx);
                if (exito)
                {
                    TxtConsola.Text += "\n¡Reversión exitosa! El registro ha vuelto a su estado original.";
                    BtnHistorial_Click(this, new RoutedEventArgs()); // Refresh list
                }
                else
                {
                    TxtConsola.Text += "\nERROR: No se pudo revertir la transacción. Revisa permisos de Administrador.";
                }
            }
        }

        private async void BtnRevertirTodo_Click(object sender, RoutedEventArgs e)
        {
            BtnRevertirTodo.IsEnabled = false;
            TxtConsola.Text = "Iniciando reversión global de todas las transacciones...";
            var transacciones = await _gestorTransacciones.ObtenerHistorialAsync();
            int exitos = 0;
            int errores = 0;

            foreach (var t in transacciones.OrderByDescending(x => x.FechaEjecucion))
            {
                if (!t.FueRevertida)
                {
                    TxtConsola.Text += $"\nRevirtiendo {t.IdTransaccion}...";
                    var exito = await _gestorTransacciones.RevertirEspecificaAsync(t.IdTransaccion);
                    if (exito) exitos++;
                    else errores++;
                }
            }

            TxtConsola.Text += $"\nReversión global completada. Exitosos: {exitos}, Errores: {errores}.";
            BtnRevertirTodo.IsEnabled = true;
            BtnHistorial_Click(this, new RoutedEventArgs());
        }

        private void BtnProcesos_Click(object sender, RoutedEventArgs e)
        {
            ResaltarBotonActivo(sender);
            OcultarPaneles();
            PnlProcesosWrapper.Visibility = Visibility.Visible;
            LstProcesos.Visibility = Visibility.Collapsed;
            
            TxtConsola.Text = "Presiona 'ESCANEAR PROCESOS AHORA' para analizar los procesos en segundo plano usando System.Diagnostics.Process.";
            AnimarTransicionVista();
        }

        private void BtnEscanearProcesos_Click(object sender, RoutedEventArgs e)
        {
            LstProcesos.Visibility = Visibility.Visible;
            TxtConsola.Text = "Analizando consumo de memoria en procesos en segundo plano...\n";
            
            var procesos = _motorProcesos.AnalizarProcesosSegundoPlano();
            LstProcesos.ItemsSource = procesos;
            
            TxtConsola.Text += $"\nAnálisis completo. Se listaron los procesos con mayor consumo de memoria en segundo plano.";
        }
    }
}