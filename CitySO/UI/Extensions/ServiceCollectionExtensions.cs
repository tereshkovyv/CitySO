using CitySO.UI.ViewModels;
using CitySO.UI.Views;
using CitySO.Views;
using CitySO.Views.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace CitySO.UI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUi(this IServiceCollection services)
    {
        services.AddSingleton<IWindowFactory, WindowFactory>();
        services.AddTransient<MainWindow>();
        services.AddTransient<GoogleSheetsSettingsWindow>();
        services.AddTransient<GeneralSettingsWindow>();
        services.AddTransient<AddCategoryWindow>();
        services.AddTransient<ChangeStatusWindow>();
        services.AddTransient<LoadingWindow>();
        
        services.AddTransient<GeneralSettingsWindowViewModel>();
        services.AddTransient<GoogleSheetsSettingsWindowViewModel>();
        services.AddTransient<AddCategoryWindowViewModel>();
        services.AddTransient<ChangeStatusWindowViewModel>();
        services.AddTransient<MainWindowViewModel>();

        return services;
    }
}