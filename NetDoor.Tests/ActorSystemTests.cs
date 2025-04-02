using System;
using System.Threading.Tasks;
using NetDoor.Core.Actors;
using NetDoor.Core.Persistence;
using NetDoor.Tests.Helpers;
using Xunit;

namespace NetDoor.Tests;

public class ActorSystemTests
{
    private readonly ActorSystem _system;

    public ActorSystemTests()
    {
        var persistence = new InMemoryActorPersistence();
        _system = new ActorSystem(new ActorSystemConfig(), persistence);
    }

    [Fact]
    public async Task ActorSystem_ShouldCreateAndManageActors()
    {
        // Arrange
        var actor = _system.ActorOf<TestActor>("test-1");
        var message = "test message";

        // Act
        await actor.TellAsync(message);

        // Assert
        var testActor = (TestActor)actor.UnderlyingActor;
        await TestHelpers.WaitForCondition(() => testActor.LastMessage?.ToString() == message);
    }

    [Fact]
    public async Task ActorSystem_ShouldHandleMultipleActors()
    {
        // Arrange
        var actor1 = _system.ActorOf<TestActor>("test-1");
        var actor2 = _system.ActorOf<TestActor>("test-2");

        // Act
        await actor1.TellAsync("message 1");
        await actor2.TellAsync("message 2");

        // Assert
        var testActor1 = (TestActor)actor1.UnderlyingActor;
        var testActor2 = (TestActor)actor2.UnderlyingActor;

        await TestHelpers.WaitForCondition(() => testActor1.LastMessage?.ToString() == "message 1");
        await TestHelpers.WaitForCondition(() => testActor2.LastMessage?.ToString() == "message 2");

        // Stop actors
        _system.Stop(actor1);
        _system.Stop(actor2);
    }
} 