using System;

namespace ScreenBuddy.Presentation.Main
{
    /// <summary>
    /// Contract for managing the lifecycle, visibility, and foreground activation of MainWindow.
    /// Acts as the single authoritative window activation service.
    /// </summary>
    public interface IMainWindowService : IDisposable
    {
        void Show();
        void Hide();
        void Restore();
        void BringToForeground();
        bool IsVisible { get; }
    }
}
