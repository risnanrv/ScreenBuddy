using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ScreenBuddy.Infrastructure.Platform
{
    public sealed record MonitorInfo
    {
        public Rectangle Bounds { get; init; }
        public bool IsPrimary { get; init; }
        public string DeviceName { get; init; } = string.Empty;
    }

    public interface IDisplayMonitor
    {
        event EventHandler? DisplayConfigurationChanged;
        IReadOnlyList<MonitorInfo> GetMonitors();
    }

    /// <summary>
    /// Enumerates connected Windows displays and monitors display topology changes.
    /// </summary>
    public sealed class DisplayMonitor : IDisplayMonitor
    {
        public event EventHandler? DisplayConfigurationChanged;

        public DisplayMonitor()
        {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }

        public IReadOnlyList<MonitorInfo> GetMonitors()
        {
            var list = new List<MonitorInfo>();
            foreach (Screen screen in Screen.AllScreens)
            {
                list.Add(new MonitorInfo
                {
                    Bounds = screen.Bounds,
                    IsPrimary = screen.Primary,
                    DeviceName = screen.DeviceName
                });
            }
            return list.AsReadOnly();
        }

        private void OnDisplaySettingsChanged(object? sender, EventArgs e)
        {
            DisplayConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
