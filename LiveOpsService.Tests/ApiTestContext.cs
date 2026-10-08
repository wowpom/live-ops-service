using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LiveOpsService.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LiveOpsService.Tests;

internal sealed class ApiTestContext : IAsyncDisposable
{
    // Each test owns a factory and therefore its own singleton InMemoryStore.
    private readonly WebApplicationFactory<Program> _factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder => builder.UseEnvironment("Testing").ConfigureLogging(logging => logging.ClearProviders()));

    public HttpClient Client { get; }
    public IServiceProvider Services => _factory.Services;

    public ApiTestContext()
    {
        Client = _factory.CreateClient();
    }

    public static string VersionPath(string project = "game", string platform = "android", string version = "1.0.0") =>
        $"/api/admin/projects/{project}/platforms/{platform}/versions/{version}";

    public static string DraftPath(string project = "game", string platform = "android", string version = "1.0.0") =>
        VersionPath(project, platform, version) + "/config";

    public static string ClientPath(string project = "game", string platform = "android", string version = "1.0.0") =>
        $"/api/config/{project}/{platform}/{version}";

    public async Task CreateVersionAsync(string project = "game", string platform = "android", string version = "1.0.0",
        bool createProject = true, bool createPlatform = true)
    {
        if (createProject)
        {
            using var response = await Client.PostAsJsonAsync("/api/admin/projects", new { slug = project, name = project });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        if (createPlatform)
        {
            using var response = await Client.PostAsJsonAsync($"/api/admin/projects/{project}/platforms", new { slug = platform, name = platform });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        using var created = await Client.PostAsJsonAsync($"/api/admin/projects/{project}/platforms/{platform}/versions", new { version });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
    }

    public async Task CreateDraftAsync(string project = "game", string platform = "android", string version = "1.0.0")
    {
        using var response = await Client.PostAsync(DraftPath(project, platform, version), null);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        using var read = await Client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
    }

    public async Task PutSectionAsync(string key, string json, string project = "game", string platform = "android", string version = "1.0.0")
    {
        using var body = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await Client.PutAsync(DraftPath(project, platform, version) + $"/sections/{key}", body);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    public async Task<PublishedConfigResponse> PublishAsync(string project = "game", string platform = "android", string version = "1.0.0")
    {
        using var response = await Client.PostAsync(DraftPath(project, platform, version) + "/publish", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return Assert.IsType<PublishedConfigResponse>(await response.Content.ReadFromJsonAsync<PublishedConfigResponse>());
    }

    public async Task<PublishedConfigResponse> ReadPublishedAsync(string project = "game", string platform = "android", string version = "1.0.0")
    {
        using var response = await Client.GetAsync(ClientPath(project, platform, version));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return Assert.IsType<PublishedConfigResponse>(await response.Content.ReadFromJsonAsync<PublishedConfigResponse>());
    }

    public static async Task<JsonElement> AssertProblemAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal((int)expected, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("title").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("traceId").GetString()));
        Assert.False(problem.TryGetProperty("stackTrace", out _));
        return problem;
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
    }
}
