using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;

namespace ScreenBuddy.Infrastructure.Platform
{
    public sealed record MonitorInfo
    {
        public Rect Bounds { get; init; }
        public bool IsPrimary { get; init; }
        public string DeviceName { get; init; } = string.Empty;
    }

    public interface IDisplayMonitor
    {
        event EventHandler? DisplayConfigurationChanged;
        IReadOnlyList<MonitorInfo> GetMonitors();
    }

    /// <summary>
    /// Enumerates connected Windows displays via Win32 EnumDisplayMonitors P/Invoke.
    /// Pure WPF implementation with zero Windows Forms dependencies.
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
            var monitors = new List<MonitorInfo>();
            NativeMonitorMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref NativeMonitorMethods.RECT lprcMonitor, IntPtr dwData) =>
            {
                var mi = new NativeMonitorMethods.MONITORINFOEX();
                mi.cbSize = Marshal.SizeOf(mi);
                if (NativeMonitorMethods.GetMonitorInfo(hMonitor, ref mi))
                {
                    bool isPrimary = (mi.dwFlags & NativeMonitorMethods.MONITORINFOF_PRIMARY) != 0;
                    double left = mi.rcMonitor.Left;
                    double top = mi.rcMonitor.Top;
                    double width = mi.rcMonitor.Right - mi.rcMonitor.Left;
                    double height = mi.rcMonitor.Bottom - mi.rcMonitor.Top;

                    monitors.Add(new MonitorInfo
                    {
                        Bounds = new Rect(left, top, width, height),
                        IsPrimary = isPrimary,
                        DeviceName = new string(mi.szDevice).TrimEnd('\0')
                    });
                }
                return true;
            }, IntPtr.Zero);

            if (monitors.Count == 0)
            {
                // Fallback to Primary Screen via WPF SystemParameters
                monitors.Add(new MonitorInfo
                {
                    Bounds = new Rect(0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight),
                    IsPrimary = true,
                    DeviceName = "Primary"
                });
            }

            return monitors.AsReadOnly();
        }

        private void OnDisplaySettingsChanged(object? sender, EventArgs e)
        {
            DisplayConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal static class NativeMonitorMethods
    {
        public const uint MONITORINFOF_PRIMARY = 1;

        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }
    }
}
