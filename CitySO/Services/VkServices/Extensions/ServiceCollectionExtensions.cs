using CitySO.Services.VkServices.Interfaces;
using CitySO.VkServices;
using Microsoft.Extensions.DependencyInjection;

namespace CitySO.Services.VkServices.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVkServices(this IServiceCollection services)
    {
        services.AddSingleton<IVkApiService, VkApiService>();
        services.AddSingleton<IVkMessagesService, VkMessagesService>();
        services.AddSingleton<IVkLongPollingService, VkLongPollingService>();
        return services;
    }
}