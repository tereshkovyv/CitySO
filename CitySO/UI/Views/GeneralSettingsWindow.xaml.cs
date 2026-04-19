using CitySO.UI.ViewModels;

namespace CitySO.UI.Views;

public partial class GeneralSettingsWindow
{
    private LoadingWindow? _loadingWindow;

    public GeneralSettingsWindow(GeneralSettingsWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseAction = Close;
        viewModel.ShowLoadingAction = ShowLoading;
        viewModel.HideLoadingAction = HideLoading;
    }

    private void ShowLoading()
    {
        if (_loadingWindow == null)
        {
            _loadingWindow = new LoadingWindow();
            _loadingWindow.Owner = this;
        }
        _loadingWindow.Show();
    }

    private void HideLoading()
    {
        _loadingWindow?.Close();
        _loadingWindow = null;
    }
}