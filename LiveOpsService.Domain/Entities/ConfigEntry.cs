namespace LiveOpsService.Domain.Entities;

public class ConfigEntry
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Dictionary<string, ConfigSection> ConfigSection { get; set; } = new();
}
