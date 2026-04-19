using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CitySO.UI.ViewModels;

public class ViewModelBase : ObservableObject
{
    public Action? CloseAction { get; set; }
    public new event PropertyChangedEventHandler? PropertyChanged;
    
    public Action? ShowLoadingAction { get; set; }
    public Action? HideLoadingAction { get; set; }

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}