using CitySO.Models;

namespace CitySO.Services.Interfaces;

public interface IAnswersService
{
    public Task GiveAnswer(AppTask task, AppUser user, string value);
}