namespace ScreenBuddy.Domain.Models
{
    /// <summary>
    /// Value object representing a break message displayed on the fullscreen overlay.
    /// </summary>
    public sealed record BreakMessage(string Text, int Index);
}
