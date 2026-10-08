namespace LiveOpsService.Domain.Entities;

public class ConfigSection
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public required string JsonBody { get; set; }
}
