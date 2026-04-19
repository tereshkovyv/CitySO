using CitySO.BackgroundServices;
using CitySO.Configuration;
using CitySO.Logging;
using CitySO.Services;
using CitySO.Services.GoogleSheetsServices.Extensions;
using CitySO.Services.Interfaces;
using CitySO.Services.VkServices.Extensions;
using CitySO.UI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CitySO;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<App>();
                services.AddUi();
                services.AddVkServices();
                services.AddGoogleSheetsServices();
                
                services.AddSingleton<IConfigurationService, ConfigurationService>();
                services.AddSingleton<ILogger, LoggerService>();
                
                services.AddTransient<ICategoriesService, CategoriesService>();
                services.AddSingleton<IAnswersService, AnswersService>();
                services.AddSingleton<IMessagesService, MessagesService>();
                services.AddSingleton<IUsersService, UsersService>();
                services.AddSingleton<ITasksService, TasksService>();
                
                services.AddHostedService<VkMessagesBackGroundService>();
            })
            .Build();
        
        _ = host.StartAsync();

        var app = host.Services.GetRequiredService<App>();
        app.Run();

        host.Dispose();
    }
}