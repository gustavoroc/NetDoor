using NetDoor.Core.Actors;
using NetDoor.Core.Persistence;

namespace NetDoor.Tests.Examples;

public class CounterActor : PersistentActor
{
    private int _count;

    public CounterActor(string id, IActorPersistence persistence) 
        : base(id, persistence)
    {
        _count = 0;
    }

    protected override object CreateInitialState()
    {
        return 0;
    }

    protected override async Task HandlePersistentMessageAsync(object message)
    {
        switch (message)
        {
            case IncrementCommand:
                await PersistEventAsync(new IncrementedEvent());
                break;
            case DecrementCommand:
                await PersistEventAsync(new DecrementedEvent());
                break;
            case GetCountCommand:
                SetResponse(new CountResponse(_count));
                break;
        }
    }

    protected override void OnEvent(object @event)
    {
        switch (@event)
        {
            case IncrementedEvent:
                _count++;
                break;
            case DecrementedEvent:
                _count--;
                break;
        }
        _state = _count;
    }

    protected override void OnSnapshot(object snapshot)
    {
        if (snapshot is int count)
        {
            _count = count;
        }
    }
}

public class IncrementCommand
{
    public static IncrementCommand Instance = new();
}

public class DecrementCommand
{
    public static DecrementCommand Instance = new();
}

public class GetCountCommand
{
    public static GetCountCommand Instance = new();
}

public class IncrementedEvent { }
public class DecrementedEvent { }
public record CountResponse(int Count); 