namespace NetDoor.Core.Actors;

using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public class ActorRef
{
    private readonly Actor _actor;
    private readonly ActorSystem _system;
    private readonly string _id;
    private readonly Dictionary<Type, TaskCompletionSource<object>> _responseSources;

    public ActorRef(Actor actor, ActorSystem system)
    {
        _actor = actor;
        _system = system;
        _id = Guid.NewGuid().ToString();
        _responseSources = new Dictionary<Type, TaskCompletionSource<object>>();
        
        var context = new ActorContext(this, _id, _system);
        _actor.SetContext(context);
    }

    public string Id => _id;

    public object UnderlyingActor => _actor;

    public async Task TellAsync(object message)
    {
        await _actor.ReceiveAsync(message);
    }

    public async Task<T> AskAsync<T>(object message)
    {
        var tcs = new TaskCompletionSource<object>();
        _responseSources[typeof(T)] = tcs;
        
        try
        {
            await _actor.ReceiveAsync(message);
            var response = await tcs.Task;
            return (T)response;
        }
        finally
        {
            _responseSources.Remove(typeof(T));
        }
    }

    public void Stop()
    {
        _actor.Stop();
    }

    public override string ToString()
    {
        return _id;
    }

    internal void SetResponse<T>(T response)
    {
        if (_responseSources.TryGetValue(typeof(T), out var tcs))
        {
            tcs.SetResult(response!);
        }
    }
} 