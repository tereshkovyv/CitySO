using CitySO.Models;
using CitySO.Services.GoogleSheetsServices.Interfaces;
using CitySO.Services.Interfaces;

namespace CitySO.Services;

public class AnswersService(
    IGoogleSheetsAnswersRepository googleSheetsAnswersRepository) : IAnswersService
{
    public async Task GiveAnswer(AppTask task, AppUser user, string value)
    {
        await googleSheetsAnswersRepository.WriteAnswer(user.Category, user.Row, task.Column, value);
    }
}