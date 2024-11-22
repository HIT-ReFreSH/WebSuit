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
using Microsoft.Extensions.Logging;

namespace HitRefresh.WebSuit.Hosts;

public class WebSuitProviderHost : IMobileSuitHost
{
    private readonly ISuitExceptionHandler _exceptionHandler;
    private readonly AsyncServiceScope _rootScope;
    private readonly IReadOnlyList<ISuitMiddleware> _suitApp;
    private IHostApplicationLifetime _lifetime;
    private TaskCompletionSource _startUp;
    private SuitRequestDelegate? _requestHandler;
    private Task? _hostTask;
    private bool _shutDown = false;

    public WebSuitProviderHost
    (
        IServiceProvider services,
        IReadOnlyList<ISuitMiddleware> middleware,
        TaskCompletionSource startUp
    )
    {
        Services = services;
        _suitApp = middleware;
        _startUp = startUp;
        _lifetime = Services.GetRequiredService<IHostApplicationLifetime>();
        _exceptionHandler = Services.GetRequiredService<ISuitExceptionHandler>();
        _rootScope = Services.CreateAsyncScope();
        Logger = Services.GetRequiredService<ILogger<WebSuitProviderHost>>();
    }

    /// <inheritdoc />
    public ILogger Logger { get; }

    public IServiceProvider Services { get; }

    public void Dispose() { _rootScope.Dispose(); }

    public void Initialize()
    {
        if (_requestHandler != null) return;
        var requestStack = new Stack<SuitRequestDelegate>();
        requestStack.Push(_ => Task.CompletedTask);


        foreach (var middleware in _suitApp.Reverse())
        {
            var next = requestStack.Peek();
            requestStack.Push(async c => await middleware.InvokeAsync(c, next));
        }

        _requestHandler = requestStack.Peek();
    }

    public async Task StartAsync(CancellationToken cancellationToken = new())
    {
        if (_hostTask is not null) return;
        Initialize();

        _startUp.SetResult();
        var client = Services.GetRequiredService<WebSuitProviderClient>();
        client.OnRequestReceived += HandleRequest;
        await client.ConnectAsync();
        _hostTask = WaitForExit();
    }

    public async Task StopAsync(CancellationToken cancellationToken = new())
    {
        if (_hostTask is null) return;
        _hostTask = null;
    }

    private async void HandleRequest(string sessionId, int requestId, string request)
    {
        if (_requestHandler is null) return;
        var requestScope = Services.CreateScope();
        var context = new SuitContext(requestScope);
        context.Request = [request];
        var webSuitContext = context.ServiceProvider.GetRequiredService<WebSuitContextService>();
        webSuitContext.SessionId = sessionId;
        webSuitContext.RequestId = requestId;
        webSuitContext.Role = "Provider";
        try
        {
            await _requestHandler(context);
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            context.Status = RequestStatus.Faulted;
            await _exceptionHandler.InvokeAsync(context);
        }

        if (context.Status == RequestStatus.OnExit
         || context.Status == RequestStatus.NoRequest && context.CancellationToken.IsCancellationRequested)
            _shutDown = true;
    }

    private async Task WaitForExit()
    {
        if (_requestHandler is null) return;

        for (; !_shutDown;)
        {
            await Task.Delay(1000);
        }

        var client = Services.GetRequiredService<WebSuitProviderClient>();
        client.OnRequestReceived -= HandleRequest;
        await client.DisconnectAsync();

        _lifetime.StopApplication();
    }


    public async ValueTask DisposeAsync() { await _rootScope.DisposeAsync(); }
}