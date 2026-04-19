namespace CitySO.Models;

public record VkOutgoingMessage()
{
    public string Text { get; init; }
    public long? PeerId { get; init; }
    public List<string> Buttons { get; init; }
}