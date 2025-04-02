using System.Threading.Channels;

namespace NetDoor.Core.Actors;

public abstract class Actor
{
    private readonly Channel<object> _mailbox;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _messageProcessingTask;
    private ActorContext? _context;
    private bool _isStopped;

    protected ActorContext Context => _context ?? throw new InvalidOperationException("Actor context not initialized");

    protected Actor()
    {
        _mailbox = Channel.CreateUnbounded<object>(new UnboundedChannelOptions 
        { 
            SingleReader = true,
            SingleWriter = false 
        });
        _cancellationTokenSource = new CancellationTokenSource();
        _messageProcessingTask = ProcessMessagesAsync();
    }

    public void SetContext(ActorContext context)
    {
        _context = context;
    }

    protected abstract Task HandleMessageAsync(object message);

    public virtual async Task ReceiveAsync(object message)
    {
        if (_isStopped)
        {
            return;
        }

        try
        {
            await HandleMessageAsync(message);
        }
        catch (Exception ex)
        {
            Context.System.HandleFailure(Context.Self, ex);
        }
    }

    public void Stop()
    {
        _isStopped = true;
        Context.Stop();
    }

    protected void SetResponse<T>(T response)
    {
        Context.Self.SetResponse(response);
    }

    private async Task ProcessMessagesAsync()
    {
        try
        {
            while (await _mailbox.Reader.WaitToReadAsync(_cancellationTokenSource.Token))
            {
                while (_mailbox.Reader.TryRead(out var message))
                {
                    try
                    {
                        await ReceiveAsync(message);
                    }
                    catch (Exception ex)
                    {
                        HandleError(ex, message);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Actor is being stopped
        }
    }

    protected virtual void HandleError(Exception exception, object message)
    {
        // Default error handling - can be overridden by actors
        if (Context?.SupervisorStrategy != null)
        {
            Context.SupervisorStrategy.HandleError(this, exception, message);
        }
    }

    public async Task StopAsync()
    {
        _cancellationTokenSource.Cancel();
        await _messageProcessingTask;
    }
} 