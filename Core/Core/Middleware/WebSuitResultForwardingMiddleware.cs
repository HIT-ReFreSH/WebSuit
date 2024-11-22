// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using HitRefresh.MobileSuit;
using HitRefresh.MobileSuit.Core;
using HitRefresh.MobileSuit.Core.Middleware;
using HitRefresh.MobileSuit.Core.Services;
using HitRefresh.WebSuit.Clients;
using HitRefresh.WebSuit.Core.Services;
using HitRefresh.WebSuit.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace HitRefresh.WebSuit.Core.Middleware;

/// <summary>
/// Forward response from WebSuit Provider to Consumer.
/// </summary>
public class WebSuitResultForwardingMiddleware : ISuitMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(SuitContext context, SuitRequestDelegate next)
    {
        if (context.Status == RequestStatus.NotHandled) context.Status = RequestStatus.CommandNotFound;
        var client = context.ServiceProvider.GetRequiredService<WebSuitProviderClient>();
        var webSuitContext = context.ServiceProvider.GetRequiredService<WebSuitContextService>();
        await client.SendResponseAsync
            (webSuitContext.SessionId, webSuitContext.RequestId, SuitContextSummary.FromSuitContext(context));
        await next(context);
    }
}