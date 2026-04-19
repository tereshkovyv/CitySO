using CitySO.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace CitySO.UI.ViewModels;

public partial class AddCategoryWindowViewModel(ICategoriesService categoriesService) : ViewModelBase
{
    private string _categoryName = string.Empty;
    private int _checkpointsCount = 2;

    public string CategoryName
    {
        get => _categoryName;
        set => SetField(ref _categoryName, value);
    }

    public int CheckpointsCount
    {
        get => _checkpointsCount;
        set => SetField(ref _checkpointsCount, value);
    }

    public Action? ShowLoadingAction { get; set; }
    public Action? HideLoadingAction { get; set; }
    
    [RelayCommand]
    private async void SaveChanges()
    {
        ShowLoadingAction?.Invoke();
        try
        {
            await categoriesService.AddCategory(CategoryName, CheckpointsCount);
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
}