using System.Text.Json;
using LiveOpsService.Application.Common.Exceptions;
using LiveOpsService.Application.Services;
using LiveOpsService.Models;

namespace LiveOpsService.Endpoints;

public static class ConfigEndpoint
{
    private const int MaxSectionBodyBytes = 64 * 1024;

    public static void MapConfigEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin/projects/{projectSlug}/platforms/{platformSlug}/versions/{version}/config")
            .WithTags("Admin configs");
        group.MapPost("", Create);
        group.MapGet("", Get);
        group.MapPut("/sections/{key}", SaveSection);
        group.MapDelete("/sections/{key}", DeleteSection);
    }

    private static async Task<IResult> Create(string projectSlug, string platformSlug, string version, ConfigService service)
    {
        var config = await service.CreateAsync(projectSlug, platformSlug, version);
        return Results.Created($"/api/admin/projects/{projectSlug}/platforms/{platformSlug}/versions/{version}/config", config.ToResponse());
    }

    private static async Task<IResult> Get(string projectSlug, string platformSlug, string version, ConfigService service)
    {
        var config = await service.GetAsync(projectSlug, platformSlug, version);
        return Results.Ok(config.ToResponse());
    }

    private static async Task<IResult> SaveSection(string projectSlug, string platformSlug, string version, string key,
        HttpRequest request, ConfigService service, CancellationToken cancellationToken)
    {
        if (!request.HasJsonContentType())
        {
            throw new BadHttpRequestException("Expected application/json.", StatusCodes.Status415UnsupportedMediaType);
        }

        if (request.ContentLength > MaxSectionBodyBytes)
        {
            throw new BadHttpRequestException("Section body exceeds 64 KiB.", StatusCodes.Status413PayloadTooLarge);
        }

        using var body = new MemoryStream();
        var buffer = new byte[8192];
        while (true)
        {
            var count = await request.Body.ReadAsync(buffer, cancellationToken);
            if (count == 0)
            {
                break;
            }

            if (body.Length + count > MaxSectionBodyBytes)
            {
                throw new BadHttpRequestException("Section body exceeds 64 KiB.", StatusCodes.Status413PayloadTooLarge);
            }

            body.Write(buffer, 0, count);
        }

        body.Position = 0;
        JsonDocument document;
        try
        {
            document = await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken);
        }
        catch (JsonException)
        {
            throw new BadRequestException("Section body must contain valid JSON.");
        }

        using (document)
        {
            await service.SaveSectionAsync(projectSlug, platformSlug, version, key, document.RootElement);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteSection(string projectSlug, string platformSlug, string version, string key, ConfigService service)
    {
        await service.DeleteSectionAsync(projectSlug, platformSlug, version, key);
        return Results.NoContent();
    }
}
