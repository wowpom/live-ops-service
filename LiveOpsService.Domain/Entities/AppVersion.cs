namespace LiveOpsService.Domain.Entities;

public class AppVersion
{
    public Guid Id { get; set; }
    public string Version { get; set; }
    public Dictionary<string, ConfigEntry> ConfigMap { get; set; } = new();
}