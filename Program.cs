using BlueDevilBridges.Components;
using BlueDevilBridges.Data;
using Data_analytics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddScoped<PairScore>();

builder.Services.AddSingleton<PairingStatisticsService>();

// add database context
builder.Services.AddDbContext<BlueDevilBridgesContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("BlueDevilBridgesContext")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();