using CitySO.Configuration;
using CitySO.Services;
using CitySO.UI;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CitySO.UI.ViewModels;

public partial class GeneralSettingsWindowViewModel : ViewModelBase
{
    private readonly IConfigurationService _configurationService;

    private string _competitionName;
    private string _vkApiKey;
    private ulong _vkGroupId;
    private string _googleSpreadSheetId;
    private bool _showAnswersImmediately;

    public GeneralSettingsWindowViewModel(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
        var generalOptions = _configurationService.GetGeneralOptions();
        _competitionName = generalOptions.Competition.Name;
        _vkApiKey = generalOptions.VkApiKey;
        _vkGroupId = generalOptions.VkGroupId;
        _googleSpreadSheetId = generalOptions.GoogleSpreadSheetId;
        _showAnswersImmediately = generalOptions.ShowAnswersImmediately;
    }

    public string CompetitionName
    {
        get => _competitionName;
        set => SetField(ref _competitionName, value);
    }

    public string VkApiKey
    {
        get => _vkApiKey;
        set => SetField(ref _vkApiKey, value);
    }

    public ulong VkGroupId
    {
        get => _vkGroupId;
        set => SetField(ref _vkGroupId, value);
    }

    public string GoogleSpreadSheetId
    {
        get => _googleSpreadSheetId;
        set => SetField(ref _googleSpreadSheetId, value);
    }

    public bool ShowAnswersImmediately
    {
        get => _showAnswersImmediately;
        set => SetField(ref _showAnswersImmediately, value);
    }

    [RelayCommand]
    private void SaveChanges()
    {
        ShowLoadingAction?.Invoke();
        try
        {
            Task.Delay(2000);
            SaveSettings();
        }
        finally
        {
            HideLoadingAction?.Invoke();
            CloseAction?.Invoke();
        }
    }
    
    [RelayCommand]
    private void DiscardChanges()
    {
        CloseAction?.Invoke();
    }

    private void SaveSettings()
    {
        _configurationService.SaveGeneralOptions(_configurationService.GetGeneralOptions() with
        {
            Competition = _configurationService.GetGeneralOptions().Competition with { Name = _competitionName },
            GoogleSpreadSheetId = _googleSpreadSheetId,
            VkGroupId = _vkGroupId,
            VkApiKey = _vkApiKey,
            ShowAnswersImmediately = _showAnswersImmediately
        });
        WeakReferenceMessenger.Default.Send(new ConfigurationChangedMessage());
    }
}