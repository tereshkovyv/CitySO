using CitySO.Configuration;
using CitySO.Configuration.Models;
using CitySO.Extensions;
using CitySO.Services.Interfaces;
using CitySO.UI;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CitySO.UI.ViewModels;

public partial class ChangeStatusWindowViewModel : ViewModelBase
{
    private readonly IConfigurationService _configurationService;
    private readonly ITasksService _tasksService;
    private readonly IUsersService _usersService;
    
    public ChangeStatusWindowViewModel(
        IConfigurationService configurationService, 
        IUsersService usersService, 
        ITasksService tasksService)
    {
        _configurationService = configurationService;
        _usersService = usersService;
        _tasksService = tasksService;
        _selectedStatus = (int)_configurationService.GetGeneralOptions().Competition.Status;
    }
    
    private int _selectedStatus;

    public int SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            SetField(ref _selectedStatus, value);
            OnPropertyChanged(nameof(CurrentStatusName));
            OnPropertyChanged(nameof(CurrentStatusDescription));
        }
    }

    public string CurrentStatusName => GetStatus(_selectedStatus).Name();

    public string CurrentStatusDescription => GetStatus(_selectedStatus).Description();

    public Action? CloseAction { get; set; }
    public Action? ShowLoadingAction { get; set; }
    public Action? HideLoadingAction { get; set; }

    public CompetitionStatus GetSelectedCompetitionStatus()
    {
        return (CompetitionStatus)_selectedStatus;
    }

    public void SetInitialStatus(CompetitionStatus status)
    {
        SelectedStatus = (int)status;
    }
    
    [RelayCommand]
    private async void SaveChanges()
    {
        ShowLoadingAction?.Invoke();
        try
        {
            if (GetStatus(_selectedStatus) is CompetitionStatus.InProcess)
            {
                await _tasksService.LoadFromGoogleSheets();
                await _usersService.LoadFromGoogleSheets();
            }
            var currentOptions = _configurationService.GetGeneralOptions();
            var newOptions = currentOptions with
            {
                Competition = currentOptions.Competition with { Status = GetStatus(_selectedStatus) }
            };

            _configurationService.SaveGeneralOptions(newOptions);
            WeakReferenceMessenger.Default.Send(new ConfigurationChangedMessage());
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

    private CompetitionStatus GetStatus(int status)
    {
        return status switch
        {
            0 => CompetitionStatus.Preparing,
            1 => CompetitionStatus.Ready,
            2 => CompetitionStatus.InProcess,
            3 => CompetitionStatus.Finished,
            _ => throw new ArgumentException()
        };
    }
}