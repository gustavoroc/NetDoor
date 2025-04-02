namespace NetDoor.Core.Actors;

public interface IPersistentActor
{
    string Id { get; }
    long Version { get; }
    object State { get; }
    void ApplyEvent(object @event);
    void ApplySnapshot(object state, long version);
} 