// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 23, 2024
//  *
//  */

using HitRefresh.MobileSuit;
using HitRefresh.MobileSuit.Core;
using HitRefresh.MobileSuit.Core.Services;

namespace HitRefresh.WebSuit.Core.Services;

/// <summary>
///     Web SuitShell for Client App, only used to Judge whether command is executable.
/// </summary>
public class WebSuitAppShell : SuitShell, ISuitShellCollection
{
    private readonly List<SuitShell> _members = new();

    private WebSuitAppShell() : base(typeof(object), _ => null, "WebSuitClient") { }

    /// <inheritdoc />
    public override int MemberCount => _members.Count;

    /// <summary>
    ///     Ordered members of this
    /// </summary>
    public IEnumerable<SuitShell> Members()
    {
        foreach (var shell in _members)
            switch (shell)
            {
            case SuitMethodShell method:
                yield return method;
                break;
            case SuitObjectShell obj:
            {
                foreach (var objMember in obj.Members()) yield return objMember;

                break;
            }
            default:
                yield return shell;
                break;
            }
    }

    internal static WebSuitAppShell FromClients(IEnumerable<SuitShell> clients)
    {
        var r = new WebSuitAppShell();
        r._members.AddRange(clients);
        return r;
    }

    /// <inheritdoc />
    public override Task Execute(SuitContext context) => Task.CompletedTask;

    /// <inheritdoc />
    public override bool MayExecute(IReadOnlyList<string> request)
    {
        return _members.Any(sys => sys.MayExecute(request));
    }
}