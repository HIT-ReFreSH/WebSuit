// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using HitRefresh.MobileSuit;
using HitRefresh.MobileSuit.Core;
using HitRefresh.WebSuit.Clients;
using HitRefresh.WebSuit.Core.Middleware;
using HitRefresh.WebSuit.Core.Services;
using HitRefresh.WebSuit.Hosts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuitHostBuilderExtensions = HitRefresh.MobileSuit.SuitHostBuilderExtensions;

namespace HitRefresh.WebSuit.Extensions;

public static class SuitHostBuilderExtension
{
    public static SuitHostBuilder AsWebSuitProvider(this SuitHostBuilder builder)
    {
        builder.Services.AddScoped<IIOHub, WebSuitProviderIOHub>();
        builder.Services.AddSingleton<WebSuitProviderClient>();
        builder.WorkFlow
               .UseRequestParsing()
               .UseHostShell()
               .UseAppShell()
               .UseCustom<WebSuitResultForwardingMiddleware>()
               .UseFinalize();

        return builder;
    }

    public static IMobileSuitHost BuildAsWebSuitProvider(this SuitHostBuilder builder)
    {
        builder.AddPreBuildMatters();
        var startUp = new TaskCompletionSource();
        builder.Services.AddSingleton<IHostApplicationLifetime>(
            new SuitHostApplicationLifetime(startUp, () => Task.CompletedTask));
        var providers = builder. Services.BuildServiceProvider();
        return new WebSuitProviderHost
        (
            providers,
            builder.WorkFlow.Build(providers),
            startUp
        );
    }
}