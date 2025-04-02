using System;
using System.Threading.Tasks;

namespace NetDoor.Tests;

public static class TestHelpers
{
    public static async Task WaitForCondition(Func<bool> condition, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(1);
        var startTime = DateTime.UtcNow;

        while (!condition() && DateTime.UtcNow - startTime < timeout)
        {
            await Task.Delay(10);
        }

        if (!condition())
        {
            throw new TimeoutException($"Condition not met within {timeout.Value.TotalSeconds} seconds");
        }
    }
} 