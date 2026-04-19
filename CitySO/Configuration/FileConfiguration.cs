using System.IO;

namespace CitySO.Configuration;

public static class FileConfiguration
{
    public static string GoogleSheetsKeyPath => Path.Combine(GetAppDataFolder(), "googleSheetsKey.json");

    public static string GoogleSheetsTokenPath => Path.Combine(GetAppDataFolder(), "googleSheetsToken.json");

    private static string GetAppDataFolder()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appDataPath, "CitySO");
    }
}