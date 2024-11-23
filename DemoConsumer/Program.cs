using HitRefresh.MobileSuit;
using HitRefresh.WebSuit;
using Microsoft.Extensions.Configuration;

var builder = Suit.CreateBuilder().AsWebSuitConsumer();

builder.Configuration.AddJsonFile("demo.json");
builder.UseRemoteShell();
builder.MapRemoteClient<WebSuitDemo>();
builder.Use4BitColorIO().UsePowerLine();
builder.Build().Run();