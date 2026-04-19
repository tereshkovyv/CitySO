using CitySO.Configuration;
using CitySO.Helpers;
using CitySO.Models;
using CitySO.Services.GoogleSheetsServices.Interfaces;

namespace CitySO.Services.GoogleSheetsServices;

public class GoogleSheetsTasksRepository(
    ILogger logger,
    IGoogleSheetsService googleSheetsService,
    IConfigurationService configurationService)
    : IGoogleSheetsTasksRepository
{
    public async Task<List<AppTask>> GetAll(string category)
    {
        if (googleSheetsService.GetService() is null)
        {
            logger.LogError("Google sheets service doesn't exist");
            return [];
        }

        var result = new List<AppTask>();

        var request =
            googleSheetsService.GetService().Spreadsheets.Values.Get(
                configurationService.GetGeneralOptions().GoogleSpreadSheetId,
                $"{category}!{NumberLetterConverter.GetLetter(8)}1:ZZZ3");
        var response = await request.ExecuteAsync();
        var values = response?.Values;
        if (values?[0] is null)
            return result;

        for (var i = 0; i < values![0].Count; i++)
        {
            var name = values?[0]?[i]?.ToString() ?? string.Empty;
            var text = values?[1]?[i]?.ToString() ?? string.Empty;
            var answer = values?[2]?[i]?.ToString() ?? string.Empty;

            var task = new AppTask()
            {
                Answer = answer,
                Category = category,
                Column = NumberLetterConverter.GetLetter(i + 8),
                Name = name,
                Text = text
            };
            result.Add(task);
        }

        return result;
    }
}