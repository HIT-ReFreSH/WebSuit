// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 23, 2024
//  *
//  */

using System.Collections.Concurrent;
using HitRefresh.WebSuit.Clients;
using HitRefresh.WebSuit.Messaging;
using Microsoft.Extensions.Configuration;

namespace HitRefresh.WebSuit.Core.Services;

/// <summary>
///     A service used to wait for Remote callback
/// </summary>
/// <param name="client"></param>
/// <param name="config"></param>
public class WebSuitResponseCallBackService(WebSuitConsumerClient client, IConfiguration config) : IDisposable
{
    private readonly int _interval = config.GetSection("WebSuit:Interval").Exists()
                                         ? config.GetSection("WebSuit:Interval").Get<int>()
                                         : 100;

    private readonly int _timeOut = config.GetSection("WebSuit:Timeout").Exists()
                                        ? config.GetSection("WebSuit:Timeout").Get<int>()
                                        : 5000;

    private readonly ConcurrentDictionary<int, SuitContextSummary> _responses = new();

    /// <inheritdoc />
    public void Dispose() { client.OnResponseReceived -= HandleResponse; }

    public void Enable() { client.OnResponseReceived += HandleResponse; }

    public async Task<SuitContextSummary> WaitFor(int id)
    {
        var sumWait = 0;
        for (;;)
        {
            if (_responses.TryGetValue(id, out var res))
            {
                _responses.TryRemove(id, out _);
                return res;
            }

            if (sumWait >= _timeOut)
                throw new TimeoutException($"Response for id {id} is not received within {_timeOut}ms.");
            await Task.Delay(_interval);
            sumWait += _interval;
        }
    }

    private void HandleResponse(int id, SuitContextSummary summary) { _responses[id] = summary; }
}