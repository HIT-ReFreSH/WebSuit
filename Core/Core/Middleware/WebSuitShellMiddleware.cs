// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using HitRefresh.MobileSuit;
using HitRefresh.MobileSuit.Core;
using HitRefresh.WebSuit.Clients;
using HitRefresh.WebSuit.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HitRefresh.WebSuit.Core.Middleware;

/// <summary>
///     Middleware to execute command over web suit server shell.
/// </summary>
public class WebSuitShellMiddleware : ISuitMiddleware
{
    private int requestId;

    /// <inheritdoc />
    public async Task InvokeAsync(SuitContext context, SuitRequestDelegate next)
    {
        if (context.CancellationToken.IsCancellationRequested)
        {
            context.Status = RequestStatus.Interrupt;
            await next(context);
        }

        var rawCmd = context.Properties.TryGetValue("WebSuit::OriginCmd", out var val) ? val : "";
        if (context.Status != RequestStatus.NotHandled
         || string.IsNullOrEmpty(rawCmd))
        {
            await next(context);
            return;
        }

        var webSuitShell = context.ServiceProvider.GetService<WebSuitAppShell>();
        if (webSuitShell?.MayExecute(context.Request) == false)
        {
            await next(context);
            return;
        }

        var ioDriver = context.ServiceProvider.GetRequiredService<WebSuitConsumerIODriver>();
        ioDriver.Enable();
        var client = context.ServiceProvider.GetRequiredService<WebSuitConsumerClient>();
        var id = requestId;
        Interlocked.Increment(ref requestId);
        await client.SendRequestAsync(id, rawCmd);
        context.Properties["WebSuit::RequestId"] = id.ToString();

        await next(context);
    }
}