using System.Collections.Concurrent;

namespace NetDoor.Core.Persistence;

public class InMemoryActorPersistence : IActorPersistence
{
    private readonly ConcurrentDictionary<string, List<object>> _events = new();
    private readonly ConcurrentDictionary<string, (object State, long Version)> _snapshots = new();

    public Task SaveEventAsync(string actorId, object @event)
    {
        var events = _events.GetOrAdd(actorId, _ => new List<object>());
        events.Add(@event);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<object>> GetEventsAsync(string actorId, long fromVersion = 0)
    {
        if (_events.TryGetValue(actorId, out var events))
        {
            return Task.FromResult(events.Skip((int)fromVersion));
        }
        return Task.FromResult(Enumerable.Empty<object>());
    }

    public Task SaveSnapshotAsync(string actorId, object state, long version)
    {
        _snapshots.AddOrUpdate(actorId, (state, version), (_, _) => (state, version));
        return Task.CompletedTask;
    }

    public Task<(object? State, long Version)?> GetLatestSnapshotAsync(string actorId)
    {
        if (_snapshots.TryGetValue(actorId, out var snapshot))
        {
            return Task.FromResult<(object? State, long Version)?>((snapshot.State, snapshot.Version));
        }
        return Task.FromResult<(object? State, long Version)?>(null);
    }

    public Task DeleteActorAsync(string actorId)
    {
        _events.TryRemove(actorId, out _);
        _snapshots.TryRemove(actorId, out _);
        return Task.CompletedTask;
    }
} 