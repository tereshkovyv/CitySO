using CitySO.Models;

namespace CitySO.Configuration.Models;

public record AppConfiguration
{
    public Competition Competition { get; init; } = new();
    public string VkApiKey { get; init; } = string.Empty;
    public ulong VkGroupId { get; init; }
    public string GoogleSpreadSheetId { get; init; } = string.Empty;
    public bool ShowAnswersImmediately { get; init; }
}