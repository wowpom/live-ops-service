using System.Text.Json;

namespace LiveOpsService.Domain.Entities;

public class ConfigSection
{
    public required string Key { get; set; }
    public required JsonElement Content { get; set; }
}
