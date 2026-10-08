using LiveOpsService.Domain.Entities;

namespace LiveOpsService.Infrastructure.Persistence;

internal static class InMemoryEntityCopy
{
    internal static Project Copy(Project project) => new()
    {
        Id = project.Id,
        Slug = project.Slug,
        Name = project.Name,
        PlatformsMap = project.PlatformsMap.ToDictionary(pair => pair.Key, pair => Copy(pair.Value))
    };

    internal static Platform Copy(Platform platform) => new()
    {
        Id = platform.Id,
        Slug = platform.Slug,
        Name = platform.Name,
        AppVersionsMap = platform.AppVersionsMap.ToDictionary(pair => pair.Key, pair => Copy(pair.Value))
    };

    internal static AppVersion Copy(AppVersion version) => new()
    {
        Id = version.Id,
        Version = version.Version,
        ConfigId = version.ConfigId
    };

    internal static ConfigEntry Copy(ConfigEntry config) => new()
    {
        Id = config.Id,
        Sections = config.Sections.ToDictionary(pair => pair.Key,
            pair => new ConfigSection { Key = pair.Key, Content = pair.Value.Content.Clone() })
    };

    internal static PublishedConfig Copy(PublishedConfig config) => new()
    {
        Revision = config.Revision,
        PublishedAt = config.PublishedAt,
        Sections = config.Sections.ToDictionary(pair => pair.Key, pair => pair.Value.Clone())
    };
}
