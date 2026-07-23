namespace ScreenBuddy.Domain.Messages
{
    /// <summary>
    /// Contract for persisting and retrieving the last displayed break message index.
    /// </summary>
    public interface IMessageIndexPersister
    {
        int LoadLastIndex();
        void SaveLastIndex(int index);
    }
}
