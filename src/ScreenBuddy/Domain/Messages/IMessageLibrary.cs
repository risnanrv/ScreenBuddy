using ScreenBuddy.Domain.Models;

namespace ScreenBuddy.Domain.Messages
{
    /// <summary>
    /// Contract for providing sequential break messages from the message library.
    /// </summary>
    public interface IMessageLibrary
    {
        BreakMessage GetNextMessage();
    }
}
