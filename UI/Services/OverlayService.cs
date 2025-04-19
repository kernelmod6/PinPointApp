using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Diagnostics;
using System.Text;

namespace PinPoint.UI.Services
{
    public static class OverlayService
    {
        private static bool s_dllLoaded = false;
        private static IntPtr s_dllHandle = IntPtr.Zero;
        private static IntPtr s_pInitializeOverlay = IntPtr.Zero;
        private static IntPtr s_pShutdownOverlay = IntPtr.Zero;
        private static IntPtr s_pShowOverlay = IntPtr.Zero;
        private static IntPtr s_pSetCrosshairColor = IntPtr.Zero;
        private static IntPtr s_pSetCrosshairSize = IntPtr.Zero;
        private static IntPtr s_pSetCrosshairThickness = IntPtr.Zero;
        private static IntPtr s_pSetCrosshairStyle = IntPtr.Zero;

        [DllImport("OverlayEngine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool InitializeOverlay();

        [DllImport("OverlayEngine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ShutdownOverlay();

        [DllImport("OverlayEngine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ShowOverlay(bool show);

        [DllImport("OverlayEngine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCrosshairColor(float r, float g, float b, float a);

        [DllImport("OverlayEngine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCrosshairSize(float size);

        [DllImport("OverlayEngine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCrosshairThickness(float thickness);

        [DllImport("OverlayEngine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCrosshairStyle(int style);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool InitializeOverlayDelegate();

        public static bool Initialize()
        {
            try
            {
                CheckDllExists();

                try
                {
                    return InitializeOverlay();
                }
                catch (DllNotFoundException ex)
                {
                    Console.WriteLine($"P/Invoke approach failed: {ex.Message}");

                    if (LoadOverlayEngineDll())
                    {
                        Console.WriteLine("Successfully loaded DLL manually, calling InitializeOverlay");
                        var initializeDelegate = Marshal.GetDelegateForFunctionPointer<InitializeOverlayDelegate>(s_pInitializeOverlay);
                        return initializeDelegate();
                    }
                    else
                    {
                        Console.WriteLine("Failed to load DLL manually");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleDllError("initializing overlay", ex);
                return false;
            }
        }

        public static void Shutdown()
        {
            try
            {
                if (!s_dllLoaded) return;
                ShutdownOverlay();
            }
            catch (Exception ex)
            {
                HandleDllError("shutting down overlay", ex);
            }
        }

        public static void ShowOverlayWrapper(bool show)
        {
            try
            {
                if (!s_dllLoaded) return;
                ShowOverlay(show);
            }
            catch (Exception ex)
            {
                HandleDllError("showing/hiding overlay", ex);
            }
        }

        public static void SetCrosshairColorWrapper(float r, float g, float b, float a)
        {
            try
            {
                if (!s_dllLoaded) return;
                SetCrosshairColor(r, g, b, a);
            }
            catch (Exception ex)
            {
                HandleDllError("setting crosshair color", ex);
            }
        }

        public static void SetCrosshairSizeWrapper(float size)
        {
            try
            {
                if (!s_dllLoaded) return;
                SetCrosshairSize(size);
            }
            catch (Exception ex)
            {
                HandleDllError("setting crosshair size", ex);
            }
        }

        public static void SetCrosshairThicknessWrapper(float thickness)
        {
            try
            {
                if (!s_dllLoaded) return;
                SetCrosshairThickness(thickness);
            }
            catch (Exception ex)
            {
                HandleDllError("setting crosshair thickness", ex);
            }
        }

        public static void SetCrosshairStyleWrapper(int style)
        {
            try
            {
                if (!s_dllLoaded) return;
                SetCrosshairStyle(style);
            }
            catch (Exception ex)
            {
                HandleDllError("setting crosshair style", ex);
            }
        }

        private static void CheckDllExists()
        {
            if (s_dllLoaded) return;

            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OverlayEngine.dll");

            if (!File.Exists(dllPath))
            {
                MessageBox.Show($"OverlayEngine.dll not found at:\n{dllPath}", "Missing DLL", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new FileNotFoundException("OverlayEngine.dll not found", dllPath);
            }

            s_dllLoaded = true;
        }

        private static void HandleDllError(string operation, Exception ex)
        {
            if (ex is DllNotFoundException || ex is EntryPointNotFoundException)
            {
                MessageBox.Show($"Error while {operation}: {ex.Message}", "DLL Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Console.WriteLine($"Error {operation}: {ex.Message}");
            }

            s_dllLoaded = false;
        }

        private static bool LoadOverlayEngineDll()
        {
            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OverlayEngine.dll");

            s_dllHandle = LoadLibrary(dllPath);
            if (s_dllHandle == IntPtr.Zero) return false;

            s_pInitializeOverlay = GetProcAddress(s_dllHandle, "InitializeOverlay");
            s_pShutdownOverlay = GetProcAddress(s_dllHandle, "ShutdownOverlay");
            s_pShowOverlay = GetProcAddress(s_dllHandle, "ShowOverlay");
            s_pSetCrosshairColor = GetProcAddress(s_dllHandle, "SetCrosshairColor");
            s_pSetCrosshairSize = GetProcAddress(s_dllHandle, "SetCrosshairSize");
            s_pSetCrosshairThickness = GetProcAddress(s_dllHandle, "SetCrosshairThickness");
            s_pSetCrosshairStyle = GetProcAddress(s_dllHandle, "SetCrosshairStyle");

            return s_pInitializeOverlay != IntPtr.Zero &&
                   s_pShutdownOverlay != IntPtr.Zero &&
                   s_pShowOverlay != IntPtr.Zero &&
                   s_pSetCrosshairColor != IntPtr.Zero &&
                   s_pSetCrosshairSize != IntPtr.Zero &&
                   s_pSetCrosshairThickness != IntPtr.Zero &&
                   s_pSetCrosshairStyle != IntPtr.Zero;
        }

        static OverlayService()
        {
            try
            {
                string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OverlayEngine.dll");
                Console.WriteLine($"DLL Path: {dllPath}");
                Console.WriteLine($"DLL Exists: {File.Exists(dllPath)}");
                Console.WriteLine($"Process Architecture: {(Environment.Is64BitProcess ? "x64" : "x86")}");

                if (File.Exists(dllPath))
                {
                    FileStream fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);

                    fs.Position = 0x3C;
                    int peHeaderOffset = br.ReadInt32();

                    fs.Position = peHeaderOffset + 4 + 20;
                    ushort architecture = br.ReadUInt16();

                    string archString = architecture == 0x8664 ? "x64" : architecture == 0x014c ? "x86" : "Unknown";
                    Console.WriteLine($"DLL Architecture: {archString} (0x{architecture:X4})");

                    br.Close();
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking DLL architecture: {ex.Message}");
            }

            try
            {
                string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OverlayEngine.dll");
                IntPtr dllHandle = LoadLibrary(dllPath);
                if (dllHandle != IntPtr.Zero)
                {
                    Console.WriteLine("Successfully loaded DLL manually");
                    FreeLibrary(dllHandle);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception trying to load DLL manually: {ex.Message}");
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId,
            uint dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr Arguments);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    }
}
