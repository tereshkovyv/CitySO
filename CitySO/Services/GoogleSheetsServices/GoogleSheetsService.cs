using System.IO;
using CitySO.Configuration;
using CitySO.Exceptions;
using CitySO.Services.GoogleSheetsServices.Interfaces;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;

namespace CitySO.Services.GoogleSheetsServices;

public class GoogleSheetsService : IGoogleSheetsService
{
    private const string UserCredentialKey = "user";
    private readonly ILogger _logger;
    private readonly IConfigurationService _configurationService;

    private SheetsService? _service;

    public GoogleSheetsService(ILogger logger, IConfigurationService configurationService)
    {
        _logger = logger;
        _configurationService = configurationService;

        InitializeService();
    }

    private void InitializeService()
    {
        try
        {
            _service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = LoadCredentialsFromFile(),
                ApplicationName = "CitySO application"
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }

    public SheetsService GetService() => _service ?? throw new ServiceUnhealthyException("Google sheets service does not initialized");

    public async Task<bool> IsHealthy()
    {
        try
        {
            if (_service is null)
                throw new ServiceUnhealthyException("Google sheets service does not initialized");
            var o = _configurationService.GetGeneralOptions().GoogleSpreadSheetId;
            var answer = await _service.Spreadsheets.Get(_configurationService.GetGeneralOptions().GoogleSpreadSheetId).ExecuteAsync();
            return answer != null;
        }
        catch(GoogleApiException ex)
        {
            throw new ServiceUnhealthyException(ex.Message);
        }
    }

    public void Authorize(string jsonKey)
    {
        File.WriteAllText(FileConfiguration.GoogleSheetsKeyPath, jsonKey);
        InitializeService();
    }

    public void LogOut()
    {
        if (Directory.Exists(FileConfiguration.GoogleSheetsTokenPath))
        {
            Directory.Delete(FileConfiguration.GoogleSheetsTokenPath, true);
            Console.WriteLine("Все токены удалены");
        }
        _logger.LogInfo("Logged out successfully");
    }

    private UserCredential LoadCredentialsFromFile()
    {
        try
        {
            var store = new FileDataStore(FileConfiguration.GoogleSheetsTokenPath, true);
            var task = store.GetAsync<UserCredential>(UserCredentialKey);
            task.Wait();
            var credential = task.Result ?? AuthorizeInternal();

            if (credential.Token.IsStale)
            {
                _logger.LogInfo("Токен google expires, refreshing...");
                var refreshTask = credential.RefreshTokenAsync(CancellationToken.None);
                refreshTask.Wait();
            }
            
            _logger.LogInfo("Credentials loaded successfully");
            return credential;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while loading credentials: {ex.Message}");
            throw;
        }
    }

    private UserCredential AuthorizeInternal()
    {
        return GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.FromFile(FileConfiguration.GoogleSheetsKeyPath).Secrets,
            new[] { SheetsService.Scope.Spreadsheets },
            UserCredentialKey,
            CancellationToken.None,
            new FileDataStore(FileConfiguration.GoogleSheetsTokenPath, true)
        ).GetAwaiter().GetResult();
    }
}