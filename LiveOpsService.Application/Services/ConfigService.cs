using System.Text.Json;
using System.Text.RegularExpressions;
using LiveOpsService.Application.Common.Exceptions;
using LiveOpsService.Application.Common.Interfaces;
using LiveOpsService.Domain.Entities;

namespace LiveOpsService.Application.Services;

public class ConfigService(IProjectRepository projects, IConfigRepository configs)
{
    public async Task<ConfigEntry> CreateAsync(string projectSlug, string platformSlug, string version)
    {
        var appVersion = await GetVersionAsync(projectSlug, platformSlug, version);
        return await configs.CreateAsync(appVersion.Id, new ConfigEntry { Id = Guid.NewGuid() });
    }

    public async Task<ConfigEntry> GetAsync(string projectSlug, string platformSlug, string version)
    {
        var appVersion = await GetVersionAsync(projectSlug, platformSlug, version);
        var config = await configs.GetByVersionIdAsync(appVersion.Id);
        if (config is null)
        {
            throw new NotFoundException("Draft configuration not found.");
        }

        return config;
    }

    public async Task SaveSectionAsync(string projectSlug, string platformSlug, string version, string key, JsonElement content)
    {
        ValidateKey(key);
        if (content.ValueKind != JsonValueKind.Object)
        {
            throw new BadRequestException("Section content must be a JSON object.");
        }

        var appVersion = await GetVersionAsync(projectSlug, platformSlug, version);
        await configs.SaveSectionAsync(appVersion.Id, new ConfigSection { Key = key, Content = content.Clone() });
    }

    public async Task DeleteSectionAsync(string projectSlug, string platformSlug, string version, string key)
    {
        ValidateKey(key);
        var appVersion = await GetVersionAsync(projectSlug, platformSlug, version);
        if (!await configs.DeleteSectionAsync(appVersion.Id, key))
        {
            throw new NotFoundException($"Section '{key}' not found.");
        }
    }

    private async Task<AppVersion> GetVersionAsync(string projectSlug, string platformSlug, string version)
    {
        var appVersion = await projects.GetVersionBySlugAsync(projectSlug, platformSlug, version);
        if (appVersion is null)
        {
            throw new NotFoundException($"Version '{version}' not found.");
        }

        return appVersion;
    }

    private static void ValidateKey(string key)
    {
        if (key.Length is < 1 or > 64 || !Regex.IsMatch(key, "\\A[a-z0-9-]+\\z"))
        {
            throw new BadRequestException("Section key must be 1–64 lowercase Latin letters, digits or hyphens.");
        }
    }
}
