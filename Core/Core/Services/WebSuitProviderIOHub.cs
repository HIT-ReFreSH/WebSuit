// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using HitRefresh.MobileSuit;
using HitRefresh.MobileSuit.Core;
using HitRefresh.MobileSuit.Core.Services;
using HitRefresh.WebSuit.Clients;
using HitRefresh.WebSuit.Messaging;

namespace HitRefresh.WebSuit.Core.Services;

public class WebSuitProviderIOHub : IIOHub
{
    protected int InterruptionIdPointer;

    /// <summary>
    ///     Initialize a IOServer.
    /// </summary>
    public WebSuitProviderIOHub
    (
        PromptFormatter promptFormatter,
        IIOHubConfigurator configurator,
        WebSuitProviderClient client,
        WebSuitContextService context
    )
    {
        ColorSetting = IColorSetting.DefaultColorSetting;
        FormatPrompt = promptFormatter;
        Client = client;
        SessionId = context.SessionId;
        Client.OnInputReceived += HandleInput;
        configurator(this);
    }

    private ConcurrentDictionary<int, string> InputQueue { get; } = new();
    protected string SessionId { get; }
    private List<PrintUnit> Prefix { get; } = [];

    protected WebSuitProviderClient Client { get; }

    /// <inheritdoc />
    public IColorSetting ColorSetting { get; set; }

    /// <inheritdoc />
    public PromptFormatter FormatPrompt { get; }

    /// <inheritdoc />
    public TextReader Input
    {
        get => throw new InvalidOperationException();
        set => throw new InvalidOperationException();
    }

    /// <inheritdoc />
    public bool IsInputRedirected => throw new InvalidOperationException();


    /// <inheritdoc />
    public void ResetInput() { throw new InvalidOperationException(); }

    /// <inheritdoc />
    public string? ReadLine() { return WaitInputUntilGot(SendInterruption(WebSuitInterruptionType.Readline)); }

    /// <inheritdoc />
    public async Task<string?> ReadLineAsync()
    {
        return await WaitInputUntilGotAsync(await SendInterruptionAsync(WebSuitInterruptionType.Readline));
    }

    /// <inheritdoc />
    public int Peek() { return int.Parse(WaitInputUntilGot(SendInterruption(WebSuitInterruptionType.PeekChar))); }

    /// <inheritdoc />
    public int Read() { return int.Parse(WaitInputUntilGot(SendInterruption(WebSuitInterruptionType.ReadChar))); }

    /// <inheritdoc />
    public string ReadToEnd() { return WaitInputUntilGot(SendInterruption(WebSuitInterruptionType.ReadToEnd)); }

    /// <inheritdoc />
    public async Task<string> ReadToEndAsync()
    {
        return await WaitInputUntilGotAsync(await SendInterruptionAsync(WebSuitInterruptionType.ReadToEnd));
    }

    /// <inheritdoc />
    public IOOptions Options { get; set; }

    /// <inheritdoc />
    public bool IsErrorRedirected => throw new InvalidOperationException();

    /// <inheritdoc />
    public bool IsOutputRedirected => throw new InvalidOperationException();

    /// <inheritdoc />
    public TextWriter ErrorStream
    {
        get => throw new InvalidOperationException();
        set => throw new InvalidOperationException();
    }

    /// <inheritdoc />
    public TextWriter Output
    {
        get => throw new InvalidOperationException();
        set => throw new InvalidOperationException();
    }


    /// <inheritdoc />
    public void ResetError() { throw new InvalidOperationException(); }

    /// <inheritdoc />
    public void ResetOutput() { throw new InvalidOperationException(); }

    /// <inheritdoc />
    public void AppendWriteLinePrefix(PrintUnit prefix) { Prefix.Add(prefix); }


    /// <inheritdoc />
    public void SubtractWriteLinePrefix() { Prefix.RemoveAt(Prefix.Count - 1); }

    /// <inheritdoc />
    public void ClearWriteLinePrefix() { Prefix.Clear(); }

    /// <inheritdoc />
    public virtual void Write(PrintUnit content) { Client.SendPrintAsync(SessionId, content).GetAwaiter().GetResult(); }

    /// <inheritdoc />
    public virtual async Task WriteAsync(PrintUnit content) { await Client.SendPrintAsync(SessionId, content); }


    /// <inheritdoc />
    public IEnumerable<PrintUnit> GetLinePrefix(OutputType type)
    {
        if (!Options.HasFlag(IOOptions.DisableLinePrefix)) return Prefix;
        if (Options.HasFlag(IOOptions.DisableTag)) return Array.Empty<PrintUnit>();
        var sb = new StringBuilder();
        AppendTimeStamp(sb);
        sb.Append(IIOHub.GetLabel(type));
        return [(sb.ToString(), null)];
    }

    ~WebSuitProviderIOHub() { Client.OnInputReceived -= HandleInput; }

    private void HandleInput(string sessionId, int interruptionId, string input)
    {
        if (SessionId == sessionId) InputQueue[interruptionId] = input;
    }

    protected int SendInterruption(WebSuitInterruptionType type)
    {
        var id = InterruptionIdPointer;
        Interlocked.Increment(ref InterruptionIdPointer);
        _ = Client.SendInterruptionAsync(SessionId, id, type);
        return id;
    }

    protected async Task<int> SendInterruptionAsync(WebSuitInterruptionType type)
    {
        var id = InterruptionIdPointer;
        Interlocked.Increment(ref InterruptionIdPointer);
        await Client.SendInterruptionAsync(SessionId, id, type);
        return id;
    }

    private string WaitInputUntilGot(int id)
    {
        string? result;
        while (!InputQueue.TryGetValue(id, out result)) Thread.Sleep(100);

        return result;
    }

    private async Task<string> WaitInputUntilGotAsync(int id)
    {
        string? result;
        while (!InputQueue.TryGetValue(id, out result)) await Task.Delay(100);

        return result;
    }

    private static void AppendTimeStamp(StringBuilder sb)
    {
        sb.Append('[');
        sb.Append(DateTime.Now.ToString(CultureInfo.InvariantCulture));
        sb.Append(']');
    }
}