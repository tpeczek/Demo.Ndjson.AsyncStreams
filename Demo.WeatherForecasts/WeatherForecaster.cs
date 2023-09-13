using System;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.WeatherForecasts
{
    public class WeatherForecaster : IWeatherForecaster
    {
        private static readonly string[] SUMMARIES = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly Random _random = new();

        public async Task<WeatherForecast> GetWeatherForecastAsync(int daysFromToday, CancellationToken cancellationToken = default)
        {
            await Task.Delay(100, cancellationToken);

            return new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(daysFromToday).ToString("d"),
                TemperatureC = _random.Next(-20, 55),
                Summary = SUMMARIES[_random.Next(SUMMARIES.Length)]
            };
        }
    }
}
