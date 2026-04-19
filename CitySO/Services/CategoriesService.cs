using CitySO.Configuration;
using CitySO.Helpers;
using CitySO.Services.GoogleSheetsServices.Interfaces;
using CitySO.Services.Interfaces;
using Google.Apis.Sheets.v4.Data;

namespace CitySO.Services;

public class CategoriesService(
    IConfigurationService configurationService,
    IGoogleSheetsService googleSheetsService) : ICategoriesService
{
    private string _categoryName = string.Empty;
    private int? _sheetId;
    private int _checkpointsCount;
    public async Task<List<string>> GetAll()
    {
        var spreadsheet = await googleSheetsService.GetService().Spreadsheets
            .Get(configurationService.GetGeneralOptions().GoogleSpreadSheetId).ExecuteAsync();
        return spreadsheet.Sheets.Select(s => s.Properties.Title).ToList();
    }
    
    public async Task AddCategory(string categoryName, int checkpointsCount)
    {
        _categoryName = categoryName;
        _checkpointsCount = checkpointsCount;
        _sheetId = await CreateSheet();
        
        var requests = new List<Request>
        {
            AddHeaderRequest(),
            AddVerticalBorderRequest(2),
            AddVerticalBorderRequest(4 + _checkpointsCount),
            AddHorizontalBorderRequest( 2),
            ColorizeRangeRequest(PinkCell(), 0, 4, 5 + _checkpointsCount),
            ColorizeRangeRequest(PinkCell(), 3, null, 0, 3),
            ColorizeRangeRequest(GreyCell(), 3, null, 5 + _checkpointsCount, null),
            ColorizeRangeRequest(GreyCell(), 3, null, 4, 4 + _checkpointsCount),
            SetInitialUserRequest("1","Админ","https://vk.com/tereshkov_yura"),
            SetInitialTaskRequest("1", "Сколько есть времен года?", "4"),
            AddVerificationFormulasRequest(),
            AddSummingFormulasRequest()
        };
        
        var batchUpdateRequest = new BatchUpdateSpreadsheetRequest(){Requests = requests};
        await googleSheetsService.GetService().Spreadsheets.BatchUpdate(batchUpdateRequest, configurationService.GetGeneralOptions().GoogleSpreadSheetId).ExecuteAsync();
    }

    private async Task<int?> CreateSheet()
    {
        var addSheetRequest = new AddSheetRequest
        {
            Properties = new SheetProperties
            {
                Title = _categoryName,
                GridProperties = new GridProperties
                {
                    RowCount = 500,
                    ColumnCount = 100
                }
            }
        };

        var batchUpdateRequest1 = new BatchUpdateSpreadsheetRequest
        {
            Requests = new List<Request> { new() { AddSheet = addSheetRequest } }
        };
        var spreadsheetId = configurationService.GetGeneralOptions().GoogleSpreadSheetId;
        
        var response = await googleSheetsService.GetService().Spreadsheets.BatchUpdate(batchUpdateRequest1, spreadsheetId).ExecuteAsync();
        return response.Replies[0].AddSheet.Properties.SheetId;
    }

    private Request ColorizeRangeRequest(
        CellData cell,
        int? startRowIndex = null,
        int? endRowIndex = null, 
        int? startColumnIndex = null, 
        int? endColumnIndex = null)
    {
        return new Request
        {
            RepeatCell = new RepeatCellRequest
            {
                Range = new GridRange
                {
                    SheetId = _sheetId,
                    StartRowIndex = startRowIndex,
                    EndRowIndex = endRowIndex,
                    StartColumnIndex = startColumnIndex,
                    EndColumnIndex = endColumnIndex
                },
                Cell = cell,
                Fields = "userEnteredFormat.backgroundColor"
            }
        };
    }

    private Request AddSummingFormulasRequest()
    {
        return new Request
        {
            RepeatCell = new RepeatCellRequest
            {
                Range = new GridRange
                {
                    SheetId = _sheetId,
                    StartRowIndex = 3,
                    StartColumnIndex = 4 + _checkpointsCount,
                    EndColumnIndex = 5 + _checkpointsCount
                },
                Cell = new CellData
                {
                    UserEnteredValue = new ExtendedValue
                    {
                        FormulaValue =
                            $"=СУММ(INDIRECT(\"{NumberLetterConverter.GetLetter(4)}\" & ROW() & \":\" & \"{NumberLetterConverter.GetLetter(4 + _checkpointsCount)}\" & ROW()))"
                    }
                },
                Fields = "userEnteredValue,userEnteredFormat.backgroundColor"
            }
        };
    }

    private Request AddVerificationFormulasRequest()
    {
        return new Request
        {
            RepeatCell = new RepeatCellRequest
            {
                Range = new GridRange
                {
                    SheetId = _sheetId,
                    StartRowIndex = 3,
                    StartColumnIndex = 3,
                    EndColumnIndex = 4
                },
                Cell = new CellData
                {
                    UserEnteredValue = new ExtendedValue
                    {
                        FormulaValue =
                            $"=СЧЁТЕСЛИ(INDIRECT(\"{NumberLetterConverter.GetLetter(6 + _checkpointsCount)}\" & ROW() & \":\" & ROW()); \"*(верно)\")"
                    }
                },
                Fields = "userEnteredValue,userEnteredFormat.backgroundColor"
            }
        };
    }

    private Request SetInitialTaskRequest(string name, string text, string answer)
    {
        return new Request()
        {
            UpdateCells = new UpdateCellsRequest()
            {
                Start = new GridCoordinate()
                {
                    SheetId = _sheetId,
                    RowIndex = 0,
                    ColumnIndex = 5 + _checkpointsCount
                },
                Rows = new List<RowData>()
                {
                    new() { Values = new List<CellData>() { WhiteCell(name) } },
                    new() { Values = new List<CellData>() { WhiteCell(text) } },
                    new() { Values = new List<CellData>() { WhiteCell(answer) } }
                },
                Fields = "userEnteredValue"
            }
        };
    }

    private Request SetInitialUserRequest(string id, string name, string vkLink)
    {
        return new Request()
        {
            UpdateCells = new UpdateCellsRequest()
            {
                Start = new GridCoordinate()
                {
                    SheetId = _sheetId,
                    RowIndex = 3,
                    ColumnIndex = 0
                },
                Rows = new List<RowData>()
                {
                    new()
                    {
                        Values = new List<CellData>()
                        {
                            WhiteCell(id), WhiteCell(name), WhiteCell(vkLink)
                        }
                    }
                },
                Fields = "userEnteredValue"
            }
        };
    }

    private Request AddHorizontalBorderRequest(int row)
    {
        return new Request
        {
            UpdateBorders = new UpdateBordersRequest
            {
                Range = new GridRange { SheetId = _sheetId, StartRowIndex = row, EndRowIndex = row + 1},
                Bottom = new Border { Style = "SOLID", Width = 3 }
            }
        };
    }

    private Request AddVerticalBorderRequest(int column)
    {
        var request = new Request
        {
            UpdateBorders = new UpdateBordersRequest
            {
                Range = new GridRange { SheetId = _sheetId, StartColumnIndex = column, EndColumnIndex = column + 1 },
                Right = new Border { Style = "SOLID", Width = 3 }
            }
        };
        return request;
    }

    private Request AddHeaderRequest()
    {
        var firstLine = new List<CellData>() { WhiteCell(_categoryName) };
        firstLine.AddRange(Enumerable.Range(0, 4 + _checkpointsCount).Select(_ => BlackCell()).ToList());
        var secondLine = new List<CellData>(){BlackCell("Розовые области можно менять до начала игры. Серые области можно менять в процессе. Остальные менять нельзя")};
        secondLine.AddRange(Enumerable.Range(0, 4 + _checkpointsCount).Select(_ => BlackCell()).ToList());
        var thirdLine = new List<CellData>()
        {
            WhiteCell("Id"), WhiteCell("Команда"), WhiteCell("Капитан"),
            WhiteCell("Правильных ответов")
        };
        thirdLine.AddRange(Enumerable.Range(0, _checkpointsCount).Select(i => GreyCell($"Точка {i + 1}")).ToList());
        thirdLine.Add(WhiteCell("Сумма баллов"));
        
        var request = new Request()
        {
            UpdateCells = new UpdateCellsRequest()
            {
                Start = new GridCoordinate()
                {
                    SheetId = _sheetId,
                    RowIndex = 0,
                    ColumnIndex = 0
                },
                Rows = new List<RowData>()
                {
                    new() { Values = firstLine },
                    new() { Values = secondLine },
                    new() { Values = thirdLine }
                },
                Fields = "userEnteredValue,userEnteredFormat.backgroundColor,userEnteredFormat.textFormat.foregroundColor"
            }
        };
        return request;
    }

    private static CellData BlackCell(string text = "") => new()
    {
        UserEnteredValue = new ExtendedValue() { StringValue = text },
        UserEnteredFormat = new CellFormat
        {
            BackgroundColor = new Color()
            {
                Red = 0.1f,
                Green = 0.1f,
                Blue = 0.1f
            },
            TextFormat = new TextFormat
            {
                ForegroundColor = new Color()
                {
                    Red = 1.0f,
                    Green = 1.0f,
                    Blue = 1.0f
                }
            }
        }
    };
    
    private static CellData WhiteCell(string text) => new()
    {
        UserEnteredValue = new ExtendedValue() { StringValue = text }
    };
    
    private static CellData GreyCell(string text = "") => new()
    {
        UserEnteredValue = new ExtendedValue() { StringValue = text },
        UserEnteredFormat = new CellFormat
        {
            BackgroundColor = new Color()
            {
                Red = 0.9f,
                Green = 0.9f,
                Blue = 0.9f
            }
        }
    };
    
    private static CellData PinkCell() => new()
    {
        UserEnteredFormat = new CellFormat
        {
            BackgroundColor = new Color()
            {
                Red = 1f,
                Green = 0.9f,
                Blue = 0.9f
            }
        }
    };
}