namespace CitySO.Services.Interfaces;

public interface ICategoriesService
{
    Task<List<string>> GetAll();
    Task AddCategory(string categoryName, int checkpointsCount);
}