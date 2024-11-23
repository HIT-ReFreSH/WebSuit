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
    public static ColorTransfer? FromColor(Color? color)
    {
        return color is { } c ? new ColorTransfer(c.Name, c.ToArgb()) : null;
    }

    public Color ToColor() { return string.IsNullOrWhiteSpace(Name) ? Color.FromName(Name) : Color.FromArgb(Argb); }
}

[Serializable]
public record PrintUnitTransfer(string Text, ColorTransfer? Foreground, ColorTransfer? Background)
{
    public static PrintUnitTransfer FromPrintUnit(PrintUnit pu)
    {
        return new PrintUnitTransfer
            (pu.Text, ColorTransfer.FromColor(pu.Foreground), ColorTransfer.FromColor(pu.Background));
    }

    public PrintUnit ToPrintUnit()
    {
        return new PrintUnit
               {
                   Text = Text,
                   Foreground = Foreground?.ToColor(),
                   Background = Background?.ToColor()
               };
    }
}