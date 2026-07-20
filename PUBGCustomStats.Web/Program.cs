using AspNetStatic;
using AspNetStatic.Optimizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PUBGCustomStats.Data;
using System;
using System.Collections.Generic;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Process command-line parameters
// Supported keys (case-insensitive):
//  --output <path>
var config = builder.Configuration;

var outputPath = config["outputPath"];

// Add services to the container.
builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("PUBGCustomStatsContext");

if (connectionString != null)
{
    connectionString = connectionString.Replace("{AppDataPath}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
}

// Add database context
builder.Services.AddDbContext<PUBGCustomStatsContext>(options =>
    options.UseSqlite(connectionString));

// Dynamically generate session PageResources from the database
List<ResourceInfoBase> allResources = new List<ResourceInfoBase>();

allResources.Add(new PageResource("/nonplayers"));
allResources.Add(new PageResource("/nonplayer/BlueZone") { OutFile = "nonplayer/bluezone.html" });
allResources.Add(new PageResource("/nonplayer/RedZone") { OutFile = "nonplayer/redzone.html" });
allResources.Add(new PageResource("/nonplayer/BlackZone") { OutFile = "nonplayer/blackzone.html" });
allResources.Add(new PageResource("/nonplayer/BOT") { OutFile = "nonplayer/bot.html" });
allResources.Add(new PageResource("/nonplayer/Helicopter") { OutFile = "nonplayer/helicopter.html" });
allResources.Add(new PageResource("/nonplayer/Bear") { OutFile = "nonplayer/bear.html" });
allResources.Add(new PageResource("/nonplayer/Drowning") { OutFile = "nonplayer/drowning.html" });
allResources.Add(new PageResource("/nonplayer/JerryCan") { OutFile = "nonplayer/jerrycan.html" });
allResources.Add(new PageResource("/nonplayer/Suicide") { OutFile = "nonplayer/suicide.html" });
allResources.Add(new PageResource("/nonplayer/Guard") { OutFile = "nonplayer/guard.html" });
allResources.Add(new PageResource("/nonplayer/Commander") { OutFile = "nonplayer/commander.html" });
allResources.Add(new PageResource("/nonplayer/Lava") { OutFile = "nonplayer/lava.html" });

using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PUBGCustomStatsContext>();

    allResources.AddRange(db.Players.Select(player =>
        new PageResource($"/player/{player.PlayerGuid}") { OutFile = $"player/{player.PlayerName.Replace(' ', '-').ToLower()}.html" }
    ));

    allResources.AddRange(db.Sessions.Select(session =>
        new PageResource($"/session?sessionGuid={session.SessionGuid}") { OutFile = $"session/{session.StartDateTime.Value.ToString("yyyy-MM-dd")}.html" }
    ));

    allResources.AddRange(db.Matches.Select(match =>
        new PageResource($"/match?matchGuid={match.MatchGuid}") { OutFile = $"match/{match.StartTime.Value.ToLocalTime().ToString("yyyy-MM-dd-HHmm")}.html" }
    ));

}

allResources.Add(new PageResource("/"));
allResources.Add(new PageResource("/index"));
allResources.Add(new PageResource("/charts"));
allResources.Add(new PageResource("/clans"));



allResources.Add(new CssResource("/css/site.css"));
allResources.Add(new CssResource("/lib/bootstrap/dist/css/bootstrap.min.css"));
allResources.Add(new CssResource("/PUBGCustomStats.Web.styles.css"));
allResources.Add(new JsResource("/lib/jquery/dist/jquery.min.js"));
allResources.Add(new JsResource("/lib/bootstrap/dist/js/bootstrap.bundle.min.js"));
allResources.Add(new JsResource("/js/site.js"));
allResources.Add(new BinResource("/favicon.ico"));
allResources.Add(new BinResource("/favicon-96x96.png"));
allResources.Add(new BinResource("/favicon.svg"));
allResources.Add(new BinResource("/apple-touch-icon.png"));
allResources.Add(new BinResource("/icon-192.png"));
allResources.Add(new BinResource("/icon-512.png"));
allResources.Add(new BinResource("/site.webmanifest"));

builder.Services.AddSingleton<IStaticResourcesInfoProvider>(
  new StaticResourcesInfoProvider(allResources)
  );




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Handle non-success status codes (e.g., 404) by re-executing to a status page
app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

if (outputPath != null)
{
    if (Path.Exists(outputPath))
    {
        app.GenerateStaticContent(outputPath);
    }
    else
    {
        Console.WriteLine($"Output path not found: {outputPath}");
    }
}

app.Run();
