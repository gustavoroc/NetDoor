# NetDoor Actor Framework

NetDoor is a lightweight, high-performance actor framework for .NET applications, inspired by Akka.NET. It provides a simple yet powerful way to build concurrent and distributed systems using the actor model.

## Features

- 🚀 Asynchronous message processing
- 🔄 Actor lifecycle management
- 🛡️ Supervision strategies for error handling
- 📊 Built-in metrics and monitoring
- 🔍 Actor system configuration
- 💡 Clean and simple API

## Installation

```bash
dotnet add package NetDoor.Core
```

## Quick Start

Here's a simple example of creating and using actors:

```csharp
// Create the actor system
var system = new ActorSystem("MySystem");

// Create an actor
var greeter = system.ActorOf<GreeterActor>("greeter");

// Send a message
await greeter.TellAsync("Hello, World!");
```

## Actor Example

```csharp
public class GreeterActor : Actor
{
    protected override async Task ReceiveAsync(object message)
    {
        if (message is string greeting)
        {
            Console.WriteLine($"Received: {greeting}");
            await Context.Self.TellAsync($"Echo: {greeting}");
        }
    }
}
```

## Advanced Features

### Supervision Strategy

```csharp
public class CustomSupervisorStrategy : IActorSupervisorStrategy
{
    public void HandleError(Actor actor, Exception exception, object message)
    {
        // Custom error handling logic
    }
}

var system = new ActorSystem("MySystem", new CustomSupervisorStrategy());
```

### Configuration

```csharp
var config = new ActorSystemConfiguration
{
    DefaultMailboxSize = 1000,
    MessageTimeout = TimeSpan.FromSeconds(60),
    EnableMetrics = true
};
```

### Metrics

```csharp
var metrics = new ActorMetrics();
var messageCounts = metrics.GetMessageCounts();
```

## Best Practices

1. Keep actors small and focused
2. Use immutable messages
3. Handle all possible message types
4. Implement proper error handling
5. Use async/await consistently
6. Avoid shared state between actors

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details. 