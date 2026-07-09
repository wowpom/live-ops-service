namespace LiveOpsService.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public Dictionary<string, Platform> PlatformsMap { get; set; }  = new();
}