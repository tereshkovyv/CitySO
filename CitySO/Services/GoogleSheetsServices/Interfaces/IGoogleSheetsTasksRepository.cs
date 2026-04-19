using CitySO.Models;

namespace CitySO.Services.GoogleSheetsServices.Interfaces;

public interface IGoogleSheetsTasksRepository
{
    public Task<List<AppTask>> GetAll(string category);
}