using System.Windows;
using CitySO.UI.ViewModels;

namespace CitySO.UI.Views;

public partial class ChangeStatusWindow
{
    private LoadingWindow? _loadingWindow;

    public ChangeStatusWindow(ChangeStatusWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseAction = Close;
        viewModel.ShowLoadingAction = ShowLoading;
        viewModel.HideLoadingAction = HideLoading;
    }

    public void ShowLoading()
    {
        if (_loadingWindow == null)
        {
            _loadingWindow = new LoadingWindow();
            _loadingWindow.Owner = this;
        }
        _loadingWindow.Show();
    }

    public void HideLoading()
    {
        _loadingWindow?.Close();
        _loadingWindow = null;
    }
}