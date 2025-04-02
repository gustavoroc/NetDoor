namespace NetDoor.Core.Actors;

public class ActorSystemConfig
{
    public int MaxMailboxSize { get; set; } = 1000;
    public int MaxConcurrentMessages { get; set; } = 100;
    public TimeSpan MessageTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan SnapshotInterval { get; set; } = TimeSpan.FromMinutes(5);
    public int SnapshotThreshold { get; set; } = 100;
} 