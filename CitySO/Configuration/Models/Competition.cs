using CitySO.Models;

namespace CitySO.Configuration.Models;

public record Competition
{
    public string Name { get; init; } = string.Empty;
    public CompetitionStatus Status { get; init; }
    public bool IsBotEnabled { get; init; }
    public int ProcessedMessages { get; init; }
}