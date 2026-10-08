using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace LiveOpsService.Tests;

public class ValidationTests
{
    [Theory]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("{")]
    [InlineData("{\"slug\":\"UPPER\",\"name\":\"Demo\"}")]
    [InlineData("{\"slug\":\"valid\",\"name\":\"   \"}")]
    public async Task InvalidProjectRequestReturns400WithoutCreatingData(string json)
    {
        await using var api = new ApiTestContext();
        using var body = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await api.Client.PostAsync("/api/admin/projects", body);
        await ApiTestContext.AssertProblemAsync(response, HttpStatusCode.BadRequest);

        var projects = await api.Client.GetFromJsonAsync<JsonElement>("/api/admin/projects");
        Assert.Equal(0, projects.GetArrayLength());
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    [InlineData("null")]
    [InlineData("1")]
    [InlineData("{")]
    public async Task InvalidSectionUpdatePreservesDraftAndPublication(string json)
    {
        await using var api = new ApiTestContext();
        await api.CreateVersionAsync();
        await api.CreateDraftAsync();
        await api.PutSectionAsync("gameplay", """{"coins":100}""");
        await api.PublishAsync();

        using var body = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await api.Client.PutAsync(ApiTestContext.DraftPath() + "/sections/gameplay", body);
        await ApiTestContext.AssertProblemAsync(response, HttpStatusCode.BadRequest);
        var draft = await api.Client.GetFromJsonAsync<JsonElement>(ApiTestContext.DraftPath());
        Assert.Equal(100, draft.GetProperty("sections").GetProperty("gameplay").GetProperty("coins").GetInt32());
        Assert.Equal(100, (await api.ReadPublishedAsync()).Sections["gameplay"].GetProperty("coins").GetInt32());
        Assert.Equal(2, (await api.PublishAsync()).Revision);
    }

    [Fact]
    public async Task RequestFieldErrorsAndDuplicateConflictUseProblemDetails()
    {
        await using var api = new ApiTestContext();
        using var invalid = await api.Client.PostAsJsonAsync("/api/admin/projects", new { slug = "BAD", name = " " });
        var problem = await ApiTestContext.AssertProblemAsync(invalid, HttpStatusCode.BadRequest);
        Assert.True(problem.GetProperty("errors").TryGetProperty("slug", out _));
        Assert.True(problem.GetProperty("errors").TryGetProperty("name", out _));

        await api.CreateVersionAsync();
        await api.CreateDraftAsync();
        using var duplicate = await api.Client.PostAsync(ApiTestContext.DraftPath(), null);
        await ApiTestContext.AssertProblemAsync(duplicate, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task SectionSizeLimitAcceptsBoundaryAndRejectsOversizedBodyWithoutChangingDraft()
    {
        await using var api = new ApiTestContext();
        await api.CreateVersionAsync();
        await api.CreateDraftAsync();
        var json = "{\"value\":\"" + new string('x', 65536 - 12) + "\"}";
        await api.PutSectionAsync("size", json);

        using var body = new StringContent(json + " ", Encoding.UTF8, "application/json");
        using var response = await api.Client.PutAsync(ApiTestContext.DraftPath() + "/sections/size", body);
        await ApiTestContext.AssertProblemAsync(response, HttpStatusCode.RequestEntityTooLarge);
        var draft = await api.Client.GetFromJsonAsync<JsonElement>(ApiTestContext.DraftPath());
        Assert.Equal(65536 - 12, draft.GetProperty("sections").GetProperty("size").GetProperty("value").GetString()?.Length);
    }
}
