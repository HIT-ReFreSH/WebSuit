// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 23, 2024
//  *
//  */

using System.Drawing;
using HitRefresh.MobileSuit;

namespace HitRefresh.WebSuit.Core;

public record ColorTransfer(string Name, int Argb)
{
    public static ColorTransfer? FromColor(Color? color) => color is { } c ? new(c.Name, c.ToArgb()) : null;

    public Color ToColor() => string.IsNullOrWhiteSpace(Name) ? Color.FromName(Name) : Color.FromArgb(Argb);
}

[Serializable]
public record PrintUnitTransfer(string Text, ColorTransfer? Foreground, ColorTransfer? Background)
{
    public static PrintUnitTransfer FromPrintUnit
        (PrintUnit pu) => new(pu.Text, ColorTransfer.FromColor(pu.Foreground), ColorTransfer.FromColor(pu.Background));

    public PrintUnit ToPrintUnit()
        => new()
           {
               Text = Text,
               Foreground = Foreground?.ToColor(),
               Background = Background?.ToColor()
           };
}