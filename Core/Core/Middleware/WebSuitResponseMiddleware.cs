// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 23, 2024
//  *
//  */

using HitRefresh.MobileSuit;
using HitRefresh.MobileSuit.Core;
using HitRefresh.WebSuit.Clients;
using HitRefresh.WebSuit.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HitRefresh.WebSuit.Core.Middleware;

/// <summary>
///     Middleware to wait for remote execution.
/// </summary>
public class WebSuitResponseMiddleware : ISuitMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(SuitContext context, SuitRequestDelegate next)
    {
        if (context.CancellationToken.IsCancellationRequested)
        {
            context.Status = RequestStatus.Interrupt;
            await next(context);
        }

        var id = int.Parse(context.Properties.GetValueOrDefault("WebSuit::RequestId", "-1"));
        if (context.Status != RequestStatus.NotHandled
         || id == -1)
        {
            await next(context);
            return;
        }

        var responseService = context.ServiceProvider.GetRequiredService<WebSuitResponseCallBackService>();
        var remoteRes = await responseService.WaitFor(id);
        remoteRes.CopyTo(context);

        await next(context);
    }
}