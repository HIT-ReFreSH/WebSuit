using HitRefresh.MobileSuit;
using HitRefresh.WebSuit;
using Microsoft.Extensions.Configuration;

var builder = Suit.CreateBuilder().AsWebSuitProvider();

builder.Configuration.AddJsonFile("demo.json");
builder.UsePowerLine();
builder.MapClient<WebSuitDemo>();
var host=builder.Build();
host.Run();