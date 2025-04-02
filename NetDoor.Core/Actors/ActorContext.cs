namespace NetDoor.Core.Actors;

public class ActorContext
{
    private readonly ActorSystem _system;
    private readonly ActorRef _self;
    private readonly string _id;
    private readonly IActorSupervisorStrategy _supervisorStrategy;

    public ActorContext(ActorRef self, string id, ActorSystem system)
    {
        _self = self;
        _id = id;
        _system = system;
        _supervisorStrategy = system.SupervisorStrategy;
    }

    public ActorRef Self => _self;

    public string Id => _id;

    public ActorSystem System => _system;

    public IActorSupervisorStrategy SupervisorStrategy => _supervisorStrategy;

    public async Task TellAsync(ActorRef target, object message)
    {
        await target.TellAsync(message);
    }

    public async Task<T> AskAsync<T>(ActorRef target, object message)
    {
        return await target.AskAsync<T>(message);
    }

    public void Stop()
    {
        _system.Stop(_self);
    }
} 