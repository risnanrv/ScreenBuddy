using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace ScreenBuddy.Infrastructure.Platform
{
    public interface IStartupRegistrar
    {
        bool IsEnabled();
        void Enable();
        void Disable();
    }

    /// <summary>
    /// Manages Windows HKCU registry autorun integration for ScreenBuddy.
    /// </summary>
    public sealed class StartupRegistrar : IStartupRegistrar
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "ScreenBuddy";

        public bool IsEnabled()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }

        public void Enable()
        {
            try
            {
                string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath))
                {
                    return;
                }

                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                key?.SetValue(AppName, $"\"{exePath}\"");
            }
            catch
            {
                // Fail-safe
            }
        }

        public void Disable()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                key?.DeleteValue(AppName, false);
            }
            catch
            {
                // Fail-safe
            }
        }
    }
}
