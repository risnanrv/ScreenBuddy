using ScreenBuddy.Domain.Models;

namespace ScreenBuddy.Application.Services
{
    /// <summary>
    /// Contract for persisting and reading application configuration settings from disk.
    /// </summary>
    public interface IConfigPersister
    {
        AppSettings LoadConfig();
        void SaveConfig(AppSettings settings);
    }
}
