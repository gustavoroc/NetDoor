using System;
using System.Threading.Tasks;
using NetDoor.Core.Actors;
using NetDoor.Core.Persistence;
using Xunit;
using NetDoor.Tests.Helpers;

namespace NetDoor.Tests;

public class ActorTests
{
    private readonly ActorSystem _system;

    public ActorTests()
    {
        var persistence = new InMemoryActorPersistence();
        _system = new ActorSystem(new ActorSystemConfig(), persistence);
    }

    [Fact]
    public async Task Actor_ShouldReceiveMessage()
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
}

public class TestActor : Actor
{
    public object? LastMessage { get; private set; }

    protected override async Task HandleMessageAsync(object message)
    {
        LastMessage = message;
        await Task.CompletedTask;
    }
}

public class TestMessage
{
    public string Content { get; }

    public TestMessage(string content)
    {
        Content = content;
    }
} 