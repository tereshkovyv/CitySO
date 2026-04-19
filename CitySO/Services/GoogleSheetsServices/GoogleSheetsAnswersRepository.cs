using CitySO.Configuration;
using CitySO.Services.GoogleSheetsServices.Interfaces;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace CitySO.Services.GoogleSheetsServices;

public class GoogleSheetsAnswersRepository(
    IGoogleSheetsService googleSheetsService,
    IConfigurationService configurationService) 
    : IGoogleSheetsAnswersRepository
{
    public async Task WriteAnswer(string sheet, int row, string column, string value)
    {
        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { new List<object> { value } }
        };
        var updateRequest = googleSheetsService.GetService().Spreadsheets.Values.Update(valueRange, 
            configurationService.GetGeneralOptions().GoogleSpreadSheetId, 
            $"{sheet}!{column}{row}");
        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        await updateRequest.ExecuteAsync();
    }
}