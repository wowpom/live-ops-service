using LiveOpsService.Domain.Entities;

namespace LiveOpsService.Models;

public static class ResponseMapping
{
    public static ProjectResponse ToResponse(this Project project) => new(project.Id, project.Slug, project.Name);
    public static PlatformResponse ToResponse(this Platform platform) => new(platform.Id, platform.Slug, platform.Name);
    public static VersionResponse ToResponse(this AppVersion version) => new(version.Id, version.Version);
}
