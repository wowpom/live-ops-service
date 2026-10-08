namespace LiveOpsService.Domain.Entities;

public class AppVersion
{
    public Guid Id { get; set; }
    public required string Version { get; set; }
    public Guid ConfigId { get; set; }
}
