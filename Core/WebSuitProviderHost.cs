// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using HitRefresh.MobileSuit;
using HitRefresh.MobileSuit.Core;
using HitRefresh.MobileSuit.Core.Services;
using HitRefresh.WebSuit.Clients;
using HitRefresh.WebSuit.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HitRefresh.WebSuit;

/// <summary>
///     A Suit Host for WebSuit provider
/// </summary>
/// <param name="services"></param>
/// <param name="startUp"></param>
/// <param name="requestHandler"></param>
/// <param name="contextFactory"></param>
/// <param name="lifetime"></param>
public class WebSuitProviderHost
(
    IServiceProvider services,
    SuitHostStartCompletionSource startUp,
    SuitRequestDelegate requestHandler,
    ISuitContextFactory contextFactory,
    IHostApplicationLifetime lifetime,
    WebSuitProviderClient client
) : SuitHost(services, startUp, requestHandler, contextFactory)
{
    private Task? _hostTask;
    private bool _shutDown;


    public override async Task StartAsync(CancellationToken cancellationToken = new())
    {
        if (_hostTask is not null) return;
        client.OnRequestReceived += HandleRequest;
        await client.ConnectAsync();
        _hostTask = WaitForExit();
        StartUp.SetResult();
    }

    public override Task StopAsync(CancellationToken cancellationToken = new())
    {
        if (_hostTask is null) return Task.CompletedTask;
        _hostTask = null;
        return Task.CompletedTask;
    }

    private async void HandleRequest(string sessionId, int requestId, string request)
    {
        var context = ContextFactory.CreateContext();
        context.Request = [request];
        var webSuitContext = context.ServiceProvider.GetRequiredService<WebSuitContextService>();
        webSuitContext.SessionId = sessionId;
        webSuitContext.RequestId = requestId;
        webSuitContext.Role = "Provider";
        await RequestHandler(context);

        if (context.Status == RequestStatus.OnExit
         || context is { Status: RequestStatus.NoRequest, CancellationToken.IsCancellationRequested: true })
            _shutDown = true;
    }

    private async Task WaitForExit()
    {
        for (; !_shutDown;) await Task.Delay(1000);
        client.OnRequestReceived -= HandleRequest;
        await client.DisconnectAsync();
        lifetime.StopApplication();
    }
}