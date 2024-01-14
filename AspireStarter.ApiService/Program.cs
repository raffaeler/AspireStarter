using AspireStarter.ApiService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<WeatherMetrics>();    // RAF
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapWeatherApi(app.Configuration);

// RAF_GRAFANA_1
app.MapGet("startup", () =>
{
    return new
    {
        GrafanaUrl = (string)builder.Configuration["GRAFANA_URL"]!
    };
});


app.MapDefaultEndpoints();

app.Run();



