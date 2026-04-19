using CitySO.Configuration;
using CitySO.Exceptions;
using CitySO.Services.VkServices.Interfaces;
using VkNet;
using VkNet.Enums.StringEnums;
using VkNet.Model;

namespace CitySO.Services.VkServices;

public class VkApiService : IVkApiService
{
    private readonly ILogger _logger;
    private readonly IConfigurationService _configurationService;
    private VkApi? _vkApi;
    
    public VkApiService(IConfigurationService configurationService, ILogger logger)
    {
        _configurationService = configurationService;
        _logger = logger;
        _vkApi = CreateApi();
    }
    

    public VkApi GetApi() => _vkApi;

    public async Task<bool> IsHealthy()
    {
        try
        {
            await _vkApi.Messages.GetConversationsAsync(new GetConversationsParams() { Filter = GetConversationFilter.Unread });
            return true;
        }
        catch(Exception ex)
        {
            throw new ServiceUnhealthyException(ex.Message);
        }
    }

    public async Task<long> GetUserIdByUrl(string url)
    {
        var nickname = new Uri(url)?.Segments?.Last()?.TrimEnd('/');
        var apiResult = await _vkApi.Utils.ResolveScreenNameAsync(nickname);
        return apiResult.Id.Value;
    }

    private VkApi CreateApi()
    {
        try
        {
            var api = new VkApi();
            api.Authorize(new ApiAuthParams { AccessToken = _configurationService.GetGeneralOptions().VkApiKey });
            return api;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }
}