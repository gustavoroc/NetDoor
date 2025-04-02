using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using NetDoor.Core.Persistence;

namespace NetDoor.Core.Actors;

public class ActorSystem
{
    private readonly ActorSystemConfig _config;
    private readonly IActorSupervisorStrategy _supervisorStrategy;
    private readonly IActorPersistence _persistence;
    private readonly Dictionary<string, ActorRef> _actors = new();

    public ActorSystem(ActorSystemConfig config, IActorPersistence persistence, IActorSupervisorStrategy? supervisorStrategy = null)
    {
        _config = config;
        _persistence = persistence;
        _supervisorStrategy = supervisorStrategy ?? new DefaultSupervisorStrategy();
    }

    public async Task<ActorRef> ActorOfAsync<T>(string id) where T : Actor
    {
        if (_actors.TryGetValue(id, out var existingActor))
        {
            return existingActor;
        }

        var actor = typeof(T).IsSubclassOf(typeof(PersistentActor))
            ? (T)Activator.CreateInstance(typeof(T), id, _persistence)!
            : (T)Activator.CreateInstance(typeof(T))!;

        var actorRef = new ActorRef(actor, this);
        _actors[id] = actorRef;

        if (actor is PersistentActor)
        {
            await actorRef.TellAsync(RecoverCommand.Instance);
        }

        return actorRef;
    }

    public ActorRef ActorOf<T>(string id) where T : Actor
    {
        return ActorOfAsync<T>(id).GetAwaiter().GetResult();
    }

    public ActorRef ActorOf<T>(string id, params object[] args) where T : Actor
    {
        if (_actors.TryGetValue(id, out var existingActor))
        {
            return existingActor;
        }

        var actor = (T)Activator.CreateInstance(typeof(T), args)!;
        var actorRef = new ActorRef(actor, this);
        _actors[id] = actorRef;
        return actorRef;
    }

    public void Stop(ActorRef actorRef)
    {
        if (_actors.Remove(actorRef.Id))
        {
            actorRef.Stop();
        }
    }

    public void StopAll()
    {
        foreach (var actorRef in _actors.Values)
        {
            actorRef.Stop();
        }
        _actors.Clear();
    }

    internal void HandleFailure(ActorRef actorRef, Exception exception)
    {
        _supervisorStrategy.HandleFailure(actorRef, exception);
    }

    internal IActorPersistence Persistence => _persistence;

    internal IActorSupervisorStrategy SupervisorStrategy => _supervisorStrategy;

    internal ActorSystemConfig Config => _config;
} 