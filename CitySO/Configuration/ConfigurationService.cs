using System.IO;
using System.Text.Json;
using CitySO.Configuration.Models;

namespace CitySO.Configuration;

public class ConfigurationService : IConfigurationService
{
    private readonly string _configPath;
    private readonly string _configDirectory;

    private AppConfiguration _appConfiguration;

    public ConfigurationService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "CitySO");
        var t = Directory.CreateDirectory(appFolder);
        _configDirectory = appFolder;
        
        _configPath  = Path.Combine(appFolder, "settings.json");
        try
        {
            if (!File.Exists(_configPath))
            {
                CreateDefaultSettings();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
            {
                CreateDefaultSettings();
            }
        }

        UpdateOptions();
    }
    
    private void CreateDefaultSettings()
    {
        var defaultSettings = new AppConfiguration();

        try
        {
            var json = JsonSerializer.Serialize(defaultSettings,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка создания файла настроек: {ex.Message}");
            throw;
        }
    }

    private void UpdateOptions()
    {
        try
        {
            var json = File.ReadAllText(_configPath);
            _appConfiguration = JsonSerializer.Deserialize<AppConfiguration>(json) ?? new AppConfiguration();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
        }
    }

    public AppConfiguration GetGeneralOptions() => _appConfiguration;

    public void SaveGeneralOptions(AppConfiguration options)
    {
        try
        {
            var json = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
        }

        UpdateOptions();
    }
}