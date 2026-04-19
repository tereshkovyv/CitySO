namespace CitySO.Services.GoogleSheetsServices.Interfaces;

public interface IGoogleSheetsAnswersRepository
{
    public Task WriteAnswer(string sheet, int row, string column, string value);
}