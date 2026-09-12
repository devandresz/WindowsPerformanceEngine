# Windows Performance Engine

Un sistema inteligente y autónomo para el diagnóstico, optimización y validación del rendimiento de Windows. Creado con evidencia empírica medible, arquitectura estricta y C# en WPF para .NET 10 LTS.

## Qué es
Windows Performance Engine es un marco (framework) avanzado de ingeniería diseñado para medir y aplicar optimizaciones condicionales dependiendo de los cuellos de botella de hardware en el sistema operativo Windows.

## Qué NO es
- No es un "FPS Booster" mágico.
- No es un conjunto de scripts destructivos (Debloater).
- No asume que configuraciones globales y a ciegas mejorarán el rendimiento en cualquier sistema.

## Arquitectura y Módulos
1. **Motor de Diagnóstico**: Detecta CPU, RAM y GPU utilizando telemetría local.
2. **Motor de Reglas**: Implementación de interfaces desacopladas (`IAplicadorRegla`) respaldadas por JSON.
3. **Modo Simulación**: Toda la interfaz del registro y servicios puede operar en "Dry-Run" sin afectar al sistema real.
4. **Rollback Nativo**: Las transacciones capturan los valores anteriores y garantizan una vuelta atrás segura (Patrón `Transaccion`).

## Uso (Development)
- El proyecto objetivo (TFM) está establecido estrictamente a `.NET 10 LTS` (`net10.0-windows`).
- Requiere Visual Studio 2026 o el SDK .NET 10.0 correspondiente.
- Iniciar el proyecto `WindowsPerformanceEngine` en la raíz (Start Up).

## Seguridad
Nunca se descartan los protocolos de seguridad de Microsoft (VBS/HVCI/Defender) sin confirmación activa, intencional y manual del usuario. No se ejecuta código remoto. Las pruebas de red utilizan herramientas de ICMP nativas o conexiones TLS controladas.
