using CitySO.Models;
using CitySO.Services.GoogleSheetsServices.Interfaces;
using CitySO.Services.Interfaces;

namespace CitySO.Services;

public class UsersService(ICategoriesService categoriesService, 
    IGoogleSheetsUsersRepository googleSheetsUsersRepository) : IUsersService
{
    private List<AppUser> _users = [];

    public List<AppUser> GetAll() => _users;

    public async Task LoadFromGoogleSheets()
    {
        _users = [];
        var categories = await categoriesService.GetAll();
        foreach (var category in categories)
        {
            var tasks = await googleSheetsUsersRepository.GetAll(category);
            _users.AddRange(tasks);
        }
        
    }
}