namespace LiveOpsService.Domain.Entities;

public class ConfigEntry
{
    public Guid Id { get; set; }
    public string JsonBody { get; set; }
    public string Key { get; set; }
}