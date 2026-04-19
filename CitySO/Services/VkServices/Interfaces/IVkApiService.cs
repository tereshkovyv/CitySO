using VkNet;

namespace CitySO.Services.VkServices.Interfaces;

public interface IVkApiService
{
    public VkApi GetApi();
    public Task<bool> IsHealthy();
    public Task<long> GetUserIdByUrl(string url);
}