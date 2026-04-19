using CitySO.Models;
using VkNet.Model;

namespace CitySO.VkServices;

public interface IVkMessagesService
{
    public Task SendMessage(VkOutgoingMessage message);
    public Task<GetConversationsResult> GetUnreadMessages();
}