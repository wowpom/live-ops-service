using System.Net;
using Xunit;

namespace LiveOpsService.Tests;

public class PublicationTests
{
    [Fact]
    public async Task FullScenarioKeepsDraftChangesInvisibleUntilNextPublication()
    {
        await using var api = new ApiTestContext();
        await api.CreateVersionAsync();
        await api.CreateDraftAsync();
        await api.PutSectionAsync("gameplay", """{"coins":100,"nested":{"energy":20},"items":[1,2]}""");
        await api.PutSectionAsync("shop", """{"price":25}""");

        using var unpublished = await api.Client.GetAsync(ApiTestContext.ClientPath());
        await ApiTestContext.AssertProblemAsync(unpublished, HttpStatusCode.NotFound);

        var before = DateTimeOffset.UtcNow;
        var first = await api.PublishAsync();
        Assert.Equal("1.0.0", first.Version);
        Assert.Equal(1, first.Revision);
        Assert.Equal(TimeSpan.Zero, first.PublishedAt.Offset);
        Assert.InRange(first.PublishedAt, before, DateTimeOffset.UtcNow);
        Assert.Equal(20, first.Sections["gameplay"].GetProperty("nested").GetProperty("energy").GetInt32());
        Assert.Equal(2, first.Sections["gameplay"].GetProperty("items").GetArrayLength());

        await api.PutSectionAsync("gameplay", """{"coins":200,"nested":{"energy":40},"items":[3]}""");
        using var removed = await api.Client.DeleteAsync(ApiTestContext.DraftPath() + "/sections/shop");
        Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);

        var stillFirst = await api.ReadPublishedAsync();
        Assert.Equal(1, stillFirst.Revision);
        Assert.Equal(first.PublishedAt, stillFirst.PublishedAt);
        Assert.Equal(100, stillFirst.Sections["gameplay"].GetProperty("coins").GetInt32());
        Assert.Equal(20, stillFirst.Sections["gameplay"].GetProperty("nested").GetProperty("energy").GetInt32());
        Assert.Equal(2, stillFirst.Sections["gameplay"].GetProperty("items").GetArrayLength());
        Assert.Equal(25, stillFirst.Sections["shop"].GetProperty("price").GetInt32());

        var second = await api.PublishAsync();
        Assert.Equal(2, second.Revision);
        var client = await api.ReadPublishedAsync();
        Assert.Equal(2, client.Revision);
        Assert.Equal(200, client.Sections["gameplay"].GetProperty("coins").GetInt32());
        Assert.Equal(40, client.Sections["gameplay"].GetProperty("nested").GetProperty("energy").GetInt32());
        Assert.False(client.Sections.ContainsKey("shop"));
        Assert.Equal(100, first.Sections["gameplay"].GetProperty("coins").GetInt32());
    }

    [Fact]
    public async Task EverySuccessfulPublicationCreatesARevisionEvenWhenContentIsUnchanged()
    {
        await using var api = new ApiTestContext();
        await api.CreateVersionAsync();
        await api.CreateDraftAsync();
        await api.PutSectionAsync("gameplay", """{"coins":100}""");

        var first = await api.PublishAsync();
        var second = await api.PublishAsync();

        Assert.Equal(1, first.Revision);
        Assert.Equal(2, second.Revision);
        Assert.Equal(first.Sections["gameplay"].GetRawText(), second.Sections["gameplay"].GetRawText());
    }

    [Fact]
    public async Task MissingOrEmptyDraftCannotBePublishedAndFailedPublicationPreservesSnapshot()
    {
        await using var api = new ApiTestContext();
        await api.CreateVersionAsync();
        var publishPath = ApiTestContext.DraftPath() + "/publish";
        using var missing = await api.Client.PostAsync(publishPath, null);
        await ApiTestContext.AssertProblemAsync(missing, HttpStatusCode.NotFound);

        await api.CreateDraftAsync();
        using var empty = await api.Client.PostAsync(publishPath, null);
        await ApiTestContext.AssertProblemAsync(empty, HttpStatusCode.BadRequest);
        using var unpublished = await api.Client.GetAsync(ApiTestContext.ClientPath());
        await ApiTestContext.AssertProblemAsync(unpublished, HttpStatusCode.NotFound);

        await api.PutSectionAsync("gameplay", """{"coins":100}""");
        await api.PublishAsync();
        using var deleted = await api.Client.DeleteAsync(ApiTestContext.DraftPath() + "/sections/gameplay");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        using var rejected = await api.Client.PostAsync(publishPath, null);
        await ApiTestContext.AssertProblemAsync(rejected, HttpStatusCode.BadRequest);
        var previous = await api.ReadPublishedAsync();
        Assert.Equal(1, previous.Revision);
        Assert.Equal(100, previous.Sections["gameplay"].GetProperty("coins").GetInt32());

        await api.PutSectionAsync("gameplay", """{"coins":200}""");
        Assert.Equal(2, (await api.PublishAsync()).Revision);
    }

    [Theory]
    [InlineData("other", "android", "1.0.0")]
    [InlineData("game", "ios", "1.0.0")]
    [InlineData("game", "android", "2.0.0")]
    public async Task PublicationsAreIsolatedByProjectPlatformAndVersion(string project, string platform, string version)
    {
        await using var api = new ApiTestContext();
        await api.CreateVersionAsync();
        await api.CreateDraftAsync();
        await api.PutSectionAsync("gameplay", """{"coins":100}""");
        await api.PublishAsync();

        await api.CreateVersionAsync(project, platform, version, createProject: project != "game",
            createPlatform: project != "game" || platform != "android");
        await api.CreateDraftAsync(project, platform, version);
        await api.PutSectionAsync("gameplay", """{"coins":300}""", project, platform, version);
        using var unpublished = await api.Client.GetAsync(ApiTestContext.ClientPath(project, platform, version));
        await ApiTestContext.AssertProblemAsync(unpublished, HttpStatusCode.NotFound);

        var other = await api.PublishAsync(project, platform, version);
        Assert.Equal(1, other.Revision);
        await api.PublishAsync();
        var original = await api.ReadPublishedAsync();
        var separate = await api.ReadPublishedAsync(project, platform, version);
        Assert.Equal(2, original.Revision);
        Assert.Equal(100, original.Sections["gameplay"].GetProperty("coins").GetInt32());
        Assert.Equal(1, separate.Revision);
        Assert.Equal(300, separate.Sections["gameplay"].GetProperty("coins").GetInt32());
    }

    [Theory]
    [InlineData("missing", "android", "1.0.0")]
    [InlineData("game", "missing", "1.0.0")]
    [InlineData("game", "android", "9.9.9")]
    public async Task UnknownParentReturns404ForPublicationAndClient(string project, string platform, string version)
    {
        await using var api = new ApiTestContext();
        await api.CreateVersionAsync();
        using var published = await api.Client.PostAsync(ApiTestContext.DraftPath(project, platform, version) + "/publish", null);
        await ApiTestContext.AssertProblemAsync(published, HttpStatusCode.NotFound);
        using var client = await api.Client.GetAsync(ApiTestContext.ClientPath(project, platform, version));
        await ApiTestContext.AssertProblemAsync(client, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task IndependentTestServersDoNotShareSingletonState()
    {
        await using var first = new ApiTestContext();
        await using var second = new ApiTestContext();
        await first.CreateVersionAsync();
        await first.CreateDraftAsync();
        await first.PutSectionAsync("gameplay", """{"coins":100}""");
        await first.PublishAsync();

        using var unknown = await second.Client.GetAsync(ApiTestContext.ClientPath());
        await ApiTestContext.AssertProblemAsync(unknown, HttpStatusCode.NotFound);
        await second.CreateVersionAsync();
        await second.CreateDraftAsync();
        await second.PutSectionAsync("gameplay", """{"coins":200}""");
        await second.PublishAsync();

        Assert.Equal(100, (await first.ReadPublishedAsync()).Sections["gameplay"].GetProperty("coins").GetInt32());
        Assert.Equal(200, (await second.ReadPublishedAsync()).Sections["gameplay"].GetProperty("coins").GetInt32());
    }
}
