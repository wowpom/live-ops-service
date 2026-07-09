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
}