using System;

namespace ScreenBuddy.Presentation.Tray
{
    public interface ITrayService : IDisposable
    {
        void Initialize();
    }
}
