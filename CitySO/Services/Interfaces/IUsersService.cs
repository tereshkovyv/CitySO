using CitySO.Models;

namespace CitySO.Services.Interfaces;

public interface IUsersService
{
    public List<AppUser> GetAll();
    public Task LoadFromGoogleSheets();
}