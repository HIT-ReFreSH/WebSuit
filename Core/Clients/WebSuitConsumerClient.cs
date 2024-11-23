// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using HitRefresh.MobileSuit;
using HitRefresh.WebSuit.Messaging;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HitRefresh.WebSuit.Clients;

public class WebSuitConsumerClient : WebSuitClient
{
    public WebSuitConsumerClient(IConfiguration configuration, ILogger<WebSuitClient> logger) : base
        (configuration, logger)
    {
        HubConnection.On<int, WebSuitInterruptionType>
            ("ReceiveInterruption", (id, type) => OnInterruptionReceived?.Invoke(id, type));
        HubConnection.On<PrintUnit>("ReceivePrint", printUnits => OnPrintReceived?.Invoke(printUnits));
        HubConnection.On<int, SuitContextSummary>
            ("ReceiveResponse", (id, response) => OnResponseReceived?.Invoke(id, response));
    }

    /// <inheritdoc />
    protected override string Type => "Consumer";

    public event Action<int, WebSuitInterruptionType>? OnInterruptionReceived;
    public event Action<PrintUnit>? OnPrintReceived;
    public event Action<int, SuitContextSummary>? OnResponseReceived;

    public async Task SendInputAsync(int interruptionId, string input)
    {
        try
        {
            await HubConnection.InvokeAsync("SendInput", RoomName, interruptionId, input);
            Logger.LogInformation("Input sent to room: {RoomName}", RoomName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to send input to room: {RoomName}", RoomName);
        }
    }

    public async Task SendRequestAsync(int requestId, string request)
    {
        try
        {
            await HubConnection.InvokeAsync("SendRequest", RoomName, requestId, request);
            Logger.LogInformation("Request sent to room: {RoomName}", RoomName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to send request to room: {RoomName}", RoomName);
        }
    }
}