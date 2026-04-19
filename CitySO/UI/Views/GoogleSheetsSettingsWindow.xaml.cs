using CitySO.UI.ViewModels;

namespace CitySO.UI.Views;

public partial class GoogleSheetsSettingsWindow
{
    public GoogleSheetsSettingsWindow(GoogleSheetsSettingsWindowViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        viewModel.CloseAction = Close;
    }
}
