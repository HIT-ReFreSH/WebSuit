// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using HitRefresh.WebSuit.Core;
using HitRefresh.WebSuit.Messaging;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HitRefresh.WebSuit.Clients;

public class WebSuitProviderClient : WebSuitClient
{
    public WebSuitProviderClient(IConfiguration configuration, ILogger<WebSuitClient> logger) : base
        (configuration, logger)
    {
        HubConnection.On<string, int, string>
        (
            "ReceiveInput",
            (sessionId, interruptionId, input) => { OnInputReceived?.Invoke(sessionId, interruptionId, input); }
        );
        HubConnection.On<string, int, string>
        (
            "ReceiveRequest",
            (sessionId, requestId, request) => OnRequestReceived?.Invoke(sessionId, requestId, request)
        );
    }

    /// <inheritdoc />
    protected override string Type => "Provider";

    public event Action<string, int, string>? OnInputReceived;
    public event Action<string, int, string>? OnRequestReceived;

    public async Task SendInterruptionAsync
        (string sessionId, int interruptionId, WebSuitInterruptionType interruptionMessage)
    {
        try
        {
            await HubConnection.InvokeAsync
                ("SendInterruption", RoomName, sessionId, interruptionId, interruptionMessage);
            Logger.LogInformation("Interruption sent to room: {RoomName}", RoomName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to send interruption to room: {RoomName}", RoomName);
        }
    }

    public async Task SendPrintAsync(string sessionId, PrintUnitTransfer printUnit)
    {
        try
        {
            await HubConnection.InvokeAsync("SendPrint", RoomName, sessionId, printUnit);
            Logger.LogInformation("Print units sent to room: {RoomName}", RoomName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to send print units to room: {RoomName}", RoomName);
        }
    }

    public async Task SendResponseAsync(string sessionId, int requestId, SuitContextSummary response)
    {
        try
        {
            await HubConnection.InvokeAsync("SendResponse", RoomName, sessionId, requestId, response);
            Logger.LogInformation("Response sent to room: {RoomName}", RoomName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to send response to room: {RoomName}", RoomName);
        }
    }
}