using System.Text.Json;
using LiveOpsService.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LiveOpsService.Tests;

public class RepositorySnapshotTests
{
    [Fact]
    public async Task MutatingReturnedEntitiesCannotChangeStoredDraftOrPublication()
    {
        await using var api = new ApiTestContext();
        await api.CreateVersionAsync();
        await api.CreateDraftAsync();
        await api.PutSectionAsync("gameplay", """{"coins":100,"nested":{"energy":20}}""");
        await api.PublishAsync();
        var projects = api.Services.GetRequiredService<IProjectRepository>();
        var configs = api.Services.GetRequiredService<IConfigRepository>();
        var version = Assert.IsType<Domain.Entities.AppVersion>(await projects.GetVersionBySlugAsync("game", "android", "1.0.0"));
        Assert.NotNull(version.ConfigId);
        var draft = Assert.IsType<Domain.Entities.ConfigEntry>(await configs.GetByVersionIdAsync(version.Id));
        Assert.Equal(draft.Id, version.ConfigId);

        using var replacement = JsonDocument.Parse("""{"coins":999}""");
        draft.Sections["gameplay"].Content = replacement.RootElement.Clone();
        draft.Sections.Clear();
        var snapshot = Assert.IsType<Domain.Entities.PublishedConfig>(await configs.GetPublishedAsync(version.Id));
        if (snapshot.Sections is IDictionary<string, JsonElement> sections && !sections.IsReadOnly)
        {
            sections["gameplay"] = replacement.RootElement.Clone();
            sections.Clear();
        }

        var published = await api.ReadPublishedAsync();
        Assert.Equal(100, published.Sections["gameplay"].GetProperty("coins").GetInt32());
        Assert.Equal(20, published.Sections["gameplay"].GetProperty("nested").GetProperty("energy").GetInt32());
        var republished = await api.PublishAsync();
        Assert.Equal(2, republished.Revision);
        Assert.Equal(100, republished.Sections["gameplay"].GetProperty("coins").GetInt32());
    }
}
