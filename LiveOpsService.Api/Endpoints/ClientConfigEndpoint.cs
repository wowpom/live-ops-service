using LiveOpsService.Application.Services;
using LiveOpsService.Models;

namespace LiveOpsService.Endpoints;

public static class ClientConfigEndpoint
{
    public static void MapClientConfigEndpoints(this WebApplication app)
    {
        app.MapGet("/api/config/{projectSlug}/{platformSlug}/{version}", GetPublished)
            .WithTags("Client configs");
    }

    private static async Task<IResult> GetPublished(string projectSlug, string platformSlug, string version, ConfigService service)
    {
        var publication = await service.GetPublishedAsync(projectSlug, platformSlug, version);
        return Results.Ok(publication.ToResponse(version));
    }
}
