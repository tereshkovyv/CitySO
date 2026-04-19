using CitySO.Services.GoogleSheetsServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CitySO.Services.GoogleSheetsServices.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGoogleSheetsServices(this IServiceCollection services)
    {
        services.AddSingleton<IGoogleSheetsService, GoogleSheetsService>();
        services.AddSingleton<IGoogleSheetsTasksRepository, GoogleSheetsTasksRepository>();
        services.AddSingleton<IGoogleSheetsUsersRepository, GoogleSheetsUsersRepository>();
        services.AddSingleton<IGoogleSheetsAnswersRepository, GoogleSheetsAnswersRepository>();
        return services;
    }
}