// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 23, 2024
//  *
//  */

using HitRefresh.MobileSuit;
using HitRefresh.MobileSuit.Core.Services;
using HitRefresh.WebSuit.Core.Services;

namespace HitRefresh.WebSuit.Core;

public class WebSuitConsumerCommandServer(IIOHub io, SuitAppShell app, WebSuitAppShell webApp, SuitHostShell host, ITaskService taskService): SuitCommandServer(io, app, host, taskService)
{
    public override async Task ListCommands(string[] args)
    {
        await base.ListCommands(args);
        await io.WriteLineAsync("---");
        await io.WriteLineAsync("WebSuit Consumer Command Server");
        await ListMembersAsync(webApp);
    }
}