namespace NetDoor.Core.Actors;

public interface IActorSupervisorStrategy
{
    void HandleFailure(ActorRef actorRef, Exception exception);
    void HandleError(Actor actor, Exception exception, object message);
} 