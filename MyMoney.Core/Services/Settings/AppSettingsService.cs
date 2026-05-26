using MyMoney.Core.Database;

namespace MyMoney.Core.Services.Settings;

public sealed class AppSettingsService
{
    private readonly IDatabaseManager _databaseManager;

    public AppSettingsService(IDatabaseManager databaseManager)
    {
        _databaseManager = databaseManager;
    }

    public Dictionary<string, string> GetSettings(string settingsKey)
    {
        return _databaseManager.GetSettingsDictionary(settingsKey);
    }

    public void SaveSettings(string settingsKey, Dictionary<string, string> settings)
    {
        _databaseManager.WriteSettingsDictionary(settingsKey, settings);
    }
}
