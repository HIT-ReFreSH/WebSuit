// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 23, 2024
//  *
//  */

using System.Collections.Concurrent;
using HitRefresh.MobileSuit;
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

    private readonly ConcurrentQueue<PrintUnit> _printUnits = new();

    private readonly ConcurrentDictionary<int, SuitContextSummary> _responses = new();

    private readonly int _timeOut = config.GetSection("WebSuit:Timeout").Exists()
                                        ? config.GetSection("WebSuit:Timeout").Get<int>()
                                        : 200000;

    /// <inheritdoc />
    public void Dispose()
    {
        client.OnResponseReceived -= HandleResponse;
        client.OnPrintReceived -= GotResponse;
    }

    public void Enable()
    {
        client.OnResponseReceived += HandleResponse;
        client.OnPrintReceived += GotResponse;
    }

    private void GotResponse(PrintUnitTransfer pu) { _printUnits.Enqueue(pu.ToPrintUnit()); }

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

            if (_printUnits.TryDequeue(out _)) sumWait = 0;

            if (sumWait >= _timeOut)
                throw new TimeoutException($"Response for id {id} is not received within {_timeOut}ms.");
            await Task.Delay(_interval);
            sumWait += _interval;
        }
    }

    private void HandleResponse(int id, SuitContextSummary summary) { _responses[id] = summary; }
}