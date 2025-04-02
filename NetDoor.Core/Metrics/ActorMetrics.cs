using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace NetDoor.Core.Metrics;

public class ActorMetrics
{
    private static readonly Meter Meter = new("NetDoor.Actors");
    private readonly ConcurrentDictionary<string, Counter<long>> _messageCounters = new();
    private readonly Counter<long> _totalMessagesProcessed;
    private readonly Histogram<double> _messageProcessingTime;

    public ActorMetrics()
    {
        _totalMessagesProcessed = Meter.CreateCounter<long>("total_messages_processed");
        _messageProcessingTime = Meter.CreateHistogram<double>("message_processing_time_ms");
    }

    public void RecordMessageProcessed(string actorPath)
    {
        _totalMessagesProcessed.Add(1);
        _messageCounters.GetOrAdd(actorPath, _ => 
            Meter.CreateCounter<long>($"messages_processed_{actorPath}")).Add(1);
    }

    public void RecordProcessingTime(string actorPath, double milliseconds)
    {
        var tags = new[] { new KeyValuePair<string, object?>("actor_path", actorPath) };
        _messageProcessingTime.Record(milliseconds, tags);
    }

    public IDictionary<string, long> GetMessageCounts()
    {
        return _messageCounters.ToDictionary(kvp => kvp.Key, kvp => 0L); // Note: Counter values are not directly accessible
    }
} 