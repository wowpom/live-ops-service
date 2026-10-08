namespace LiveOpsService.Domain.Entities;

public class AppVersion
{
    public Guid Id { get; set; }
    public string Version { get; set; }
    public Guid ConfigId { get; set;}
}