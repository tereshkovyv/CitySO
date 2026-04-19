using CitySO.Configuration.Models;
using CitySO.Models;

namespace CitySO.Extensions;

public static class CompetitionStatusExtensions
{
    public static string Name(this CompetitionStatus status)
    {
        return status switch
        {
            CompetitionStatus.Preparing => "Подготовка",
            CompetitionStatus.Ready => "Готово",
            CompetitionStatus.InProcess => "В процессе",
            CompetitionStatus.Finished => "Завершено",
            _ => ""
        };
    }
    
    public static string Description(this CompetitionStatus status)
    {
        return status switch
        {
            CompetitionStatus.Preparing => "Можно менять категории и команды в таблице. Бот информацию не выдает",
            CompetitionStatus.Ready => "Нельзя менять таблицу. Можно получать информацию через бота. Нельзя давать ответы",
            CompetitionStatus.InProcess => "Нельзя менять таблицу. Можно получать информацию через бота. Можно давать ответы",
            CompetitionStatus.Finished => "Нельзя менять таблицу. Можно получать информацию через бота. Нельзя давать ответы",
            _ => ""
        };
    }
}