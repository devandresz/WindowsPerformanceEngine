# 📖 Windows Performance Engine - Documentación Técnica y Estado del Proyecto

## Introducción

**Windows Performance Engine (W.P.E.)** nace de la visión de crear una herramienta de ingeniería autónoma, transparente y de alto rendimiento para optimizar entornos Windows, especialmente enfocada en el Gaming y la baja latencia (Esports). 

A diferencia de los "optimizadores" tradicionales (bloatware) que modifican el registro a ciegas o inyectan procesos peligrosos, W.P.E. se construyó sobre un paradigma **Transaccional y Científico**. Cada optimización es tratada como una transacción atómica (con posibilidad de Rollback), y se valida estadísticamente mediante telemetría del Kernel (ETW) para confirmar si realmente mejora el frametime (1% Lows) o si, por el contrario, causa una regresión de rendimiento. Todo el sistema está programado nativamente en **C# y .NET 10 LTS**, garantizando portabilidad y seguridad sin depender de librerías externas u oscuras.

---

## 📂 Arquitectura de Carpetas y Archivos

El código se divide en una arquitectura N-Capas, separando estrictamente la UI de la lógica de negocio (Motor). A continuación, se detalla cada componente y su responsabilidad:

### 1. Proyecto UI (`src/WindowsPerformanceEngine/`)
Contiene la capa de presentación (WPF). Es responsable única y exclusivamente de renderizar los datos y comunicarse con el Motor.
*   **`MainWindow.xaml`**: El corazón visual del proyecto. Define el Dashboard moderno, las tarjetas de telemetría (CPU, RAM, GPU, OS), las barras de progreso para el Benchmark gráfico y los paneles laterales de navegación.
*   **`MainWindow.xaml.cs`**: Code-behind que actúa como controlador. Instancia e inyecta los servicios del Motor, maneja el *Click* de los botones y la actualización asíncrona de la UI (ej. ocultar paneles, dibujar barras de progreso).
*   **`WindowsPerformanceEngine.csproj`**: Archivo de proyecto que apunta a `.NET 10 LTS`. Configurado para generar un único ejecutable auto-contenido (`PublishSingleFile`).

### 2. Capa de Datos Externa (`base-datos-optimizacion/`)
*   **`optimizaciones.json`**: El "Cerebro" de conocimiento empírico. Contiene las reglas estructuradas que el Motor evalúa. Cada regla incluye su Nivel de Riesgo, Evidencia Científica (fuentes bibliográficas) e indica explícitamente si requiere reinicio. Aquí configuramos el forzado de Game Mode, exclusiones de Defender, HAGS, y mitigación de placebos (como el TCP Nagle).

### 3. Proyecto Core (`src/WindowsPerformanceEngine.Motor/`)
Contiene toda la ingeniería, sin referencias a WPF. Es la biblioteca de clases independiente.

#### `Dominio/`
*   **`Modelos/`**: Clases anémicas que representan la información pura.
    *   `InformacionHardware.cs`: Estructura para almacenar la telemetría (CPU, RAM, Placa Base, Monitor).
    *   `ReglaOptimizacion.cs`: Mapeo del JSON a objetos de C#.
    *   `Transaccion.cs`: Registro de los valores Original/Nuevo para el sistema de Rollback.
    *   `ResultadosDiagnostico.cs`: Puntuaciones de red y FPS.
*   **`Interfaces/`**: Contratos (Abstracciones) para mantener el bajo acoplamiento (Ej. `IRegistroWindows`, `IGestorTransacciones`).

#### `IoC/`
*   **`InyectorMotor.cs`**: Gestiona la Inyección de Dependencias. Conecta las interfaces con sus implementaciones y provee todos los servicios (como un contenedor) a la UI.

#### `Servicios/` (Los "Motores")
Aquí reside la inteligencia algorítmica.

1.  **`MotorOptimizacion.cs`**: Responsable de aplicar las reglas en el sistema operativo mediante el Registro. Envuelve cada cambio en un bloque Try-Catch y lo comunica al Gestor de Transacciones. Admite modo Simulación (Dry-Run).
2.  **`GestorTransacciones.cs`**: El sistema de seguridad. Cada vez que se aplica una optimización, este gestor guarda el valor "Antes" y "Después". Si algo falla, expone métodos como `RevertirEspecificaAsync()` para restaurar el PC.
3.  **`MotorRegresion.cs`**: Analiza los resultados del ETW "Antes" y "Después" de una optimización. Si los FPS 1% Low caen más allá del margen estadístico de error (ej. -5%), dispara el Auto-Rollback inmediatamente.
4.  **`MotorEnergiaTermico.cs`**: Sistema de salvaguarda física. Lee los sensores térmicos de la CPU y planes de energía (vía WMI/ACPI). Si detecta estrangulamiento térmico (>90°C), bloquea optimizaciones agresivas en la UI.
5.  **`MotorFpsEtw.cs`**: El motor de alto rendimiento que se engancha directamente al Kernel (Event Tracing for Windows, Proveedor `DxgKrnl`) para medir el `Frametime` real de DirectX en milisegundos sin inyectar DLLs externas.
6.  **`MotorRendimientoGpu.cs`**: Usa contadores de rendimiento de bajo nivel (`PerformanceCounters`) para leer en tiempo real la saturación del bloque 3D de la tarjeta gráfica (AMD/NVIDIA).
7.  **`MotorProcesos.cs`**: "Bloatware Analyzer". Escanea los procesos en ejecución, determina cuánto WorkingSet de RAM consumen e identifica programas redundantes o RGB pesados, alertando al usuario en lugar de cerrar tareas a la fuerza.
8.  **`MotorDecision.cs`**: El juez algorítmico. Evalúa las reglas y el Hardware (ej. bloquea reglas de HDD si detecta un NVMe) y retorna `DecisionOptimizacion.Optimizar` o `Rechazar`.
9.  **`LectorBaseDatosOptimizacion.cs`**: Responsable de leer e hidratar el archivo `optimizaciones.json`.
10. **`ServicioHardware.cs`**: Extractores de WMI para mapear la CPU, RAM, Placa Base, Discos y GPU.
11. **`MotorMonitor.cs`**: Mediante P/Invoke a `User32.dll` (`EnumDisplaySettings`), detecta la tasa de refresco nativa máxima y actual del monitor (ej. 144Hz vs 60Hz).
12. **`ServicioReporteAuditoria.cs`**: El generador de Documentos. Produce un Audit Trail físico (`.txt`) exportable al Escritorio con el Log Transaccional completo tras un ciclo de optimización.

#### `Reglas/`
*   **`OptimizacionGameMode.cs`**: Implementación base para reglas codificadas (Hardcoded) en lugar de JSON, demostrando soporte a reglas programáticas directas.

---

## 🚀 Estado Actual del Proyecto (Cierre y Exportación)

El desarrollo ha alcanzado un estado de **PRODUCTO FINAL**. La herramienta ha superado la fase de Prototipo y la fase de User Acceptance Testing (UAT).
*   **Compilación Estricta:** El proyecto compila bajo `.NET 10 LTS` generando un único archivo ejecutable (PublishSingleFile) `win-x64` que no requiere dependencias.
*   **Cobertura de Tests:** Todos los tests unitarios (`dotnet test`) de la lógica matemática, decisión y regresión pasan al 100%.
*   **UI Dinámica:** La interfaz WPF está refinada, con barras de progreso (ProgressBar) a color, switches de Modo Simulación que funcionan en el flujo principal, y navegación inteligente de paneles.
*   **Listos para Despliegue:** W.P.E. está preparado para implementarse en los ordenadores objetivo. Todo el ciclo lógico (Identificación de Regla -> Validación de Hardware -> Aceptación -> Aplicación -> Verificación ETW -> Auto-Rollback en caso de fallo -> Exportación de Reporte) funciona de forma continua.

---

## 💬 Bitácora del Chat y Evolución (Resumen de las Fases)

*(Nota de Sistema: Debido a la extensión técnica masiva de la conversación, el flujo de desarrollo se resume en sus hitos algorítmicos fundamentales)*

1.  **Génesis (Prompt 0-107):** El proyecto inició con la premisa clara de crear un optimizador que no sea un "Snake-Oil". Acordamos el target framework `.NET 10` y la separación de capas (`WindowsPerformanceEngine` vs `WindowsPerformanceEngine.Motor`).
2.  **Construcción del Core Transaccional:** Se implementaron los motores base (`MotorOptimizacion` y `GestorTransacciones`) permitiendo que cada cambio en el registro de Windows pudiera revertirse. La confianza científica fue integrada directamente en el diseño UML.
3.  **Fase de Alto Rendimiento (ETW y P/Invoke):** Se introdujo C# de bajo nivel. Incorporé la lectura de eventos ETW `Present` y el uso de contadores `PerformanceCounters` para desvincularnos de WMI y obtener latencia (Frametime) real de la tarjeta de video, junto con la detección nativa de Hercios (Hz).
4.  **Expansión de Seguridad (Procesos y Temperatura):** Para prevenir dañar equipos, implementamos el motor de estrangulamiento térmico (`MotorEnergiaTermico.cs`) y el analizador inteligente de procesos (`MotorProcesos.cs`). 
5.  **Fase de Integración WPF:** Uní todas las piezas del rompecabezas en la UI, añadiendo gráficos de barras visuales y conectando los botones laterales.
6.  **UAT y Hotfixes Finales:** 
    *   Se solicitó el sistema de reporte automatizado (`ServicioReporteAuditoria`).
    *   La IA resolvió bugs críticos de XAML de forma autónoma (Checkbox invisible, Botones perdidos, fallos lógicos en la conversión térmica y empaquetado del JSON en el Publish), consolidando la aplicación en su versión Gold final.
