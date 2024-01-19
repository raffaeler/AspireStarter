using System.Diagnostics;
using System.Security.Claims;

namespace AspireStarter.ApiService;

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    [LogPropertyIgnore]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public static class WeatherApi
{
    private static readonly string[] summaries = ["Freezing", "Bracing", "Chilly",
        "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    // RAF_TRACING
    private static ActivitySource _activitySource = new ActivitySource("RafWeatherApiTracing", "1.0.0");

    public static void MapWeatherApi(
        this IEndpointRouteBuilder app,
        IConfiguration configuration)
    {
        app.MapGet("/weatherforecast", (
            HttpRequest request,
            ClaimsPrincipal user,
            ILogger < Program> logger,
            WeatherMetrics weatherMetrics) =>
        {
            // RAF_TRACING
            using Activity? activity = _activitySource.StartActivity(
                name: "Activity GetWeatherForecast",
                kind: ActivityKind.Server,
                parentId: null,
                tags: [new KeyValuePair<string, object?>("Authenticated", user.Identity?.IsAuthenticated ?? false)]);

            var forecast = Enumerable.Range(1, 5).Select(index =>
            {
                var weatherMeasure = new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                );

                // RAF_LOGGING
                //LogHelper1.LogGetWeatherAction(logger, index, null);
                //LogHelper2.LogGetWeatherAction(logger, index, weatherMeasure);

                // RAF_TRACING
                activity?.AddTag("Freeze", weatherMeasure.TemperatureC < 0);

                // RAF_METRICS
                weatherMetrics.ForecastRequested(weatherMeasure);

                return weatherMeasure;
            })
            .ToArray();

            LogHelper2.LogGetForecast(logger, forecast);
            return forecast;
        });
    }

}


