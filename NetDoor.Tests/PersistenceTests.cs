using NetDoor.Core.Actors;
using NetDoor.Core.Persistence;
using NetDoor.Tests.Examples;
using NetDoor.Tests.Helpers;

namespace NetDoor.Tests;

public class PersistenceTests
{
    private readonly ActorSystem _system;
    private readonly IActorPersistence _persistence;

    public PersistenceTests()
    {
        _persistence = new InMemoryActorPersistence();
        _system = new ActorSystem(new ActorSystemConfig(), _persistence);
    }

    private async Task WaitForCount(ActorRef actor, int expectedCount, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        
        while (true)
        {
            try
            {
                var response = await actor.AskAsync<CountResponse>(GetCountCommand.Instance);
                if (response.Count == expectedCount)
                {
                    return;
                }
            }
            catch
            {
                // Ignore errors and continue waiting
            }

            if (DateTime.UtcNow - startTime > timeout)
            {
                throw new TimeoutException($"Count did not reach {expectedCount} within {timeout.Value.TotalSeconds} seconds");
            }
            
            await Task.Delay(50);
        }
    }

    [Fact]
    public async Task PersistentActor_ShouldRecoverStateAfterRestart()
    {
        // Arrange
        var actorRef = await _system.ActorOfAsync<CounterActor>("counter-1");
        
        // Act - Increment counter
        await actorRef.TellAsync(IncrementCommand.Instance);
        await actorRef.TellAsync(IncrementCommand.Instance);
        
        // Wait for processing
        await WaitForCount(actorRef, 2);

        // Create new actor with same ID to simulate restart
        var restartedActorRef = await _system.ActorOfAsync<CounterActor>("counter-1");
        
        // Assert - State should be recovered
        var recoveredCount = await restartedActorRef.AskAsync<CountResponse>(GetCountCommand.Instance);
        Assert.Equal(2, recoveredCount.Count);
    }

    [Fact]
    public async Task PersistentActor_ShouldHandleMultipleEvents()
    {
        // Arrange
        var actorRef = await _system.ActorOfAsync<CounterActor>("counter-2");
        
        // Act - Increment and decrement
        await actorRef.TellAsync(IncrementCommand.Instance);
        await actorRef.TellAsync(IncrementCommand.Instance);
        await actorRef.TellAsync(DecrementCommand.Instance);
        
        // Wait for processing
        await WaitForCount(actorRef, 1);

        // Assert
        var finalCount = await actorRef.AskAsync<CountResponse>(GetCountCommand.Instance);
        Assert.Equal(1, finalCount.Count);
    }
} 