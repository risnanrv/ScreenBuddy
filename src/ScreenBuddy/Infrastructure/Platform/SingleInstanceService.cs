using System;
using System.Threading;

namespace ScreenBuddy.Infrastructure.Platform
{
    /// <summary>
    /// Implements single-instance application activation IPC via Win32 EventWaitHandle.
    /// Allows a second launched instance to signal the primary instance to bring its MainWindow to the foreground.
    /// </summary>
    public sealed class SingleInstanceService : IDisposable
    {
        private const string EventName = "ScreenBuddy_Activate_Event_v1.1_9A8B7C";
        private EventWaitHandle? _eventWaitHandle;
        private RegisteredWaitHandle? _registeredWaitHandle;

        public void StartListening(Action onActivationRequested)
        {
            ArgumentNullException.ThrowIfNull(onActivationRequested);

            bool createdNew;
            _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, EventName, out createdNew);

            _registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                _eventWaitHandle,
                (state, timedOut) =>
                {
                    if (!timedOut)
                    {
                        onActivationRequested();
                    }
                },
                null,
                -1,
                false);
        }

        public static void SignalRunningInstance()
        {
            try
            {
                using var eventWaitHandle = EventWaitHandle.OpenExisting(EventName);
                eventWaitHandle.Set();
            }
            catch
            {
                // If event wait handle does not exist or signal fails, fail-safe
            }
        }

        public void Dispose()
        {
            _registeredWaitHandle?.Unregister(null);
            _eventWaitHandle?.Dispose();
        }
    }
}
