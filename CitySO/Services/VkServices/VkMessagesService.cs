using CitySO.Exceptions;
using CitySO.Models;
using CitySO.Services;
using CitySO.Services.VkServices.Interfaces;
using CitySO.UI;
using CommunityToolkit.Mvvm.Messaging;
using VkNet.Enums.StringEnums;
using VkNet.Model;

namespace CitySO.VkServices;

public class VkMessagesService(IVkApiService vkApiService, ILogger logger) : IVkMessagesService
{
    public async Task SendMessage(VkOutgoingMessage message)
    {
        await vkApiService.GetApi().Messages.SendAsync(new MessagesSendParams()
        {
            Message = message.Text,
            PeerId = message.PeerId,
            RandomId = new Random().Next(1, int.MaxValue),
            Keyboard = new MessageKeyboard()
            {
                Buttons = message.Buttons.Select(button => new[]
                {
                    new MessageKeyboardButton()
                    {
                        Action = new MessageKeyboardButtonAction()
                        {
                            Type = KeyboardButtonActionType.Text,
                            Label = button
                        }
                    }
                })
            }
        });
        logger.LogInfo($"Sent response to {message.PeerId}");
        WeakReferenceMessenger.Default.Send(new MessageProcessedMessage());
    }

    public async Task<GetConversationsResult> GetUnreadMessages()
    {
        try
        {
            return await vkApiService.GetApi().Messages.GetConversationsAsync(new GetConversationsParams() { Filter = GetConversationFilter.Unread });
        }
        catch(Exception ex)
        {
            throw new ServiceUnhealthyException(ex.Message);
        }
    }
}