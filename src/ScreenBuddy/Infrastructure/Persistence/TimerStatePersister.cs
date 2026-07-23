using System;
using System.IO;
using System.Text.Json;
using ScreenBuddy.Domain.Models;

namespace ScreenBuddy.Infrastructure.Persistence
{
    public interface ITimerStatePersister
    {
        void Write(TimerSnapshot snapshot);
        TimerSnapshot? Read();
        void Delete();
    }

    /// <summary>
    /// Implements atomic snapshot state persistence for crash recovery.
    /// </summary>
    public sealed class TimerStatePersister : ITimerStatePersister
    {
        private readonly string _stateFilePath;

        public TimerStatePersister(string? customFilePath = null)
        {
            if (!string.IsNullOrEmpty(customFilePath))
            {
                _stateFilePath = customFilePath;
            }
            else
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ScreenBuddy");

                Directory.CreateDirectory(dir);
                _stateFilePath = Path.Combine(dir, "timer-state.json");
            }
        }

        public void Write(TimerSnapshot snapshot)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            try
            {
                string dir = Path.GetDirectoryName(_stateFilePath)!;
                Directory.CreateDirectory(dir);

                string tempPath = _stateFilePath + ".tmp";
                string json = JsonSerializer.Serialize(snapshot);

                File.WriteAllText(tempPath, json);
                File.Move(tempPath, _stateFilePath, overwrite: true);
            }
            catch
            {
                // Fail-safe
            }
        }

        public TimerSnapshot? Read()
        {
            try
            {
                if (!File.Exists(_stateFilePath))
                {
                    return null;
                }

                string json = File.ReadAllText(_stateFilePath);
                return JsonSerializer.Deserialize<TimerSnapshot>(json);
            }
            catch
            {
                return null;
            }
        }

        public void Delete()
        {
            try
            {
                if (File.Exists(_stateFilePath))
                {
                    File.Delete(_stateFilePath);
                }
            }
            catch
            {
                // Fail-safe
            }
        }
    }
}
