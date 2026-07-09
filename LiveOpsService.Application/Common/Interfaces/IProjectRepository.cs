using LiveOpsService.Domain.Entities;

namespace LiveOpsService.Application.Common.Interfaces;

public interface IProjectRepository
{
    Task<Project> CreateAsync(string slug, string name);
    
    Task<Project?> GetBySlugAsync(string slug);
    
    Task<IReadOnlyList<Project>> GetAllAsync();
}