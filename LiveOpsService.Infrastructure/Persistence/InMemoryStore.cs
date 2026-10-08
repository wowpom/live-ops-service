using LiveOpsService.Domain.Entities;

namespace LiveOpsService.Infrastructure.Persistence;

public class InMemoryStore
{
    internal object SyncRoot { get; } = new();
    internal Dictionary<string, Project> Projects { get; } = new();
    internal Dictionary<Guid, ConfigEntry> Configs { get; } = new();
}
