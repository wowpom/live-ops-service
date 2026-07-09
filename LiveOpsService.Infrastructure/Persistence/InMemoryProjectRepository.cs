using LiveOpsService.Application.Common.Exceptions;
using LiveOpsService.Application.Common.Interfaces;
using LiveOpsService.Domain.Entities;

namespace LiveOpsService.Infrastructure.Persistence;

public class InMemoryProjectRepository : IProjectRepository
{
    private readonly Dictionary<string, Project> _projects = new();
    
    public Task<Project> CreateAsync(string slug, string name)
    {
        if (_projects.ContainsKey(slug))
        {
            throw new ConflictException($"Project with slug {slug} already exists");
        }

        var project = new Project()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
        };
        
        _projects.Add(slug, project);
        return Task.FromResult(project);
    }

    public Task<Project?> GetBySlugAsync(string slug)
    {
        if (_projects.TryGetValue(slug, out var project))
        {
            return Task.FromResult(project);
        }
        
        return Task.FromResult<Project?>(null);
    }

    public Task<IReadOnlyList<Project>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<Project>>(_projects.Values.ToList());
    }

    public Task<Platform> AddPlatformAsync(string projectSlug, string slug, string name)
    {
        if (!_projects.TryGetValue(projectSlug, out var project))
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
        
        return Task.FromResult(platform);
    }

    public Task<Platform?> GetPlatformBySlugAsync(string projectSlug, string slug)
    {
        if (!_projects.TryGetValue(projectSlug, out var project))
        {
            throw new NotFoundException($"Project with slug {projectSlug} does not exist");
        }

        if (project.PlatformsMap.TryGetValue(slug, out var platform))
        {
            return Task.FromResult(platform);
        }
        
        return Task.FromResult<Platform?>(null);
    }

    public Task<List<Platform>> GetAllPlatformsAsync(string projectSlug)
    {
        if (!_projects.TryGetValue(projectSlug, out var project))
        {
            throw new NotFoundException($"Project with slug {projectSlug} does not exist");
        }
        
        return Task.FromResult(project.PlatformsMap.Values.ToList());
    }

    public Task<AppVersion> AddVersionAsync(string projectSlug, string platformSlug, string slug)
    {
        if (!_projects.TryGetValue(projectSlug, out var project))
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

        return Task.FromResult(appVersion);
    }

    public Task<AppVersion?> GetVersionBySlugAsync(string projectSlug, string platformSlug, string slug)
    {
        if (!_projects.TryGetValue(projectSlug, out var project))
        {
            throw new NotFoundException($"Project with slug {projectSlug} does not exist");
        }

        if (!project.PlatformsMap.TryGetValue(platformSlug, out var platform))
        {
            throw new NotFoundException($"Platform with slug {platformSlug} does not exist");
        }

        if (platform.AppVersionsMap.ContainsKey(slug))
        {
            return Task.FromResult(platform.AppVersionsMap[slug]);
        }
        
        return Task.FromResult<AppVersion?>(null);
    }

    public Task<List<AppVersion>> GetAllVersions(string projectSlug, string platformSlug)
    {
        if (!_projects.TryGetValue(projectSlug, out var project))
        {
            throw new NotFoundException($"Project with slug {projectSlug} does not exist");
        }

        if (!project.PlatformsMap.TryGetValue(platformSlug, out var platform))
        {
            throw new NotFoundException($"Platform with slug {platformSlug} does not exist");
        }
        
        return Task.FromResult(platform.AppVersionsMap.Values.ToList());
    }
}