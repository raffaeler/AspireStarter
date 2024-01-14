using System.Diagnostics.Metrics;

namespace AspireStarter.ApiService;

public class WeatherMetrics
{
    public static readonly string WeatherMetricsName = "WeatherApi";
    private readonly Counter<int> _forecastCounter;
    public WeatherMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(WeatherMetricsName);
        _forecastCounter = meter.CreateCounter<int>("weatherapi.forecast_count");
    }

    internal void ForecastRequested(WeatherForecast forecast)
        => _forecastCounter.Add(1,
            new KeyValuePair<string, object?>("forecast date", forecast.Date),
            new KeyValuePair<string, object?>("forecast temperature", forecast.TemperatureC)
            );
}
