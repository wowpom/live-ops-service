using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace LiveOpsService.Tests;

public class ConcurrencyTests
{
    [Theory]
    [InlineData("project")]
    [InlineData("platform")]
    [InlineData("version")]
    [InlineData("draft")]
    public async Task ConcurrentDuplicateCreationHasOneWinner(string resource)
    {
        await using var api = new ApiTestContext();
        await api.CreateVersionAsync();
        var path = resource switch
        {
            "project" => "/api/admin/projects",
            "platform" => "/api/admin/projects/game/platforms",
            "version" => "/api/admin/projects/game/platforms/android/versions",
            _ => ApiTestContext.DraftPath()
        };

        async Task<HttpStatusCode> CreateAsync()
        {
            using var response = resource switch
            {
                "project" or "platform" => await api.Client.PostAsJsonAsync(path, new { slug = "duplicate", name = "Duplicate" }),
                "version" => await api.Client.PostAsJsonAsync(path, new { version = "2.0.0" }),
                _ => await api.Client.PostAsync(path, null)
            };
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                await ApiTestContext.AssertProblemAsync(response, HttpStatusCode.Conflict);
            }

            return response.StatusCode;
        }

        var responses = await Task.WhenAll(Task.Run(CreateAsync), Task.Run(CreateAsync));
        Assert.Single(responses, status => status == HttpStatusCode.Created);
        Assert.Single(responses, status => status == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ConcurrentPublicationsHaveUniqueConsecutiveRevisions()
    {
        await using var api = new ApiTestContext();
        await api.CreateVersionAsync();
        await api.CreateDraftAsync();
        await api.PutSectionAsync("gameplay", """{"coins":100}""");

        var publications = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => Task.Run(() => api.PublishAsync())));

        Assert.Equal(Enumerable.Range(1, 20).Select(value => (long)value), publications.Select(item => item.Revision).Order());
        Assert.Equal(20, (await api.ReadPublishedAsync()).Revision);
        Assert.All(publications, publication => Assert.Equal(100, publication.Sections["gameplay"].GetProperty("coins").GetInt32()));
    }

    [Fact]
    public async Task ReadsAndPublicationSeeCompleteSectionsDuringConcurrentDraftChanges()
    {
        await using var api = new ApiTestContext();
        await api.CreateVersionAsync();
        await api.CreateDraftAsync();
        await api.PutSectionAsync("gameplay", """{"generation":0,"nested":{"generation":0}}""");
        await api.PutSectionAsync("shop", """{"price":25}""");
        await api.PublishAsync();

        var edits = Task.Run(async () =>
        {
            for (var generation = 1; generation <= 30; generation++)
            {
                var json = JsonSerializer.Serialize(new { generation, nested = new { generation } });
                await api.PutSectionAsync("gameplay", json);
            }
        });
        var publications = Task.Run(async () =>
        {
            for (var index = 0; index < 30; index++)
            {
                var publication = await api.PublishAsync();
                var section = publication.Sections["gameplay"];
                Assert.Equal(section.GetProperty("generation").GetInt32(), section.GetProperty("nested").GetProperty("generation").GetInt32());
                Assert.Equal(25, publication.Sections["shop"].GetProperty("price").GetInt32());
            }
        });
        var reads = Task.Run(async () =>
        {
            for (var index = 0; index < 30; index++)
            {
                var publication = await api.ReadPublishedAsync();
                var section = publication.Sections["gameplay"];
                Assert.Equal(section.GetProperty("generation").GetInt32(), section.GetProperty("nested").GetProperty("generation").GetInt32());
                Assert.Equal(25, publication.Sections["shop"].GetProperty("price").GetInt32());
            }
        });

        await Task.WhenAll(edits, publications, reads);
        Assert.Equal(31, (await api.ReadPublishedAsync()).Revision);
    }
}
