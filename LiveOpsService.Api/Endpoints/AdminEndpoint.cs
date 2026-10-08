using LiveOpsService.Application.Common.Exceptions;
using LiveOpsService.Application.Common.Interfaces;
using LiveOpsService.Models;

namespace LiveOpsService.Endpoints;

public static class AdminEndpoint
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin").WithTags("Admin");
        
        group.MapPost("/projects", CreateProject);
        group.MapGet("/projects/{slug}", GetBySlug);
        group.MapGet("/projects", GetAll);
        group.MapPost("/projects/{projectSlug}/platforms", CreatePlatform);
        group.MapPost("/projects/{projectSlug}/platforms/{platformSlug}/versions", CreateVersion);
    }

    private static async Task<IResult> CreateProject(CreateProjectRequest request, IProjectRepository repository)
    {
        var project = await repository.CreateAsync(request.slug, request.name);
        return Results.Created($"/api/admin/projects/{request.slug}", project);
    }

    private static async Task<IResult> CreatePlatform(string projectSlug, CreatePlatformRequest request, IProjectRepository repository)
    {
        var platform = await repository.AddPlatformAsync(projectSlug, request.slug, request.name);
        return Results.Created($"/api/admin/projects/{projectSlug}/platforms/{request.slug}", platform);
    }

    private static async Task<IResult> CreateVersion(string projectSlug, string platformSlug, CreateVersionRequest request, IProjectRepository repository)
    {
        var version = await repository.AddVersionAsync(projectSlug, platformSlug, request.version);
        return Results.Created($"/api/admin/projects/{projectSlug}/platforms/{platformSlug}/versions/{request.version}", version);
    }

    private static async Task<IResult> GetAll(IProjectRepository repository)
    {
        var projects = await repository.GetAllAsync();
        return Results.Ok(projects);
    }

    private static async Task<IResult> GetBySlug(IProjectRepository repository, string slug)
    {
        var project = await repository.GetBySlugAsync(slug);
        if (project is null)
            throw new NotFoundException($"Project '{slug}' not found.");

        return Results.Ok(project);
    }
}
