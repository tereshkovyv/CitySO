namespace CitySO.Models;

public class AppUser
{
    public int Id { get; set; }
    public string Name { get; init; } = string.Empty;
    public string VkLink { get; set; } = string.Empty;
    public long VkId { get; init; }
    public string Category { get; init; } = string.Empty;
    public int Row { get; init; }
}