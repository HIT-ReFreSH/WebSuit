// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using System.Collections.Concurrent;
using HitRefresh.WebSuit.Core;
using HitRefresh.WebSuit.Services;
using Microsoft.AspNetCore.SignalR;

namespace HitRefresh.WebSuit.Messaging;

public class WebSuitHub(KeyChainService keyChain, ILogger<WebSuitHub> logger) : Hub
{
    private static readonly ConcurrentDictionary<string, RoomInfo> Rooms = new();
    private static readonly ConcurrentDictionary<string, string> ConsumerToProviderMap = new();


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? roomName = null;

        // 查找并移除 Consumer 和 Provider 的连接
        foreach (var room in Rooms)
        {
            if (room.Value.Providers.Remove(Context.ConnectionId))
            {
                roomName = room.Key;
            }
            else if (room.Value.Consumers.Remove(Context.ConnectionId))
            {
                roomName = room.Key;
                ConsumerToProviderMap.TryRemove(Context.ConnectionId, out _);
            }

            if (roomName != null && room.Value.IsEmpty())
            {
                Rooms.TryRemove(room.Key, out _);
                break;
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task Authenticate(string role, string roomName, DateTime utcTime, string signature)
    {
        if (!IsTimestampValid(utcTime)) throw new HubException("Invalid or expired timestamp.");

        var dataToVerify = $"{role}@{roomName}@{utcTime}";

        if (!keyChain.VerifySignature(dataToVerify, signature))
            throw new HubException("Signature verification failed.");

        switch (role)
        {
        case "Provider": await RegisterProvider(roomName); break;
        case "Consumer": await RegisterConsumer(roomName); break;
        default: throw new HubException("Invalid role specified.");
        }
    }

    private async Task RegisterProvider(string roomName)
    {
        var room = Rooms.GetOrAdd(roomName, _ => new RoomInfo());
        room.Providers.Add(Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
    }

    private async Task RegisterConsumer(string roomName)
    {
        var room = Rooms.GetOrAdd(roomName, _ => new RoomInfo());

        if (room.Providers.Count == 0) throw new HubException("Room does not have a Provider.");

        room.Consumers.Add(Context.ConnectionId);

        var providerId = room.Providers.ToArray()[Random.Shared.Next(0, room.Providers.Count - 1)];
        ConsumerToProviderMap[Context.ConnectionId] = providerId;

        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
    }

    // Consumer -> Provider 消息转发
    public async Task SendInput(string roomName, int interruptionId, string input)
    {
        if (!Rooms.TryGetValue(roomName, out var room) || !room.Consumers.Contains(Context.ConnectionId))
            throw new HubException("You are not a Consumer in this room.");

        if (!ConsumerToProviderMap.TryGetValue(Context.ConnectionId, out var providerId))
            throw new HubException("No Provider is assigned to you.");

        await Clients.Client(providerId).SendAsync("ReceiveInput", Context.ConnectionId, interruptionId, input);
        // logger.LogInformation("SendInput({0}->{1}): #{id} {2}", roomName, Context.ConnectionId, interruptionId, input);
    }

    public async Task SendRequest(string roomName, int requestId, string request)
    {
        if (!Rooms.TryGetValue(roomName, out var room) || !room.Consumers.Contains(Context.ConnectionId))
            throw new HubException("You are not a Consumer in this room.");

        if (!ConsumerToProviderMap.TryGetValue(Context.ConnectionId, out var providerId))
            throw new HubException("No Provider is assigned to you.");

        await Clients.Client(providerId).SendAsync("ReceiveRequest", Context.ConnectionId, requestId, request);
    }

    // Provider -> Consumer 消息转发
    public async Task SendInterruption
        (string roomName, string consumerId, int interruptionId, WebSuitInterruptionType interruptionMessage)
    {
        if (!Rooms.TryGetValue(roomName, out var room) || !room.Providers.Contains(Context.ConnectionId))
            throw new HubException("You are not a Provider in this room.");

        if (!room.Consumers.Contains(consumerId)) throw new HubException("The specified Consumer is not in this room.");

        await Clients.Client(consumerId).SendAsync("ReceiveInterruption", interruptionId, interruptionMessage);
        // logger.LogInformation("SendInterruption({0}->{1}): #{id} {2}", roomName, Context.ConnectionId, interruptionId, Enum.GetName(interruptionMessage));
    }

    public async Task SendPrint(string roomName, string consumerId, PrintUnitTransfer printUnits)
    {
        if (!Rooms.TryGetValue(roomName, out var room) || !room.Providers.Contains(Context.ConnectionId))
            throw new HubException("You are not a Provider in this room.");

        if (!room.Consumers.Contains(consumerId)) throw new HubException("The specified Consumer is not in this room.");

        await Clients.Client(consumerId).SendAsync("ReceivePrint", printUnits);
        // logger.LogInformation($"{nameof(SendPrint)}({{0}}->{{1}}): {{2}}", roomName, consumerId, printUnits.Text);
    }

    public async Task SendResponse(string roomName, string consumerId, int requestId, SuitContextSummary response)
    {
        if (!Rooms.TryGetValue(roomName, out var room) || !room.Providers.Contains(Context.ConnectionId))
            throw new HubException("You are not a Provider in this room.");

        if (!room.Consumers.Contains(consumerId)) throw new HubException("The specified Consumer is not in this room.");

        await Clients.Client(consumerId).SendAsync("ReceiveResponse", requestId, response);
    }

    private static bool IsTimestampValid(DateTime utcTime)
    {
        var now = DateTime.UtcNow;
        return utcTime > now.AddMinutes(-5) && utcTime < now.AddMinutes(5);
    }
}

// 房间信息类
public class RoomInfo
{
    public HashSet<string> Providers { get; set; } = new();
    public HashSet<string> Consumers { get; set; } = new();

    public bool IsEmpty() { return Providers.Count == 0 && Consumers.Count == 0; }
}