namespace LiveOpsService.Domain.Entities;

public class ConfigEntry
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, ConfigSection> ConfigSection { get; set; } = new();
}