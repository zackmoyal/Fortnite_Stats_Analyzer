using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FortniteStatsAnalyzer.Services;
using FortniteStatsAnalyzer.Configuration;

// Add services to the container
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // Adds support for MVC and Razor views

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Register configuration
builder.Services.Configure<FortniteApiSettings>(builder.Configuration.GetSection("FortniteApiSettings"));
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAISettings"));

// Register services with proper DI
builder.Services.AddHttpClient();
builder.Services.AddScoped<IFortniteApiService, FortniteApiService>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<IFortniteStatsService, FortniteStatsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Custom error page in non-dev environments
    app.UseHsts(); // Adds HTTP Strict Transport Security
}

app.UseHttpsRedirection(); // Forces HTTPS
app.UseStaticFiles(); // Serves static files like CSS, JS, etc.
app.UseRouting();
app.UseAuthorization(); // Authorization middleware (if needed)

// Configure endpoints for controllers views
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Default routing to Home/Index

app.Run();
