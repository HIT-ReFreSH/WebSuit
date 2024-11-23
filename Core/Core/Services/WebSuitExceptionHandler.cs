// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 23, 2024
//  *
//  */

using HitRefresh.MobileSuit.Core;
using HitRefresh.MobileSuit.Core.Services;
using HitRefresh.WebSuit.Clients;
using HitRefresh.WebSuit.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace HitRefresh.WebSuit.Core.Services;

public class WebSuitExceptionHandler : ISuitExceptionHandler
{
    public async Task InvokeAsync(SuitContext context)
    {
        var client = context.ServiceProvider.GetRequiredService<WebSuitProviderClient>();
        var webSuitContext = context.ServiceProvider.GetRequiredService<WebSuitContextService>();
        await client.SendResponseAsync
            (webSuitContext.SessionId, webSuitContext.RequestId, SuitContextSummary.FromSuitContext(context));
    }
}