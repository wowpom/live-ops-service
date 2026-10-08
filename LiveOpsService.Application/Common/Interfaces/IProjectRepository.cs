using LiveOpsService.Domain.Entities;

namespace LiveOpsService.Application.Common.Interfaces;

public interface IProjectRepository
{
    Task<Project> CreateAsync(string slug, string name);
    
    Task<Project?> GetBySlugAsync(string slug);
    
    Task<IReadOnlyList<Project>> GetAllAsync();
    
    Task<Platform> AddPlatformAsync(string projectSlug, string slug, string name);
    
    Task<Platform?> GetPlatformBySlugAsync(string projectSlug, string slug);
    
    Task<List<Platform>> GetAllPlatformsAsync(string projectSlug);
    
    Task<AppVersion> AddVersionAsync(string projectSlug,string platformSlug, string slug);
    Task<AppVersion?> GetVersionBySlugAsync(string projectSlug, string platformSlug, string slug);
    Task<List<AppVersion>> GetAllVersions(string projectSlug, string platformSlug);
}