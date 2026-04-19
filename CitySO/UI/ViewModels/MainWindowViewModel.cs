using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Win32;
using CitySO.Configuration;
using CitySO.Configuration.Models;
using CitySO.Exceptions;
using CitySO.Extensions;
using CitySO.Models;
using CitySO.Services;
using CitySO.Services.GoogleSheetsServices.Interfaces;
using CitySO.Services.Interfaces;
using CitySO.Services.VkServices.Interfaces;
using CitySO.UI;
using CitySO.UI.Views;
using CitySO.Views.Factories;
using CitySO.VkServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using VkNet.Model;

namespace CitySO.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly IWindowFactory _windowFactory;
    private readonly ICategoriesService _categoriesService;
    private readonly IGoogleSheetsService _googleSheetsService;
    private readonly ITasksService _tasksService;
    private readonly IUsersService _usersService;
    private readonly IConfigurationService _configurationService;
    private readonly IVkApiService _vkApiService;
    private readonly IVkMessagesService _vkMessagesService;
    private readonly IMessagesService _globalMessagesService;

    private LoadingWindow? _loadingWindow;

    private string _competitionName = string.Empty;
    private string _competitionStatus = string.Empty;
    private bool _isVkConnected;
    private string _vkConnectionStatus = string.Empty;
    private bool _isGoogleConnected;
    private string _googleConnectionStatus = string.Empty;
    private bool _isBotEnabled;
    private int _processedMessages;
    private int _queuedMessages;
    private ObservableCollection<CategoryItem> _categories = [];


    public MainWindowViewModel(
        ILogger logger, 
        IWindowFactory windowFactory, 
        ICategoriesService categoriesService, 
        IGoogleSheetsService googleSheetsService,
        IConfigurationService configurationService, 
        IVkApiService vkApiService, 
        IVkMessagesService vkMessagesService, 
        ITasksService tasksService, 
        IUsersService usersService,
        IMessagesService globalMessagesService)
    {
        _logger = logger;
        _windowFactory = windowFactory;
        _categoriesService = categoriesService;
        _googleSheetsService = googleSheetsService;
        _configurationService = configurationService;
        _vkApiService = vkApiService;
        _vkMessagesService = vkMessagesService;
        _tasksService = tasksService;
        _usersService = usersService;
        _globalMessagesService = globalMessagesService;

        UpdateGeneralOptionsText(_configurationService.GetGeneralOptions());
        WeakReferenceMessenger.Default.Register<ConfigurationChangedMessage>(this, (r, m) => UpdateGeneralOptionsText(_configurationService.GetGeneralOptions()));
        WeakReferenceMessenger.Default.Register<MessageProcessedMessage>(this, (r, m) => ProcessedMessages++);
        
        IsVkConnected = false;
        IsGoogleConnected = false;
        IsBotEnabled = false;
        ProcessedMessages = -1;
        QueuedMessages = -1;
        GoogleConnectionStatus = "Не подключено";
        VkConnectionStatus = "Не подключено";
    }

    public string CompetitionName
    {
        get => _competitionName;
        private set => SetProperty(ref _competitionName, value);
    }

    public string CompetitionStatus
    {
        get => _competitionStatus;
        private set => SetProperty(ref _competitionStatus, value);
    }

    public bool IsVkConnected
    {
        get => _isVkConnected;
        set => SetProperty(ref _isVkConnected, value);
    }

    public bool IsGoogleConnected
    {
        get => _isGoogleConnected;
        set => SetProperty(ref _isGoogleConnected, value);
    }

    public string GoogleConnectionStatus
    {
        get => _googleConnectionStatus;
        set => SetProperty(ref _googleConnectionStatus, value);
    }
    
    public string VkConnectionStatus
    {
        get => _vkConnectionStatus;
        set => SetProperty(ref _vkConnectionStatus, value);
    }

    public bool IsBotEnabled
    {
        get => _isBotEnabled;
        set => SetProperty(ref _isBotEnabled, value);
    }

    public int ProcessedMessages
    {
        get => _processedMessages;
        set => SetProperty(ref _processedMessages, value);
    }

    public int QueuedMessages
    {
        get => _queuedMessages;
        set => SetProperty(ref _queuedMessages, value);
    }

    public ObservableCollection<CategoryItem> Categories 
    { 
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    private void UpdateGeneralOptionsText(AppConfiguration options)
    {
        CompetitionName = options?.Competition?.Name is { } name
            ? $"{name}"
            : "Не задано";

        CompetitionStatus = $"Статус: {options?.Competition?.Status.Name()}";
    }

    private void ShowLoading()
    {
        if (_loadingWindow == null)
        {
            _loadingWindow = _windowFactory.CreateWindow<LoadingWindow>();
            if (System.Windows.Application.Current.MainWindow is { } mainWindow)
                _loadingWindow.Owner = mainWindow;
        }
        _loadingWindow.Show();
    }

    private void HideLoading()
    {
        _loadingWindow?.Close();
        _loadingWindow = null;
    }

    [RelayCommand]
    private void ImportConfig()
    {
        _logger.LogInfo("Import config requested");

        var dialog = new OpenFileDialog
        {
            Filter = "CitySO configuration (*.cityso)|*.cityso",
            DefaultExt = ".cityso",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
            return;

        ShowLoading();
        try
        {
            var json = File.ReadAllText(dialog.FileName);
            AppConfiguration newOptions;
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("GeneralOptions", out var generalOptionsElement))
                newOptions = JsonSerializer.Deserialize<AppConfiguration>(generalOptionsElement.GetRawText()) ?? new AppConfiguration();
            else
                newOptions = JsonSerializer.Deserialize<AppConfiguration>(json) ?? new AppConfiguration();

            _configurationService.SaveGeneralOptions(newOptions);
            UpdateGeneralOptionsText(_configurationService.GetGeneralOptions());
            MessageBox.Show("Конфигурация успешно импортирована.", "Импорт конфигурации", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to import configuration: {ex.Message}");
            MessageBox.Show($"Не удалось импортировать конфигурацию: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            HideLoading();
        }
    }

    [RelayCommand]
    private void ExportConfig()
    {
        _logger.LogInfo("Export config requested");

        var dialog = new SaveFileDialog
        {
            Filter = "CitySO configuration (*.cityso)|*.cityso",
            DefaultExt = ".cityso",
            AddExtension = true,
            FileName = $"{_configurationService.GetGeneralOptions().Competition.Name}.cityso"
        };

        if (dialog.ShowDialog() != true)
            return;

        ShowLoading();
        try
        {
            var options = _configurationService.GetGeneralOptions();
            var json = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dialog.FileName, json);
            MessageBox.Show("Конфигурация успешно экспортирована.", "Экспорт конфигурации", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to export configuration: {ex.Message}");
            MessageBox.Show($"Не удалось экспортировать конфигурацию: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            HideLoading();
        }
    }

    [RelayCommand]
    private void Exit()
    {
        _logger.LogInfo("Application exit requested");
        System.Windows.Application.Current.Shutdown();
    }

    [RelayCommand]
    private void AddCategory()
    {
        _logger.LogInfo("Add category window requested");
        var addCategoryWindow = _windowFactory.CreateWindow<AddCategoryWindow>();
        if (System.Windows.Application.Current.MainWindow is Window mainWindow)
            addCategoryWindow.Owner = mainWindow;

        if (addCategoryWindow.ShowDialog() != true) return;
        var viewModel = (AddCategoryWindowViewModel)addCategoryWindow.DataContext;
        _logger.LogInfo($"Category '{viewModel.CategoryName}' added");
    }

    [RelayCommand]
    private void ChangeStatus()
    {
        _logger.LogInfo("Change status window requested");
        var changeStatusWindow = _windowFactory.CreateWindow<ChangeStatusWindow>();
        if (System.Windows.Application.Current.MainWindow is Window mainWindow)
            changeStatusWindow.Owner = mainWindow;
        
        var viewModel = (ChangeStatusWindowViewModel)changeStatusWindow.DataContext;
        if (changeStatusWindow.ShowDialog() != true) return;
        var newStatus = viewModel.GetSelectedCompetitionStatus();
        _logger.LogInfo($"Status changed to '{newStatus}'");
    }

    [RelayCommand]
    private void About()
    {
        _logger.LogInfo("About dialog requested");

        var aboutWindow = new Window
        {
            Title = "О программе",
            Width = 400,
            Height = 300,
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x0F, 0x17, 0x2A)),
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = System.Windows.Application.Current.MainWindow,
            ResizeMode = ResizeMode.NoResize
        };

        var textBlock = new System.Windows.Controls.TextBlock
        {
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF1, 0xF5, 0xF9)),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(20)
        };

        var hyperlink1 = new Hyperlink
        {
            NavigateUri = new Uri("https://github.com/your-repo/CitySO"),
            Inlines = { new Run("Исходный код на GitHub") }
        };
        hyperlink1.RequestNavigate += (s, e) =>
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri)
                { UseShellExecute = true });

        var hyperlink2 = new Hyperlink
        {
            NavigateUri = new Uri("mailto:tereshkovyv@yandex.ru"),
            Inlines = { new Run("tereshkovyv@yandex.ru") }
        };
        hyperlink2.RequestNavigate += (s, e) =>
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri)
                { UseShellExecute = true });

        textBlock.Inlines.Add("CitySO v1.0\n\n");
        textBlock.Inlines.Add("Программа для управления квестами\n\n");
        textBlock.Inlines.Add("Лицензия: MIT\n\n");
        textBlock.Inlines.Add("Исходный код: ");
        textBlock.Inlines.Add(hyperlink1);
        textBlock.Inlines.Add("\n\nАвтор: Терешков Юрий: ");
        textBlock.Inlines.Add(hyperlink2);

        aboutWindow.Content = textBlock;
        aboutWindow.ShowDialog();
    }

    [RelayCommand]
    private void DownloadLogs()
    {
        _logger.LogInfo("Download logs requested");

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var logFilePath = Path.Combine(appDataPath, "CitySO", "logs.txt");

        if (!File.Exists(logFilePath))
        {
            MessageBox.Show("Файл логов не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Text files (*.txt)|*.txt",
            DefaultExt = ".txt",
            FileName = $"logs_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"
        };

        if (dialog.ShowDialog() == true)
        {
            ShowLoading();
            try
            {
                File.Copy(logFilePath, dialog.FileName, true);
                MessageBox.Show("Логи успешно сохранены.", "Скачать логи", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить логи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                HideLoading();
            }
        }
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var sw = _windowFactory.CreateWindow<GoogleSheetsSettingsWindow>();
        if (System.Windows.Application.Current.MainWindow is { } mainWindow)
            sw.Owner = mainWindow;
        sw.ShowDialog();
    }

    [RelayCommand]
    private void OpenGeneralSettings()
    {
        var generalSettingsWindow = _windowFactory.CreateWindow<GeneralSettingsWindow>();
        if (System.Windows.Application.Current.MainWindow is { } mainWindow)
            generalSettingsWindow.Owner = mainWindow;
        
        generalSettingsWindow.ShowDialog();
    }

    [RelayCommand]
    private void ResetGeneralOptions()
    {
        var result = MessageBox.Show(
            "Вы уверены, что хотите сбросить настройки? Все параметры будут очищены.",
            "Сброс настроек",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        ShowLoading();
        try
        {
            _configurationService.SaveGeneralOptions(new AppConfiguration());
            UpdateGeneralOptionsText(_configurationService.GetGeneralOptions());
            MessageBox.Show("Настройки успешно сброшены.", "Сброс настроек", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        finally
        {
            HideLoading();
        }
    }

    [RelayCommand]
    private async Task ProcessQueue()
    {
        _logger.LogInfo("Processing message queue requested");
        ShowLoading();
        try
        {
            var unreadConversations = await _vkMessagesService.GetUnreadMessages();
            int successCount = 0;
            int errorCount = 0;

            foreach (var conversation in unreadConversations.Items)
            {
                if ((conversation.Conversation.UnreadCount ?? 0) > 0)
                {
                    try
                    {
                        var history = await _vkApiService.GetApi().Messages.GetHistoryAsync(new MessagesGetHistoryParams
                        {
                            PeerId = conversation.Conversation.Peer.Id,
                            Count = conversation.Conversation.UnreadCount.Value
                        });

                        foreach (var message in history.Messages.Where(m => m.FromId != (long?)_configurationService.GetGeneralOptions().VkGroupId))
                        {
                            var incomingMessage = new VkIncomingMessage
                            {
                                Text = message.Text,
                                PeerId = message.PeerId.Value
                            };

                            var answer = await _globalMessagesService.Handle(incomingMessage);
                            await _vkMessagesService.SendMessage(answer);
                            successCount++;
                        }

                        await _vkApiService.GetApi().Messages.MarkAsReadAsync(conversation.Conversation.Peer.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing conversation {conversation.Conversation.Peer.Id}: {ex.Message}");
                        errorCount++;
                    }
                }
            }

            MessageBox.Show($"Успешно обработано: {successCount}\nОбработано с ошибкой: {errorCount}", "Результат обработки", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to process queue: {ex.Message}");
            MessageBox.Show($"Не удалось обработать очередь: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            HideLoading();
        }
    }

    [RelayCommand]
    private async Task RefreshCategories()
    {
        ShowLoading();
        try
        {
            await _tasksService.LoadFromGoogleSheets();
            await _usersService.LoadFromGoogleSheets();
            Categories = new ObservableCollection<CategoryItem>(await GetCategories());
            _logger.LogInfo("Categories refresh requested");
        }
        finally
        {
            HideLoading();
        }
    }

    [RelayCommand]
    private async Task RefreshConnections()
    {
        ShowLoading();
        try
        {
            try
            {
                IsGoogleConnected = await _googleSheetsService.IsHealthy();
                GoogleConnectionStatus = "Подключено";
            }
            catch (ServiceUnhealthyException ex)
            {
                GoogleConnectionStatus = ex.Message;
            }
            
            try
            {
                IsVkConnected = await _vkApiService.IsHealthy();
                VkConnectionStatus = "Подключено";
            }
            catch (ServiceUnhealthyException ex)
            {
                VkConnectionStatus = ex.Message;
            }

            _logger.LogInfo("Connections refresh requested");
        }
        finally
        {
            HideLoading();
        }
    }

    [RelayCommand]
    private async void RefreshBotStatus()
    {
        ShowLoading();
        try
        {
            var unreadMessagesCount = (await _vkMessagesService.GetUnreadMessages()).Count;
            QueuedMessages = (int)unreadMessagesCount;
            ProcessedMessages = _configurationService.GetGeneralOptions().Competition.ProcessedMessages;
            _logger.LogInfo("Bot status refresh requested");
        }
        finally
        {
            HideLoading();
        }
    }

    private async Task<List<CategoryItem>> GetCategories()
    {
        var categories = await _categoriesService.GetAll();
        return categories.Select(c => new CategoryItem
        {
            Name = c, TasksCount = _tasksService.GetAll().Count(t => t.Category == c),
            ParticipantsCount = _usersService.GetAll().Count(u => u.Category == c)
        }).ToList();
    }
}

public class CategoryItem
{
    public string Name { get; set; } = string.Empty;
    public int TasksCount { get; set; }
    public int ParticipantsCount { get; set; }
}