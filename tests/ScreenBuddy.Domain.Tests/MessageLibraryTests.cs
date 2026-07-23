using FluentAssertions;
using NSubstitute;
using ScreenBuddy.Domain.Messages;
using Xunit;

namespace ScreenBuddy.Domain.Tests
{
    public class MessageLibraryTests
    {
        [Fact]
        public void GetNextMessage_RotatesSequentiallyAndWraps()
        {
            var messages = new[] { "Msg 0", "Msg 1", "Msg 2" };
            var persister = Substitute.For<IMessageIndexPersister>();
            persister.LoadLastIndex().Returns(0);

            var library = new MessageLibrary(messages, persister);

            var m0 = library.GetNextMessage();
            m0.Text.Should().Be("Msg 0");
            m0.Index.Should().Be(0);

            var m1 = library.GetNextMessage();
            m1.Text.Should().Be("Msg 1");
            m1.Index.Should().Be(1);

            var m2 = library.GetNextMessage();
            m2.Text.Should().Be("Msg 2");
            m2.Index.Should().Be(2);

            var m3 = library.GetNextMessage();
            m3.Text.Should().Be("Msg 0"); // Wrapped around
            m3.Index.Should().Be(0);

            persister.Received(4).SaveLastIndex(Arg.Any<int>());
        }

        [Fact]
        public void GetNextMessage_RespectsLoadedIndex()
        {
            var messages = new[] { "Msg 0", "Msg 1", "Msg 2" };
            var persister = Substitute.For<IMessageIndexPersister>();
            persister.LoadLastIndex().Returns(2);

            var library = new MessageLibrary(messages, persister);

            var msg = library.GetNextMessage();
            msg.Text.Should().Be("Msg 2");
            msg.Index.Should().Be(2);
        }
    }
}
