using System;

namespace NetDoor.Core.Actors;

public class DefaultSupervisorStrategy : IActorSupervisorStrategy
{
    public void HandleFailure(ActorRef actorRef, Exception exception)
    {
        // Log the error and restart the actor
        Console.WriteLine($"Actor {actorRef.Id} failed with error: {exception.Message}");
        actorRef.Stop();
    }

    public void HandleError(Actor actor, Exception exception, object message)
    {
        // Log the error and restart the actor
        Console.WriteLine($"Actor failed while processing message {message.GetType().Name} with error: {exception.Message}");
        actor.Stop();
    }
} 