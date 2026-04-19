using System.Windows;

namespace CitySO.Views.Factories;

public interface IWindowFactory
{
    T CreateWindow<T>() where T : Window;
}