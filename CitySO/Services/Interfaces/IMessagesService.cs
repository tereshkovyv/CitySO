using CitySO.Models;

namespace CitySO.Services;

public interface IMessagesService
{
    public Task<VkOutgoingMessage> Handle(VkIncomingMessage incomingMessage);
}