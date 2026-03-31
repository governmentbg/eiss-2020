using IOWebApplication.Extensions;
using Microsoft.AspNetCore.Builder;

//Console.WriteLine($"{DateTime.Now}: App START"); 
var builder = WebApplication.CreateBuilder(args);
builder.ConfigureBuilder();

builder.Services.ConfigureProgramServices(builder.Configuration);

//Console.WriteLine($"{DateTime.Now}: App BUILD");

var app = builder.Build();

app.ConfigureApplication(builder.Configuration);
//Console.WriteLine($"{DateTime.Now}: App RUN");
app.Run();