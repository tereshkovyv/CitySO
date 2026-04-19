using Google.Apis.Sheets.v4;

namespace CitySO.Services.GoogleSheetsServices.Interfaces;

public interface IGoogleSheetsService
{
    public void Authorize(string jsonKey);
    public void LogOut();
    public SheetsService GetService();
    public Task<bool> IsHealthy();
}