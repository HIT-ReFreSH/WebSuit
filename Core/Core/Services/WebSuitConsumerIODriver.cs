// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 23, 2024
//  *
//  */

using HitRefresh.MobileSuit;
using HitRefresh.WebSuit.Clients;
using HitRefresh.WebSuit.Messaging;

namespace HitRefresh.WebSuit.Core.Services;

public class WebSuitConsumerIODriver(IIOHub io, WebSuitConsumerClient client) : IDisposable
{
    /// <inheritdoc />
    public void Dispose()
    {
        client.OnPrintReceived -= Print;
        client.OnInterruptionReceived -= Interrupt;
    }

    public void Enable()
    {
        client.OnPrintReceived += Print;
        client.OnInterruptionReceived += Interrupt;
    }

    private void Print(PrintUnitTransfer context)
    {
        io.Write(context.ToPrintUnit());
    }

    private async void Interrupt(int id, WebSuitInterruptionType type)
    {
        switch (type)
        {
        case WebSuitInterruptionType.Halt: break; // TODO Halt handle
        case WebSuitInterruptionType.Readline: await client.SendInputAsync(id, await io.ReadLineAsync() ?? ""); break;
        case WebSuitInterruptionType.ReadChar: await client.SendInputAsync(id, io.Read().ToString()); break;
        case WebSuitInterruptionType.PeekChar: await client.SendInputAsync(id, io.Peek().ToString()); break;
        case WebSuitInterruptionType.ReadToEnd: await client.SendInputAsync(id, await io.ReadToEndAsync() ?? ""); break;
        default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}