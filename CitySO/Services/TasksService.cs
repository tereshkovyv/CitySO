using CitySO.Models;
using CitySO.Services.GoogleSheetsServices.Interfaces;
using CitySO.Services.Interfaces;

namespace CitySO.Services;

public class TasksService(
    ICategoriesService categoriesService, 
    IGoogleSheetsTasksRepository googleSheetsTasksRepository) : ITasksService
{
    private List<AppTask> _tasks = [];

    public List<AppTask> GetAll() => _tasks;

    public async Task LoadFromGoogleSheets()
    {
        _tasks = [];
        var categories = await categoriesService.GetAll();
        foreach (var category in categories)
        {
            var tasks = await googleSheetsTasksRepository.GetAll(category);
            _tasks.AddRange(tasks);
        }
        
    }
}