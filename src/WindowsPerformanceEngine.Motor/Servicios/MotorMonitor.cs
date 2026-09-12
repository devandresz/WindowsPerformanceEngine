using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace WindowsPerformanceEngine.Motor.Servicios
{
    public class MotorMonitor
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int ENUM_REGISTRY_SETTINGS = -2;

        public (int ResolucionX, int ResolucionY, int FrecuenciaActualHz) ObtenerConfiguracionMonitorPrincipal()
        {
            DEVMODE vDevMode = new DEVMODE();
            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref vDevMode))
            {
                return (vDevMode.dmPelsWidth, vDevMode.dmPelsHeight, vDevMode.dmDisplayFrequency);
            }
            return (0, 0, 0); // Falla de lectura
        }
    }
}
