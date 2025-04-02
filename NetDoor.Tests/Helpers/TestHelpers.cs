namespace NetDoor.Tests.Helpers;

public static class TestHelpers
{
    public static async Task WaitForCondition(Func<Task<bool>> condition, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(5);
        var startTime = DateTime.UtcNow;
        
        while (!await condition())
        {
            if (DateTime.UtcNow - startTime > timeout)
            {
                throw new TimeoutException($"Condition not met within {timeout.Value.TotalSeconds} seconds");
            }
            
            await Task.Delay(50);
        }
    }

    public static Task WaitForCondition(Func<bool> condition, TimeSpan? timeout = null)
    {
        return WaitForCondition(() => Task.FromResult(condition()), timeout);
    }
} 