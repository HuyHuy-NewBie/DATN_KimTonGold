var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// UI-only prototype: all screens are served as static assets.
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();
