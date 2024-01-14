﻿namespace AspireStarter.ApiService;

// Logging old way
static class LogHelper1
{
    public static readonly Action<ILogger, int, Exception?> LogGetWeatherAction = LoggerMessage.Define<int>(
        LogLevel.Information, new EventId(101, "GetWeather"), "Getting weather forecast for {Days} days");
}

// Logging new way
static partial class LogHelper2
{
    [LoggerMessage(
        EventId = 102,
        Level = LogLevel.Information,
        Message = "Getting weather forecast for {days} days: {forecast}")]
    public static partial void LogGetWeatherAction(ILogger logger, int days, [LogProperties] WeatherForecast forecast);

    // ==================================================

    [LoggerMessage(
        EventId = 103,
        Level = LogLevel.Information,
        Message = "Getting weather forecast for {days} days: {forecast}")]
    public static partial void LogGetWeatherAction2(ILogger logger, int days,
        [TagProvider(typeof(LogHelper2), "FormatWhetherForecast")] WeatherForecast forecast);

    public static void FormatWhetherForecast(ITagCollector collector, WeatherForecast? forecast)
    {
        collector.Add("Weather Forecast Date", forecast?.Date);
        collector.Add("Weather Forecast Celsius", forecast?.TemperatureC);
        collector.Add("Weather Summary", forecast?.Summary);
    }

    // ==================================================

    [LoggerMessage(
        EventId = 104,
        Level = LogLevel.Information,
        Message = "Getting weather forecast: {forecast}")]
    public static partial void LogGetForecast(ILogger logger, IEnumerable<WeatherForecast> forecast);

}