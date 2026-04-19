using CitySO.Services;
using CitySO.Services.VkServices.Interfaces;
using CitySO.VkServices;
using Microsoft.Extensions.Hosting;
using VkNet.Enums.StringEnums;
using VkNet.Model;
using ILogger = CitySO.Services.ILogger;

namespace CitySO.BackgroundServices;

public class VkMessagesBackGroundService(
    IVkLongPollingService vkLongPollingService,
    IVkMessagesService vkMessagesService,
    IMessagesService globalMessagesService, 
    ILogger logger) 
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var newMessages = vkLongPollingService.GetNewMessages();
            foreach (var message in newMessages)
            {
                var answer = await globalMessagesService.Handle(message);
                await vkMessagesService.SendMessage(answer);
            }
        }
    }
}