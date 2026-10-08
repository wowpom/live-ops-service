using LiveOpsService.Domain.Entities;

namespace LiveOpsService.Models;

public static class ResponseMapping
{
    public static ProjectResponse ToResponse(this Project project) => new(project.Id, project.Slug, project.Name);
    public static PlatformResponse ToResponse(this Platform platform) => new(platform.Id, platform.Slug, platform.Name);
    public static VersionResponse ToResponse(this AppVersion version) => new(version.Id, version.Version);
    public static ConfigResponse ToResponse(this ConfigEntry config) => new(
        config.Id, config.Sections.ToDictionary(pair => pair.Key, pair => pair.Value.Content.Clone()));
    public static PublishedConfigResponse ToResponse(this PublishedConfig config, string version) => new(
        version, config.Revision, config.PublishedAt,
        config.Sections.ToDictionary(pair => pair.Key, pair => pair.Value.Clone()));
}
