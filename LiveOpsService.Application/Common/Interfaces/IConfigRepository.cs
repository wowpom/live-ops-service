using LiveOpsService.Domain.Entities;

namespace LiveOpsService.Application.Common.Interfaces;

public interface IConfigRepository
{
    // Creation and linking to the version must be atomic; duplicate creation is a conflict.
    Task<ConfigEntry> CreateAsync(Guid versionId, ConfigEntry config);
    Task<ConfigEntry?> GetByVersionIdAsync(Guid versionId);
    Task SaveSectionAsync(Guid versionId, ConfigSection section);
    Task<bool> DeleteSectionAsync(Guid versionId, string key);
    // Snapshot creation, revision increment and replacement must be atomic with draft edits.
    Task<PublishedConfig> PublishAsync(Guid versionId);
    Task<PublishedConfig?> GetPublishedAsync(Guid versionId);
}
