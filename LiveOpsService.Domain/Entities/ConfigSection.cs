namespace LiveOpsService.Domain.Entities;

public class ConfigSection
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string JsonBody { get; set; }
}