﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ndjson.AsyncStreams.AspNetCore.Mvc;
using Demo.WeatherForecasts;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Demo.Ndjson.AsyncStreams.AspNetCore.Mvc
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherForecastsController : Controller
    {
        private readonly IWeatherForecaster _weatherForecaster;
        private readonly ILogger _logger;

        public WeatherForecastsController(IWeatherForecaster weatherForecaster, ILogger<WeatherForecastsController> logger)
        {
            _weatherForecaster = weatherForecaster;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get(CancellationToken cancellationToken)
        {
            List<WeatherForecast> weatherForecasts = new();

            for (int daysFromToday = 1; daysFromToday <= 10; daysFromToday++)
            {
                weatherForecasts.Add(await _weatherForecaster.GetWeatherForecastAsync(daysFromToday, cancellationToken));
            };

            return weatherForecasts;
        }

        [HttpGet("stream")]
        // This action always returns NDJSON.
        public NdjsonAsyncEnumerableResult<WeatherForecast> GetStream(CancellationToken cancellationToken)
        {
            return new NdjsonAsyncEnumerableResult<WeatherForecast>(StreamWeatherForecastsAsync(cancellationToken));
        }

        [HttpGet("negotiate-stream")]
        // This action returns JSON or NDJSON depending on Accept request header.
        public IAsyncEnumerable<WeatherForecast> NegotiateStream(CancellationToken cancellationToken)
        {
            return StreamWeatherForecastsAsync(cancellationToken);
        }

        [HttpPost("stream")]
        // This action accepts NDJSON.
        public async Task<IActionResult> PostStream(IAsyncEnumerable<WeatherForecast> weatherForecasts)
        {
            await foreach (WeatherForecast weatherForecast in weatherForecasts)
            {
                _logger.LogInformation($"{weatherForecast.Summary} ({DateTime.UtcNow})");
            }

            return Ok();
        }

        private async IAsyncEnumerable<WeatherForecast> StreamWeatherForecastsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int daysFromToday = 1; daysFromToday <= 10; daysFromToday++)
            {
                WeatherForecast weatherForecast = await _weatherForecaster.GetWeatherForecastAsync(daysFromToday, cancellationToken);

                yield return weatherForecast;
            };
        }
    }
}
