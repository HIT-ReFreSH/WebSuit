// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using System.Security.Cryptography;
using System.Text;
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
    public WebSuitClient(IConfiguration configuration, ILogger<WebSuitClient> logger)
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
    private static byte[] Encrypt(string pathOrKey, string data)
    {
        string privateKey;
        if (pathOrKey.StartsWith("file::"))
        {
            var fileName = pathOrKey.Substring("file::".Length);
            if (!File.Exists(fileName)) throw new FileNotFoundException("Private key file not found.", fileName);

            privateKey = File.ReadAllText(fileName).Trim();
        }
        else
        {
            privateKey = pathOrKey;
        }

        var dataBytes = Encoding.UTF8.GetBytes(data);

        if (privateKey.StartsWith("ssh-ed25519 "))
        {
            // 处理 Ed25519 私钥格式
            var privateKeyBytes = Convert.FromBase64String(privateKey.Split(' ')[1]);
            var key = Key.Import(SignatureAlgorithm.Ed25519, privateKeyBytes, KeyBlobFormat.RawPrivateKey);
            return SignatureAlgorithm.Ed25519.Sign(key, dataBytes);
        }

        if (privateKey.StartsWith("ssh-rsa "))
        {
            // 处理 RSA 私钥格式
            var privateKeyBytes = Convert.FromBase64String(privateKey.Split(' ')[1]);
            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
            return rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        if (privateKey.StartsWith("-----BEGIN PRIVATE KEY-----"))
        {
            // 处理 PEM 格式的私钥
            var privateKeyBytes = Convert.FromBase64String
            (
                privateKey.Replace
                           ("-----BEGIN PRIVATE KEY-----", "")
                          .Replace("-----END PRIVATE KEY-----", "")
                          .Replace("\n", "")
            );
            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
            return rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        throw new ArgumentException("Unsupported private key format.");
    }

    protected abstract string Type { get;}
    public async Task ConnectAsync()
    {
        var utcTime = DateTime.UtcNow.ToString("o"); // 使用 ISO 8601 格式
        var dataToSign = $"{Type}@{RoomName}@{utcTime}";
        var signatureBytes = Encrypt(_privateKey, dataToSign);
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