// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 23, 2024
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

namespace HitRefresh.WebSuit;

// <summary>
/// A entity, which serves the shell functions of a mobile-suit program.
/// </summary>
public class WebSuitConsumerHost
(
    IServiceProvider services,
    SuitHostStartCompletionSource startUp,
    SuitRequestDelegate requestHandler,
    ISuitContextFactory contextFactory,
    IHostApplicationLifetime lifetime,
    WebSuitConsumerClient client,
    WebSuitConsumerIODriver consumerIoDriver,
    WebSuitResponseCallBackService callBackService
) : SuitHost(services, startUp, requestHandler, contextFactory)
{


    public override async Task StartAsync(CancellationToken cancellationToken = new())
    {
        await client.ConnectAsync();
        using var scope = this.Services.CreateScope();
        var io=scope.ServiceProvider.GetRequiredService<IIOHub>();
        if (client.Connected)
        {
            await io.WriteLineAsync("Remote Connected.", OutputType.Ok);
            callBackService.Enable();
        }
        else
        {
            await io.WriteLineAsync("Remote Not Connected.", OutputType.Error);

        }

        await base.StartAsync(cancellationToken);
    }


}