// /*
//  * Author: Ferdinand Su
//  * Email: ${User.Email}
//  * Date: 11 23, 2024
//  *
//  */

using HitRefresh.MobileSuit;
using HitRefresh.MobileSuit.UI;

namespace HitRefresh.WebSuit;

public class WebSuitDemo(IIOHub IO)
{
    [SuitAlias("H")]
    [SuitInfo("hello command.")]
    public void Hello()
    {
        IO.WriteLine("Hello! MobileSuit!");
    }

    [SuitAlias("cui")]
    public void CuiTest()
    {
        IO.Write("This Will Raise a Dead Lock. Only Async support.");
        var selected=IO.CuiSelectItemFrom("Select one", new[] { "x", "y", "z" }, null,
                                          (_, x) => x);
        var yes = IO.CuiYesNo($"You've selected {selected}, aren't you? SelectMany?");
        if (yes)
        {
            var selecteds = IO.CuiSelectItemsFrom("Select many", new[]
                                                                 {
                                                                     "w","x", "y", "z"
                                                                 }, null,
                                                  (_, x) => x);
            Console.WriteLine($@"{string.Join(",",selecteds)} is selected");
        }
    }

    [SuitAlias("art")]
    public async Task<string> AsyncReadlineTest()
    {
        return await IO.ReadLineAsync("I will Return whatever you write")??"";

    }

    public static object NumberConvert(string arg)
    {
        return int.Parse(arg);
    }

    [SuitAlias("Sn")]
    public void ShowNumber(int i)
    {
        IO.WriteLine(i.ToString());
    }

    [SuitAlias("Sn2")]
    public void ShowNumber2(int i, int[] j
    )
    {
        IO.WriteLine(i.ToString());
        IO.WriteLine(j.Length >= 1 ? j[0].ToString() : "");
    }
}