using System.Threading.Tasks;
using Xunit;
using NetDoor.Core.Actors;
using NetDoor.Core.Persistence;
using NetDoor.Tests.Helpers;

namespace NetDoor.Tests;

public class ActorCommunicationTests
{
    private readonly ActorSystem _system;

    public ActorCommunicationTests()
    {
        var persistence = new InMemoryActorPersistence();
        _system = new ActorSystem(new ActorSystemConfig(), persistence);
    }

    [Fact]
    public async Task Actors_ShouldCommunicateAsynchronously()
    {
        // Arrange
        var sender = _system.ActorOf<SenderActor>("sender");
        var receiver = _system.ActorOf<ReceiverActor>("receiver");

        // Act
        await sender.TellAsync(new SendMessageCommand(receiver, "Hello!"));

        // Assert
        var receiverActor = (ReceiverActor)receiver.UnderlyingActor;
        await TestHelpers.WaitForCondition(() => receiverActor.LastMessage == "Hello!");
    }

    [Fact]
    public async Task Actor_ShouldReceiveResponse()
    {
        // Arrange
        var sender = _system.ActorOf<SenderActor>("sender");
        var receiver = _system.ActorOf<ReceiverActor>("receiver");

        // Act
        var response = await sender.AskAsync<string>(new AskMessageCommand(receiver, "Hello!"));

        // Assert
        Assert.Equal("Hello back!", response);
    }
}

public class SenderActor : Actor
{
    protected override async Task HandleMessageAsync(object message)
    {
        switch (message)
        {
            case SendMessageCommand cmd:
                await cmd.Target.TellAsync(cmd.Message);
                break;
            case AskMessageCommand cmd:
                var response = await Context.AskAsync<string>(cmd.Target, cmd.Message);
                SetResponse(response);
                break;
        }
    }
}

public class ReceiverActor : Actor
{
    public string? LastMessage { get; private set; }

    protected override async Task HandleMessageAsync(object message)
    {
        if (message is string text)
        {
            LastMessage = text;
            SetResponse("Hello back!");
        }
        await Task.CompletedTask;
    }
}

public record SendMessageCommand(ActorRef Target, string Message);
public record AskMessageCommand(ActorRef Target, string Message); 