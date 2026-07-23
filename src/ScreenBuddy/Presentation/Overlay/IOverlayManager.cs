using System;
using ScreenBuddy.Domain.Models;

namespace ScreenBuddy.Presentation.Overlay
{
    public interface IOverlayManager : IDisposable
    {
        void ShowBreak(BreakMessage message);
        void HideBreak();
        void UpdateRemainingTime(int remainingSeconds);
    }
}
