namespace NetDoor.Core.Configuration;

public class ActorSystemConfiguration
{
    public int DefaultMailboxSize { get; set; } = 100;
    public int DefaultActorConcurrencyLevel { get; set; } = 1;
    public TimeSpan MessageTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableMetrics { get; set; } = false;
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
}

public enum LogLevel
{
    Debug,
    Information,
    Warning,
    Error,
    Critical
} 