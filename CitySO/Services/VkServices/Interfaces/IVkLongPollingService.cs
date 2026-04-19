using CitySO.Models;

namespace CitySO.Services.VkServices.Interfaces;

public interface IVkLongPollingService
{
    public List<VkIncomingMessage> GetNewMessages();
}