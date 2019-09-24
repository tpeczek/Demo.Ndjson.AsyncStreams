using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Lib.AspNetCore.Mvc.Ndjson;

namespace Demo.AspNetCore.Mvc.FetchStreaming.Controllers
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

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            List<WeatherForecast> weatherForecasts = new List<WeatherForecast>();

            Random rng = new Random();

            for (int index = 1; index <= 10; index++)
            {
                await Task.Delay(100);

                weatherForecasts.Add(CreateWeatherForecast(index, rng));
            };

            return weatherForecasts;
        }

        [HttpGet("stream")]
        public NdjsonStreamResult GetStream()
        {
            NdjsonStreamResult result = new NdjsonStreamResult();

            _ = StreamAsync(result);

            return result;
        }

        private async Task StreamAsync(NdjsonStreamResult result)
        {
            Random rng = new Random();

            for (int index = 1; index <= 10; index++)
            {
                await Task.Delay(100);

                await result.WriteAsync(CreateWeatherForecast(index, rng));
            };

            result.Complete();
        }

        private static WeatherForecast CreateWeatherForecast(int index, Random rng)
        {
            return new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(index).ToString("d"),
                TemperatureC = rng.Next(-20, 55),
                Summary = SUMMARIES[rng.Next(SUMMARIES.Length)]
            };
        }
    }
}
