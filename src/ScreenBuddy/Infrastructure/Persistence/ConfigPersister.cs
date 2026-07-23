using System;
using System.IO;
using System.Text.Json;
using ScreenBuddy.Application.Services;
using ScreenBuddy.Domain.Models;

namespace ScreenBuddy.Infrastructure.Persistence
{
    /// <summary>
    /// Implements atomic JSON configuration persistence in %APPDATA%\ScreenBuddy\config.json.
    /// </summary>
    public sealed class ConfigPersister : IConfigPersister
    {
        private readonly string _configFilePath;
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public ConfigPersister(string? customFilePath = null)
        {
            if (!string.IsNullOrEmpty(customFilePath))
            {
                _configFilePath = customFilePath;
            }
            else
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ScreenBuddy");

                Directory.CreateDirectory(dir);
                _configFilePath = Path.Combine(dir, "config.json");
            }
        }

        public AppSettings LoadConfig()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    return AppSettings.Default;
                }

                string json = File.ReadAllText(_configFilePath);
                AppSettings? deserialized = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                return deserialized ?? AppSettings.Default;
            }
            catch
            {
                return AppSettings.Default;
            }
        }

        public void SaveConfig(AppSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            string dir = Path.GetDirectoryName(_configFilePath)!;
            Directory.CreateDirectory(dir);

            string tempPath = _configFilePath + ".tmp";
            string json = JsonSerializer.Serialize(settings, JsonOptions);

            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _configFilePath, overwrite: true);
        }
    }
}
