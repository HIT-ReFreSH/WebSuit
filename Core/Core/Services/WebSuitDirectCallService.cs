// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 23, 2024
//  *
//  */

using HitRefresh.WebSuit.Clients;
using HitRefresh.WebSuit.Messaging;

namespace HitRefresh.WebSuit.Core.Services;

/// <summary>
///     Run remote procedure call directly
/// </summary>
/// <param name="client"></param>
public class WebSuitDirectCallService
    (WebSuitConsumerClient client, WebSuitResponseCallBackService responseCallBackService)
{
    protected int RequestId = -2;

    /// <summary>
    ///     Invoke WebSuitDirectCall with raw MobileSuit command request
    /// </summary>
    /// <param name="requestRaw"></param>
    /// <returns>null if timeout</returns>
    public async Task<SuitContextSummary?> InvokeAsync(string requestRaw)
    {
        var id = RequestId;
        Interlocked.Decrement(ref RequestId);
        await client.SendRequestAsync(id, requestRaw);
        try
        {
            return await responseCallBackService.WaitFor(id);
        }
        catch (TimeoutException)
        {
            return null;
        }
    }
}