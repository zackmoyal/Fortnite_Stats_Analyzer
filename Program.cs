using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FortniteStatsAnalyzer.Services;
using FortniteStatsAnalyzer.Configuration;

// Add services to the container
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // Adds support for MVC and Razor views

// Register HttpClient for API calls
builder.Services.AddHttpClient();

// Register FortniteStatsService
builder.Services.AddScoped<FortniteStatsService>(); // Adds FortniteStatsService to the DI container

// Register OpenAIService
builder.Services.AddScoped<IOpenAIService, OpenAIService>();

// Register FortniteApiSettings
builder.Services.Configure<FortniteApiSettings>(builder.Configuration.GetSection("FortniteApi"));
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));

// Register services
builder.Services.AddHttpClient<IFortniteApiService, FortniteApiService>();
builder.Services.AddHttpClient<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<IFortniteStatsService, FortniteStatsService>();

// Add logging
builder.Services.AddLogging();

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
