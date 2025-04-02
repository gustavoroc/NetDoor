using System;
using System.Threading.Tasks;
using NetDoor.Core.Actors;
using Xunit;
using NetDoor.Core.Persistence;

namespace NetDoor.Tests.Examples;

public class OrderProcessingExample
{
    private readonly ActorSystem _system;

    public OrderProcessingExample()
    {
        var persistence = new InMemoryActorPersistence();
        _system = new ActorSystem(new ActorSystemConfig(), persistence);
    }

    [Fact]
    public async Task ProcessOrder_ShouldSucceed()
    {
        // Arrange
        var orderProcessor = _system.ActorOf<OrderProcessorActor>("order-processor");
        var paymentProcessor = _system.ActorOf<PaymentProcessorActor>("payment-processor");
        var inventoryManager = _system.ActorOf<InventoryManagerActor>("inventory-manager");

        var order = new Order
        {
            Id = "order-1",
            CustomerId = "customer-1",
            Items = new[] { new OrderItem("item-1", 2), new OrderItem("item-2", 1) },
            TotalAmount = 100.0m
        };

        // Act
        var result = await orderProcessor.AskAsync<OrderResult>(new ProcessOrderCommand(order));

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Order processed successfully", result.Message);
    }
}

public class OrderProcessorActor : Actor
{
    protected override async Task HandleMessageAsync(object message)
    {
        if (message is ProcessOrderCommand cmd)
        {
            // Process payment
            var paymentProcessor = Context.System.ActorOf<PaymentProcessorActor>("payment-processor");
            var paymentResult = await Context.AskAsync<PaymentResult>(paymentProcessor, new ProcessPaymentCommand(cmd.Order.TotalAmount));

            if (!paymentResult.Success)
            {
                SetResponse(new OrderResult(false, "Payment failed"));
                return;
            }

            // Check inventory
            var inventoryManager = Context.System.ActorOf<InventoryManagerActor>("inventory-manager");
            var inventoryResult = await Context.AskAsync<InventoryResult>(inventoryManager, new CheckInventoryCommand(cmd.Order.Items));

            if (!inventoryResult.Success)
            {
                SetResponse(new OrderResult(false, "Insufficient inventory"));
                return;
            }

            SetResponse(new OrderResult(true, "Order processed successfully"));
        }
    }
}

public class PaymentProcessorActor : Actor
{
    protected override async Task HandleMessageAsync(object message)
    {
        if (message is ProcessPaymentCommand cmd)
        {
            // Simulate payment processing
            await Task.Delay(100);
            SetResponse(new PaymentResult(true, "Payment processed"));
        }
    }
}

public class InventoryManagerActor : Actor
{
    protected override async Task HandleMessageAsync(object message)
    {
        if (message is CheckInventoryCommand cmd)
        {
            // Simulate inventory check
            await Task.Delay(100);
            SetResponse(new InventoryResult(true, "Inventory available"));
        }
    }
}

public record Order
{
    public string Id { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public OrderItem[] Items { get; init; } = Array.Empty<OrderItem>();
    public decimal TotalAmount { get; init; }
}

public record OrderItem(string ProductId, int Quantity);
public record ProcessOrderCommand(Order Order);
public record ProcessPaymentCommand(decimal Amount);
public record CheckInventoryCommand(OrderItem[] Items);

public record OrderResult(bool Success, string Message);
public record PaymentResult(bool Success, string Message);
public record InventoryResult(bool Success, string Message); 