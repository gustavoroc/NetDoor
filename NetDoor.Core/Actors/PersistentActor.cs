using System.Threading.Channels;
using NetDoor.Core.Persistence;

namespace NetDoor.Core.Actors;

public abstract class PersistentActor : Actor, IPersistentActor
{
    private readonly string _id;
    private readonly IActorPersistence _persistence;
    private long _version;
    protected object _state;

    protected PersistentActor(string id, IActorPersistence persistence)
    {
        _id = id;
        _persistence = persistence;
        _version = 0;
        _state = CreateInitialState();
    }

    public string Id => _id;

    public long Version => _version;

    public object State => _state;

    protected abstract object CreateInitialState();

    protected override async Task HandleMessageAsync(object message)
    {
        switch (message)
        {
            case RecoverCommand:
                await RecoverAsync();
                break;
            default:
                await HandlePersistentMessageAsync(message);
                break;
        }
    }

    protected abstract Task HandlePersistentMessageAsync(object message);

    private async Task RecoverAsync()
    {
        // Reset state to initial
        _state = CreateInitialState();
        _version = 0;
        OnSnapshot(_state);

        var snapshot = await _persistence.GetLatestSnapshotAsync(_id);
        if (snapshot.HasValue)
        {
            ApplySnapshot(snapshot.Value.State!, snapshot.Value.Version);
        }

        var events = await _persistence.GetEventsAsync(_id, _version);
        foreach (var @event in events)
        {
            ApplyEvent(@event);
        }
    }

    protected async Task PersistEventAsync(object @event)
    {
        await _persistence.SaveEventAsync(_id, @event);
        ApplyEvent(@event);

        if (_version % Context.System.Config.SnapshotThreshold == 0)
        {
            await _persistence.SaveSnapshotAsync(_id, _state, _version);
        }
    }

    public void ApplyEvent(object @event)
    {
        OnEvent(@event);
        _version++;
    }

    public void ApplySnapshot(object state, long version)
    {
        _state = state;
        _version = version;
        OnSnapshot(state);
    }

    protected abstract void OnEvent(object @event);
    protected abstract void OnSnapshot(object snapshot);
}

public class RecoverCommand
{
    public static RecoverCommand Instance = new();
} 