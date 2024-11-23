// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using HitRefresh.MobileSuit.Core;
using HitRefresh.MobileSuit.Core.Services;
using HitRefresh.WebSuit;
using HitRefresh.WebSuit.Clients;
using HitRefresh.WebSuit.Core;
using HitRefresh.WebSuit.Core.Middleware;
using HitRefresh.WebSuit.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HitRefresh.MobileSuit;

public static class SuitHostBuilderExtension
{
    /// <summary>
    ///     Transforms the original builder as WebSuitProviderHostBuilder
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static SuitHostBuilder AsWebSuitProvider(this SuitHostBuilder builder)
    {
        builder.Services.AddScoped<IIOHub, WebSuitProviderIOHub>();
        builder.Services.AddSingleton<ISuitExceptionHandler, WebSuitExceptionHandler>();
        builder.Services.AddScoped<WebSuitContextService>();
        builder.Services.AddSingleton<WebSuitProviderClient>();
        builder.Services.AddLogging(loggingBuilder => { loggingBuilder.AddConsole(); });
        builder.WorkFlow
               .UseRequestParsing()
               .UseHostShell()
               .UseAppShell()
               .UseCustom<WebSuitResultForwardingMiddleware>()
               .UseFinalize();
        return builder;
    }

    /// <summary>
    ///     Transforms the original builder as WebSuitConsumerHostBuilder
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static SuitHostBuilder AsWebSuitConsumer(this SuitHostBuilder builder)
    {
        builder.Services.AddScoped<WebSuitConsumerIODriver>();
        builder.Services.AddSingleton<WebSuitResponseCallBackService>();
        builder.Services.AddSingleton<WebSuitConsumerClient>();
        builder.Services.AddSingleton<IMobileSuitHost, WebSuitConsumerHost>();

        builder.WorkFlow.UsePrompt()
               .UseIOInput()
               .UseRequestDump()
               .UseRequestParsing()
               .UseHostShell()
               .UseAppShell()
               .UseWebSuitShell()
               .UseWebSuitResponse()
               .UseFinalize();
        return builder;
    }

    /// <summary>
    ///     Check execution availability before calling remote, save your time.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static SuitHostBuilder UseRemoteShell(this SuitHostBuilder builder)
    {
        builder.Services.AddSingleton<WebSuitAppShell>
        (
            sp => WebSuitAppShell.FromClients(sp.GetKeyedServices<SuitShell>(WebSuitBuildUtils.WEB_SUIT_CLIENT_FLAG))
        );
        return builder;
    }

    /// <summary>
    ///     Add a remote client shell to mobile suit
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public static SuitHostBuilder AddRemoteClient(this SuitHostBuilder builder, SuitShell client)
    {
        builder.Services.AddKeyedSingleton(WebSuitBuildUtils.WEB_SUIT_CLIENT_FLAG, client);
        return builder;
    }

    /// <summary>
    ///     Add a remote method shell to mobile suit
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public static SuitHostBuilder MapRemote(this SuitHostBuilder builder, string name, Delegate method)
    {
        builder.AddClient(SuitMethodShell.FromDelegate(name, method));
        return builder;
    }

    /// <summary>
    ///     Add a remote client shell to mobile suit
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="client"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static SuitHostBuilder MapRemoteClient<T>(this SuitHostBuilder builder)
    {
        builder.AddClient(SuitObjectShell.FromType(typeof(T)));
        return builder;
    }

    /// <summary>
    ///     Add a remote client shell to mobile suit
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="client"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static SuitHostBuilder MapRemoteClient<T>(this SuitHostBuilder builder, string name)
    {
        builder.AddClient(SuitObjectShell.FromType(typeof(T), name));
        return builder;
    }

    /// <summary>
    ///     Enable direct call of remote command
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static SuitHostBuilder UseDirectCall(this SuitHostBuilder builder)
    {
        builder.Services.AddSingleton<WebSuitDirectCallService>();
        return builder;
    }

    /// <summary>
    ///     Add WebSuitResultForwardingMiddleware to workflow
    /// </summary>
    /// <param name="flow"></param>
    /// <returns></returns>
    public static ISuitWorkFlow UseResultForwarding(this ISuitWorkFlow flow)
    {
        return flow.UseCustom<WebSuitResultForwardingMiddleware>();
    }

    /// <summary>
    ///     Add WebSuitRequestDumpMiddleware to workflow
    /// </summary>
    /// <param name="flow"></param>
    /// <returns></returns>
    public static ISuitWorkFlow UseRequestDump(this ISuitWorkFlow flow)
    {
        return flow.UseCustom<WebSuitRequestDumpMiddleware>();
    }

    /// <summary>
    ///     Add WebSuitRequestDumpMiddleware to workflow
    /// </summary>
    /// <param name="flow"></param>
    /// <returns></returns>
    public static ISuitWorkFlow UseWebSuitResponse(this ISuitWorkFlow flow)
    {
        return flow.UseCustom<WebSuitResponseMiddleware>();
    }

    /// <summary>
    ///     Add WebSuitShellMiddleware to workflow
    /// </summary>
    /// <param name="flow"></param>
    /// <returns></returns>
    public static ISuitWorkFlow UseWebSuitShell(this ISuitWorkFlow flow)
    {
        return flow.UseCustom<WebSuitShellMiddleware>();
    }
}