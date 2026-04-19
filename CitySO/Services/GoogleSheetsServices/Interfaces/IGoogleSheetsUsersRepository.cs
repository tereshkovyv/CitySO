using CitySO.Models;

namespace CitySO.Services.GoogleSheetsServices.Interfaces;

public interface IGoogleSheetsUsersRepository
{
    public Task<List<AppUser>> GetAll(string category);
}