using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MusicWebApp;
using MusicWebApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Cấu hình YouTube API (đọc từ wwwroot/appsettings.json)
var youTubeOptions = builder.Configuration.GetSection("YouTube").Get<YouTubeOptions>() ?? new YouTubeOptions();
builder.Services.AddSingleton(youTubeOptions);

builder.Services.AddScoped<IYouTubeMusicService, YouTubeMusicService>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<MusicPlayerState>();
builder.Services.AddScoped<FavoritesService>();
builder.Services.AddScoped<RecentlyPlayedService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<PlaylistService>();

await builder.Build().RunAsync();
