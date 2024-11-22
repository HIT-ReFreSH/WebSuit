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
using Microsoft.Extensions.DependencyInjection;

namespace HitRefresh.WebSuit.Core.Middleware;


/// <summary>
///     Middleware to execute command over web suit server shell.
/// </summary>
public class WebSuitRequestDumpMiddleware : ISuitMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(SuitContext context, SuitRequestDelegate next)
    {
        context.Properties["WebSuit::OriginCmd"] = context.Request.FirstOrDefault("");
        await next(context);
    }
}