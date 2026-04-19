using CitySO.Models;

namespace CitySO.Services.Interfaces;

public interface ITasksService
{
    public List<AppTask> GetAll();
    public Task LoadFromGoogleSheets();
}