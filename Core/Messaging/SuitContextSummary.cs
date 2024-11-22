// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 22, 2024
//  *
//  */

using HitRefresh.MobileSuit;
using HitRefresh.MobileSuit.Core;

namespace HitRefresh.WebSuit.Messaging;

/// <summary>
/// Summarized from real SuitContext for easier transfer
/// </summary>
/// <param name="Request"></param>
/// <param name="Response"></param>
/// <param name="Status"></param>
/// <param name="ExceptionMessage"></param>
/// <param name="Properties"></param>
public record SuitContextSummary
(
    string[] Request,
    string? Response,
    RequestStatus Status,
    string? ExceptionMessage,
    Dictionary<string, string> Properties
)
{
    public static SuitContextSummary FromSuitContext(SuitContext context)
        => new
        (
            context.Request,
            context.Response,
            context.Status,
            context.Exception?.Message,
            context.Properties
        );

    public void CopyTo(SuitContext context)
    {
        context.Request = Request;
        context.Response = Response;
        context.Status = Status;
        context.Exception = ExceptionMessage is not null ? new Exception(ExceptionMessage) : null;
        foreach (var (key, value) in Properties)
        {
            context.Properties[key] = value;
        }

    }
}