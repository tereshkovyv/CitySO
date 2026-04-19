using CitySO.Configuration;
using CitySO.Models;
using CitySO.Services;
using CitySO.Services.VkServices.Interfaces;
using VkNet.Enums.StringEnums;
using VkNet.Model;

namespace CitySO.VkServices;

public class VkLongPollingService : IVkLongPollingService
{
    private LongPollServerResponse _server;
    private readonly IVkApiService _apiService;
    private readonly IConfigurationService _configurationService;

    public VkLongPollingService(IVkApiService vkApiService, IConfigurationService configurationService, IVkApiService apiService)
    {
        _configurationService = configurationService;
        _apiService = apiService;
        _server = vkApiService.GetApi().Groups.GetLongPollServer(configurationService.GetGeneralOptions().VkGroupId);
    }

    public List<VkIncomingMessage> GetNewMessages()
    {
        // try
        // {
            var poll = _apiService.GetApi().Groups.GetBotsLongPollHistory(
                new BotsLongPollHistoryParams
                {
                    Server = _server.Server,
                    Ts = _server.Ts,
                    Key = _server.Key,
                    Wait = 25
                });

            _server.Ts = poll.Ts;

            if (poll.Updates == null) return [];

            return (from update in poll.Updates
                where update.Type.Value == GroupUpdateType.MessageNew
                select ((MessageNew)update.Instance)
                into messageUpdate
                select new VkIncomingMessage()
                    { PeerId = messageUpdate.Message.PeerId, Text = messageUpdate.Message.Text }).ToList();
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError("Error in poll loop");
        //     // При ошибке получаем новый сервер
        //     _server = _apiService.GetApi().Groups
        //         .GetLongPollServer(_configurationService.GetGeneralOptions().VkGroupId);
        //     await Task.Delay(1000, stoppingToken); // Небольшая задержка перед повтором
        // }
    }
}