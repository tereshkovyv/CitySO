namespace CitySO.Models;

public record VkIncomingMessage
{
    public string Text { get; init; } = string.Empty;
    public long? PeerId { get; init; }
}