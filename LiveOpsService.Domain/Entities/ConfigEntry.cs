namespace LiveOpsService.Domain.Entities;

public class ConfigEntry
{
    public Guid Id { get; set; }
    public Dictionary<string, ConfigSection> Sections { get; set; } = new();
}
