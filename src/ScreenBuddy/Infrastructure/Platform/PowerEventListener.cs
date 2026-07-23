using Microsoft.Win32;
using System;

namespace ScreenBuddy.Infrastructure.Platform
{
    public interface IPowerEventListener : IDisposable
    {
        event EventHandler? SystemResuming;
    }

    /// <summary>
    /// Listens to Windows OS SystemEvents power mode changes for wake-from-sleep reconciliation.
    /// </summary>
    public sealed class PowerEventListener : IPowerEventListener
    {
        public event EventHandler? SystemResuming;

        public PowerEventListener()
        {
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                SystemResuming?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        }
    }
}
