// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using System.Security.Cryptography;
using System.Text;
using HitRefresh.WebSuit.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSec.Cryptography;

namespace HitRefresh.WebSuit.Clients;

public abstract class WebSuitClient
{
    protected HubConnection HubConnection{ get; }
    protected ILogger<WebSuitClient> Logger{ get; }
    private readonly string _privateKey;
    protected string RoomName { get; }

    protected WebSuitClient(IConfiguration configuration, ILogger<WebSuitClient> logger)
    {
        Logger = logger;
        var host = configuration["WebSuit:Host"] ?? "";
        RoomName = configuration["WebSuit:App"] ?? "";
        _privateKey = configuration["WebSuit:PrivateKey"] ?? "";
        // 配置 SignalR Hub 连接
        HubConnection = new HubConnectionBuilder()
                        .WithUrl($"{host}/connect")
                        .Build();

    }


    protected abstract string Type { get;}
    public async Task ConnectAsync()
    {
        var utcTime = DateTime.UtcNow;//.ToString("o"); // 使用 ISO 8601 格式
        var dataToSign = $"{Type}@{RoomName}@{utcTime}";
        var signatureBytes = KeyChainService.Encrypt( _privateKey, dataToSign);
        var signature = Convert.ToBase64String(signatureBytes);

        try
        {
            await HubConnection.StartAsync();
            await HubConnection.InvokeAsync("Authenticate", "Provider", RoomName, utcTime, signature);
            Logger.LogInformation("Connected as Provider to room: {RoomName}", RoomName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to connect to room: {RoomName}", RoomName);
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            await HubConnection.StopAsync();
            Logger.LogInformation("Disconnected from room: {RoomName}", RoomName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to disconnect from room: {RoomName}", RoomName);
        }
    }
}