using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ndjson.AsyncStreams.AspNetCore.Mvc;

namespace Demo.Ndjson.AsyncStreams.AspNetCore.Mvc
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherForecastsController : Controller
    {
        public class WeatherForecast
        {
            public string DateFormatted { get; set; }

            public int TemperatureC { get; set; }

            public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

            public string Summary { get; set; }
        }

        private static readonly string[] SUMMARIES = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private static readonly Random RANDOM = new();

        private readonly ILogger _logger;

        public WeatherForecastsController(ILogger<WeatherForecastsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            List<WeatherForecast> weatherForecasts = new();

            for (int daysFromToday = 1; daysFromToday <= 10; daysFromToday++)
            {
                weatherForecasts.Add(await GetWeatherForecastAsync(daysFromToday));
            };

            return weatherForecasts;
        }

        [HttpGet("stream")]
        public NdjsonAsyncEnumerableResult<WeatherForecast> GetStream()
        {
            static async IAsyncEnumerable<WeatherForecast> streamWeatherForecastsAsync()
            {
                for (int daysFromToday = 1; daysFromToday <= 10; daysFromToday++)
                {
                    WeatherForecast weatherForecast = await GetWeatherForecastAsync(daysFromToday);

                    yield return weatherForecast;
                };
            };

            return new NdjsonAsyncEnumerableResult<WeatherForecast>(streamWeatherForecastsAsync());
        }

        [HttpPost("stream")]
        public async Task<IActionResult> PostStream(IAsyncEnumerable<WeatherForecast> weatherForecasts)
        {
            await foreach (WeatherForecast weatherForecast in weatherForecasts)
            {
                _logger.LogInformation($"{weatherForecast.Summary} ({DateTime.UtcNow})");
            }

            return Ok();
        }

        private static async Task<WeatherForecast> GetWeatherForecastAsync(int daysFromToday)
        {
            await Task.Delay(1000);

            return new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(daysFromToday).ToString("d"),
                TemperatureC = RANDOM.Next(-20, 55),
                Summary = SUMMARIES[RANDOM.Next(SUMMARIES.Length)]
            };
        }
    }
}
