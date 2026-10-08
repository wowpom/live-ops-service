using LiveOpsService.Application.Common.Exceptions;
using LiveOpsService.Application.Common.Interfaces;
using LiveOpsService.Domain.Entities;
using static LiveOpsService.Infrastructure.Persistence.InMemoryEntityCopy;

namespace LiveOpsService.Infrastructure.Persistence;

public class InMemoryConfigRepository(InMemoryStore store) : IConfigRepository
{
    public Task<ConfigEntry> CreateAsync(Guid versionId, ConfigEntry config)
    {
        lock (store.SyncRoot)
        {
            var version = store.Projects.Values.SelectMany(project => project.PlatformsMap.Values)
                .SelectMany(platform => platform.AppVersionsMap.Values).FirstOrDefault(item => item.Id == versionId);
            if (version is null)
            {
                throw new NotFoundException("Version not found.");
            }

            if (version.ConfigId is not null)
            {
                throw new ConflictException("Draft configuration already exists.");
            }

            store.Configs.Add(versionId, Copy(config));
            version.ConfigId = config.Id;
            return Task.FromResult(Copy(config));
        }
    }

    public Task<ConfigEntry?> GetByVersionIdAsync(Guid versionId)
    {
        lock (store.SyncRoot)
        {
            return Task.FromResult(store.Configs.TryGetValue(versionId, out var config) ? Copy(config) : null);
        }
    }

    public Task SaveSectionAsync(Guid versionId, ConfigSection section)
    {
        lock (store.SyncRoot)
        {
            var config = GetRequired(versionId);
            config.Sections[section.Key] = new ConfigSection { Key = section.Key, Content = section.Content.Clone() };
            return Task.CompletedTask;
        }
    }

    public Task<bool> DeleteSectionAsync(Guid versionId, string key)
    {
        lock (store.SyncRoot)
        {
            return Task.FromResult(GetRequired(versionId).Sections.Remove(key));
        }
    }

    public Task<PublishedConfig> PublishAsync(Guid versionId)
    {
        lock (store.SyncRoot)
        {
            var draft = GetRequired(versionId);
            if (draft.Sections.Count == 0)
            {
                throw new BadRequestException("A draft must contain at least one section before publication.");
            }

            var revision = store.Publications.TryGetValue(versionId, out var previous)
                ? checked(previous.Revision + 1)
                : 1;
            var publication = new PublishedConfig
            {
                Revision = revision,
                PublishedAt = DateTimeOffset.UtcNow,
                Sections = draft.Sections.ToDictionary(pair => pair.Key, pair => pair.Value.Content.Clone())
            };

            store.Publications[versionId] = publication;
            return Task.FromResult(Copy(publication));
        }
    }

    public Task<PublishedConfig?> GetPublishedAsync(Guid versionId)
    {
        lock (store.SyncRoot)
        {
            return Task.FromResult(store.Publications.TryGetValue(versionId, out var publication) ? Copy(publication) : null);
        }
    }

    private ConfigEntry GetRequired(Guid versionId)
    {
        if (!store.Configs.TryGetValue(versionId, out var config))
        {
            throw new NotFoundException("Draft configuration not found.");
        }

        return config;
    }
}
