using System.Windows;
using CitySO.Services.GoogleSheetsServices.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace CitySO.UI.ViewModels;

public partial class GoogleSheetsSettingsWindowViewModel(
    IGoogleSheetsService googleSheetsService)
    : ViewModelBase
{
    private string _jsonKey = string.Empty;
    private string _loginStatus = "Не подключено";

    public string JsonKey
    {
        get => _jsonKey;
        set => SetField(ref _jsonKey, value);
    }

    public string LoginStatus
    {
        get => _loginStatus;
        private set => SetField(ref _loginStatus, value);
    }
    
    [RelayCommand]
    private void Authorize()
    {
        if (string.IsNullOrWhiteSpace(JsonKey))
        {
            LoginStatus = "Ошибка: JSON-ключ не заполнен.";
            return;
        }
        try
        {
            googleSheetsService.Authorize(JsonKey);

            MessageBox.Show(
                "Вы успешно вошли в Google.", 
                "Вход", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
    
            CloseAction?.Invoke();
        }
        catch (Exception ex)
        {
            LoginStatus = $"Ошибка входа: {ex.Message}";
        }
    }

    [RelayCommand]
    private void LogOut()
    {
        googleSheetsService.LogOut();
        
        MessageBox.Show(
            "Вы успешно вышли из Google.", 
            "Выход", 
            MessageBoxButton.OK, 
            MessageBoxImage.Information);
    
        CloseAction?.Invoke();
    }
}
