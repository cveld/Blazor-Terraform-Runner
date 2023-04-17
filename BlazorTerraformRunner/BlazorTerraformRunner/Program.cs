using Blazored.Toast;
using BlazorTerraformRunner.Data;
using BlazorTerraformRunner.Models;
using BlazorTerraformRunner.Services;
using BlazorTerraformRunner.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddBlazoredToast();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<FileSystemMonitors>();
builder.Services.AddTransient<FileSystemMonitor>();
builder.Services.AddTransient<Vt100Streamer>();
builder.Services.AddSingleton<KubernetesClient>();
builder.Services.AddSingleton<TerraformRunner>();
builder.Services.AddScoped<TailService>(); 
builder.Services.AddScoped<MessageService>();
builder.Services.AddSingleton<MessageRepository>();
builder.Services.AddSingleton<TailRepository>();
builder.Services.AddHostedService<KubernetesMonitor>();
builder.Services.AddScoped<IFileSystemMonitorHandler, FileSystemMonitorHandler>();
builder.Services.AddTransient<Func<FileSystemMonitor>>(serviceProvider => () => serviceProvider.GetService<FileSystemMonitor>() ?? throw new NullReferenceException(nameof(FileSystemMonitor)));
builder.Services.AddTransient<Func<Vt100Streamer>>(serviceProvider => () => serviceProvider.GetService<Vt100Streamer>() ?? throw new NullReferenceException(nameof(Vt100Streamer)));
builder.Services.AddSingleton<WorkspaceConfigService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
