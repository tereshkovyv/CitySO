using CitySO.Configuration;
using CitySO.Configuration.Models;
using CitySO.Models;
using CitySO.Services.Interfaces;

namespace CitySO.Services;

public class MessagesService(
    IConfigurationService configurationService,
    IAnswersService answersService,
    IUsersService usersService,
    ITasksService tasksService)
    : IMessagesService
{
    public async Task<VkOutgoingMessage> Handle(VkIncomingMessage incomingMessage)
    {
        var (command, value) = GetMessageParts(incomingMessage.Text);
        return command switch
        {
            "/tasks" => HandleTasks(incomingMessage.PeerId.Value),
            "/help" => HandleHelp(incomingMessage.PeerId.Value),
            _ => await HandleGiveAnswer(incomingMessage.PeerId.Value, incomingMessage.Text)
        };
    }

    private VkOutgoingMessage HandleHelp(long requesterId)
    {
        var user = usersService.GetAll().FirstOrDefault(u => u.VkId == requesterId);
        return user is null ? Unauthorized(requesterId) : AnswerWithHelpAndTasksButtons($"""

                                                 Добро пожаловать на {configurationService.GetGeneralOptions().Competition.Name}!
                                                 Ваша команда: {user.Name}
                                                 Вы участвуете в категории: {user.Category}

                                                 Для того, чтобы отправить ответ - отправьте номер задания и ответ через пробел.
                                                 Пример: 6 4

                                                 Также вы можете посмотреть список заданий командой /tasks.
                                                 Для того, чтобы вызвать это сообщение введите /help

                                                 """, requesterId);
    }

    private VkOutgoingMessage HandleTasks(long requesterId)
    {
        var user = usersService.GetAll().FirstOrDefault(u => u.VkId == requesterId);
        if (user is null)
            return Unauthorized(requesterId);
        var tasks = tasksService.GetAll()
            .Where(t => t.Category == user.Category)
            .OrderBy(t => t.Name)
            .Select(t => $"{t.Name}. {t.Text}");

        return AnswerWithHelpAndTasksButtons($"Список заданий в категории {user.Category}\n" + string.Join('\n', tasks),
            requesterId);
    }

    private async Task<VkOutgoingMessage> HandleGiveAnswer(long requesterId, string message)
    {
        var user = usersService.GetAll().FirstOrDefault(u => u.VkId == requesterId);
        if (user is null)
            return Unauthorized(requesterId);
        
        switch (configurationService.GetGeneralOptions().Competition.Status)
        {
            case < CompetitionStatus.InProcess:
                return AnswerWithHelpButton("Соревнование еще не началось", requesterId);
            case > CompetitionStatus.InProcess:
                return AnswerWithHelpButton("Соревнование уже закончилось", requesterId);
        }

        var (p1, p2) = GetMessageParts(message);
        if (p2 == "")
            return AnswerWithHelpAndTasksButtons($"Вы не ввели ответ на задание {p1}. Его нужно вводить через пробел.",
                requesterId);
        
        var task = tasksService.GetAll().FirstOrDefault(t => t.Name == p1 && t.Category == user?.Category);
        if (task is null)
            return AnswerWithHelpAndTasksButtons($"Задания '{p1}' в категории '{user?.Category}' не существует.",
                requesterId);
        var mark = task.Answer == p2 ? "верно" : "неверно";
        var result = $"{p2}({mark})";
        await answersService.GiveAnswer(task, user, result);

        var answer = configurationService.GetGeneralOptions().ShowAnswersImmediately
            ? task.Answer == p2 ? "Ответ верный" : "Ответ неверный"
            : "Ответ принят";
        return AnswerWithHelpButton(answer, requesterId);
    }

    private VkOutgoingMessage Unauthorized(long peerId) => new()
    {
        PeerId = peerId, Text =
            $"""

             Добро пожаловать на {configurationService.GetGeneralOptions().Competition.Name}!
             На данный момент вы не зарегистрированы в системе.

             Для того, чтобы вызвать это сообщение введите /help

             """,
        Buttons = ["/help"]
    };

    private VkOutgoingMessage AnswerWithHelpButton(string message, long peerId) => new()
        { Text = message, PeerId = peerId, Buttons = ["/help"] };
    
    private VkOutgoingMessage AnswerWithHelpAndTasksButtons(string message, long peerId) => new()
        { Text = message, PeerId = peerId, Buttons = ["/help", "/tasks"] };
    
    private static (string, string) GetMessageParts(string message)
    {
        var parts = message.Split([' '], 2);

        var firstPart = parts[0];
        var secondPart = parts.Length != 1 ? parts[1] : "";

        return (firstPart, secondPart);
    }
}