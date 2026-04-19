using CitySO.Configuration;
using CitySO.Exceptions;
using CitySO.Models;
using CitySO.Services.GoogleSheetsServices.Interfaces;
using CitySO.Services.VkServices.Interfaces;

namespace CitySO.Services.GoogleSheetsServices;

public class GoogleSheetsUsersRepository(
    ILogger logger,
    IGoogleSheetsService googleSheetsService,
    IConfigurationService configurationService,
    IVkApiService vkApiService)
: IGoogleSheetsUsersRepository
{
    public async Task<List<AppUser>> GetAll(string category)
    {
        if (googleSheetsService.GetService() is null)
        {
            logger.LogError("Google sheets service doesn't exist");
            return [];
        }
        
        var result = new List<AppUser>();
        
        var request =
            googleSheetsService.GetService().Spreadsheets.Values.Get(configurationService.GetGeneralOptions().GoogleSpreadSheetId,
                $"{category}!A{4}:C");
        var response = await request.ExecuteAsync();
        var values = response?.Values;
        
        if (values is null)
            return result;

        for (var i = 0; i < values!.Count; i++)
        {
            if (!int.TryParse(values?[0]?[0]?.ToString(), out var id))
                throw new IncorrectDataException(
                    $"Неверный формат id. {values?[0]?[0]} должен быть числом");
            if (string.IsNullOrEmpty(values?[i]?[1]?.ToString()))
                throw new IncorrectDataException($"Пропущено название команды для id {id}");
            
            if (string.IsNullOrEmpty(values?[i]?[2]?.ToString()))
                throw new IncorrectDataException($"Нет ссылки на капитана у команды {values![i][1]}");
            var vkId = values?[i]?[2]?.ToString() ?? "";

            var user = new AppUser()
            {
                Category = category,
                Id = id,
                Name = values![i][1].ToString()!,
                VkLink = vkId,
                VkId = await vkApiService.GetUserIdByUrl(vkId),
                Row = i + 4
            };
            result.Add(user);
        }

        return result;
    }
}