using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ScreenBuddy.Domain.Models;

namespace ScreenBuddy.Domain.Messages
{
    /// <summary>
    /// Implements sequential rotation through the break message library.
    /// </summary>
    public sealed class MessageLibrary : IMessageLibrary
    {
        private readonly ReadOnlyCollection<string> _messages;
        private readonly IMessageIndexPersister? _indexPersister;
        private int _currentIndex;

        public MessageLibrary(IEnumerable<string> messages, IMessageIndexPersister? indexPersister = null)
        {
            ArgumentNullException.ThrowIfNull(messages);

            var list = new List<string>(messages);
            if (list.Count == 0)
            {
                list.Add("Rest is not the opposite of productivity. It is its fuel.");
            }

            _messages = new ReadOnlyCollection<string>(list);
            _indexPersister = indexPersister;
            _currentIndex = _indexPersister?.LoadLastIndex() ?? 0;

            if (_currentIndex < 0 || _currentIndex >= _messages.Count)
            {
                _currentIndex = 0;
            }
        }

        public BreakMessage GetNextMessage()
        {
            int indexToReturn = _currentIndex;
            string text = _messages[indexToReturn];

            _currentIndex = (_currentIndex + 1) % _messages.Count;
            _indexPersister?.SaveLastIndex(_currentIndex);

            return new BreakMessage(text, indexToReturn);
        }
    }
}
