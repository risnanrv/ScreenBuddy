using System;
using System.IO;
using System.Text.Json;
using ScreenBuddy.Domain.Messages;

namespace ScreenBuddy.Infrastructure.Persistence
{
    public sealed class MessageIndexPersister : IMessageIndexPersister
    {
        private readonly string _filePath;

        private sealed record IndexData(int LastIndex);

        public MessageIndexPersister(string? customFilePath = null)
        {
            if (!string.IsNullOrEmpty(customFilePath))
            {
                _filePath = customFilePath;
            }
            else
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ScreenBuddy");

                Directory.CreateDirectory(dir);
                _filePath = Path.Combine(dir, "message-index.json");
            }
        }

        public int LoadLastIndex()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return 0;
                }

                string json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<IndexData>(json);
                return data?.LastIndex ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public void SaveLastIndex(int index)
        {
            try
            {
                string dir = Path.GetDirectoryName(_filePath)!;
                Directory.CreateDirectory(dir);

                string tempPath = _filePath + ".tmp";
                string json = JsonSerializer.Serialize(new IndexData(index));

                File.WriteAllText(tempPath, json);
                File.Move(tempPath, _filePath, overwrite: true);
            }
            catch
            {
                // Fail-safe
            }
        }
    }
}
