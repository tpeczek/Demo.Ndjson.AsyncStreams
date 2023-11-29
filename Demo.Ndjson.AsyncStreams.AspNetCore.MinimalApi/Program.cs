using Demo.WeatherForecasts;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IWeatherForecaster, WeatherForecaster>();

var app = builder.Build();

app.MapGet("/", () => "-- Demo.Ndjson.AsyncStreams.AspNetCore.MinimalApi --");

app.MapGet("/api/WeatherForecasts", async (IWeatherForecaster weatherForecaster, CancellationToken cancellationToken) =>
{
    List<WeatherForecast> weatherForecasts = new();

    for (int daysFromToday = 1; daysFromToday <= 10; daysFromToday++)
    {
        weatherForecasts.Add(await weatherForecaster.GetWeatherForecastAsync(daysFromToday, cancellationToken));
    };

    return weatherForecasts;
});

// This endpoint always returns NDJSON.
//app.MapGet("/api/WeatherForecasts/stream", (IWeatherForecaster weatherForecaster, CancellationToken cancellationToken) => Results.Ndjson(StreamWeatherForecastsAsync(weatherForecaster, cancellationToken)));

// This action accepts NDJSON.
//app.MapPost("/api/WeatherForecasts/stream", async (IAsyncEnumerable<WeatherForecast> weatherForecasts, ILogger logger) =>
//{
//    await foreach (WeatherForecast weatherForecast in weatherForecasts)
//    {
//        logger.LogInformation($"{weatherForecast.Summary} ({DateTime.UtcNow})");
//    }

//    return Results.Ok;
//});

// This action returns JSON or NDJSON depending on Accept request header.
app.MapGet("/api/WeatherForecasts/negotiate-stream", (IWeatherForecaster weatherForecaster, CancellationToken cancellationToken) => StreamWeatherForecastsAsync(weatherForecaster, cancellationToken));

app.Run();

async IAsyncEnumerable<WeatherForecast> StreamWeatherForecastsAsync(IWeatherForecaster weatherForecaster, [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    for (int daysFromToday = 1; daysFromToday <= 10; daysFromToday++)
    {
        WeatherForecast weatherForecast = await weatherForecaster.GetWeatherForecastAsync(daysFromToday, cancellationToken);

        yield return weatherForecast;
    };
}
