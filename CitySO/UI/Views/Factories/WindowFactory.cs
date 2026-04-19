using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace CitySO.Views.Factories;

public class WindowFactory(
    IServiceProvider serviceProvider) 
    : IWindowFactory
{
    public T CreateWindow<T>() where T : Window
    {
        return serviceProvider.GetRequiredService<T>();
    }
}