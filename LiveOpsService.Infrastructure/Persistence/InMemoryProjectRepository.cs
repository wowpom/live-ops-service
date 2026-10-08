using LiveOpsService.Application.Common.Exceptions;
using LiveOpsService.Application.Common.Interfaces;
using LiveOpsService.Domain.Entities;
using static LiveOpsService.Infrastructure.Persistence.InMemoryEntityCopy;

namespace LiveOpsService.Infrastructure.Persistence;

public class InMemoryProjectRepository(InMemoryStore store) : IProjectRepository
{
    private Dictionary<string, Project> Projects => store.Projects;

    public Task<Project> CreateAsync(string slug, string name)
    {
        lock (store.SyncRoot)
        {
            if (Projects.ContainsKey(slug))
            {
                throw new ConflictException($"Project with slug {slug} already exists");
            }

            var project = new Project()
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = slug,
            };

            Projects.Add(slug, project);
            return Task.FromResult(Copy(project));
        }
    }

    public Task<Project?> GetBySlugAsync(string slug)
    {
        lock (store.SyncRoot)
        {
            if (Projects.TryGetValue(slug, out var project))
            {
                return Task.FromResult<Project?>(Copy(project));
            }

            return Task.FromResult<Project?>(null);
        }
    }

    public Task<IReadOnlyList<Project>> GetAllAsync()
    {
        lock (store.SyncRoot)
        {
            return Task.FromResult<IReadOnlyList<Project>>(Projects.Values.Select(Copy).ToList());
        }
    }

    public Task<Platform> AddPlatformAsync(string projectSlug, string slug, string name)
    {
        lock (store.SyncRoot)
        {
            if (!Projects.TryGetValue(projectSlug, out var project))
            {
                throw new NotFoundException($"Project with slug {projectSlug} does not exist");
            }

            if (project.PlatformsMap.ContainsKey(slug))
            {
                throw new ConflictException($"Platform with slug {slug} already exists");
            }

            var platform = new Platform()
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = slug
            };

            project.PlatformsMap.Add(slug, platform);

            return Task.FromResult(Copy(platform));
        }
    }

    public Task<Platform?> GetPlatformBySlugAsync(string projectSlug, string slug)
    {
        lock (store.SyncRoot)
        {
            if (!Projects.TryGetValue(projectSlug, out var project))
            {
                throw new NotFoundException($"Project with slug {projectSlug} does not exist");
            }

            if (project.PlatformsMap.TryGetValue(slug, out var platform))
            {
                return Task.FromResult<Platform?>(Copy(platform));
            }

            return Task.FromResult<Platform?>(null);
        }
    }

    public Task<List<Platform>> GetAllPlatformsAsync(string projectSlug)
    {
        lock (store.SyncRoot)
        {
            if (!Projects.TryGetValue(projectSlug, out var project))
            {
                throw new NotFoundException($"Project with slug {projectSlug} does not exist");
            }

            return Task.FromResult(project.PlatformsMap.Values.Select(Copy).ToList());
        }
    }

    public Task<AppVersion> AddVersionAsync(string projectSlug, string platformSlug, string slug)
    {
        lock (store.SyncRoot)
        {
            if (!Projects.TryGetValue(projectSlug, out var project))
            {
                throw new NotFoundException($"Project with slug {projectSlug} does not exist");
            }

            if (!project.PlatformsMap.TryGetValue(platformSlug, out var platform))
            {
                throw new NotFoundException($"Platform with slug {platformSlug} does not exist");
            }

            if (platform.AppVersionsMap.ContainsKey(slug))
            {
                throw new ConflictException($"Version with slug {slug} already exists");
            }

            var appVersion = new AppVersion
            {
                Id = Guid.NewGuid(),
                Version = slug
            };

            platform.AppVersionsMap.Add(slug, appVersion);

            return Task.FromResult(Copy(appVersion));
        }
    }

    public Task<AppVersion?> GetVersionBySlugAsync(string projectSlug, string platformSlug, string slug)
    {
        lock (store.SyncRoot)
        {
            if (!Projects.TryGetValue(projectSlug, out var project))
            {
                throw new NotFoundException($"Project with slug {projectSlug} does not exist");
            }

            if (!project.PlatformsMap.TryGetValue(platformSlug, out var platform))
            {
                throw new NotFoundException($"Platform with slug {platformSlug} does not exist");
            }

            if (platform.AppVersionsMap.ContainsKey(slug))
            {
                return Task.FromResult<AppVersion?>(Copy(platform.AppVersionsMap[slug]));
            }

            return Task.FromResult<AppVersion?>(null);
        }
    }

    public Task<List<AppVersion>> GetAllVersions(string projectSlug, string platformSlug)
    {
        lock (store.SyncRoot)
        {
            if (!Projects.TryGetValue(projectSlug, out var project))
            {
                throw new NotFoundException($"Project with slug {projectSlug} does not exist");
            }

            if (!project.PlatformsMap.TryGetValue(platformSlug, out var platform))
            {
                throw new NotFoundException($"Platform with slug {platformSlug} does not exist");
            }

            return Task.FromResult(platform.AppVersionsMap.Values.Select(Copy).ToList());
        }
    }
}