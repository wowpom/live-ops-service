namespace LiveOpsService.Domain.Entities;

public class Platform
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public Dictionary<string, AppVersion> AppVersionsMap { get; set; } =  new();
}