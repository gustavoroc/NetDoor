namespace NetDoor.Core.Persistence;

public interface IActorPersistence
{
    Task SaveEventAsync(string actorId, object @event);
    Task<IEnumerable<object>> GetEventsAsync(string actorId, long fromVersion = 0);
    Task SaveSnapshotAsync(string actorId, object state, long version);
    Task<(object? State, long Version)?> GetLatestSnapshotAsync(string actorId);
    Task DeleteActorAsync(string actorId);
} 