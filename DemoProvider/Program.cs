using HitRefresh.MobileSuit;
using Microsoft.Extensions.Configuration;

var builder = Suit.CreateBuilder().AsWebSuitProvider();

builder.Configuration.AddJsonFile("demo.json");
builder.Build().Run();