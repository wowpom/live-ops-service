using System.Text.Json;

namespace LiveOpsService.Models;

public record PublishedConfigResponse(
    string Version,
    long Revision,
    DateTimeOffset PublishedAt,
    IReadOnlyDictionary<string, JsonElement> Sections);
