using System.Text.Json;

namespace LiveOpsService.Domain.Entities;

public class PublishedConfig
{
    public long Revision { get; init; }
    public DateTimeOffset PublishedAt { get; init; }
    public required IReadOnlyDictionary<string, JsonElement> Sections { get; init; }
}
