using System.Text.Json;

namespace LiveOpsService.Models;

public record ConfigResponse(Guid Id, Dictionary<string, JsonElement> Sections);
