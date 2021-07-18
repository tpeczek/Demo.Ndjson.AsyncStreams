using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ndjson.AsyncStreams.Net.Http;
using Demo.WeatherForecasts;

namespace Demo.Ndjson.AsyncStreams.Net.Http
{
    class Program
    {
        private static readonly IWeatherForecaster _weatherForecaster = new WeatherForecaster();

        static async Task Main(string[] args)
        {
            await ConsumeNdjsonStreamAsync().ConfigureAwait(false);

            await NegotiateJsonStreamAsync().ConfigureAwait(false);

            await NegotiateNdjsonStreamAsync().ConfigureAwait(false);

            await StreamNdjsonAsync().ConfigureAwait(false);

            Console.WriteLine("Press any key to exit . . .");
            Console.ReadKey(true);
        }

        private static async Task ConsumeNdjsonStreamAsync()
        {
            Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] Receving weather forecasts . . .");

            using HttpClient httpClient = new();

            using HttpResponseMessage response = await httpClient.GetAsync("https://localhost:5001/api/WeatherForecasts/stream", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            await foreach (WeatherForecast weatherForecast in response.Content!.ReadFromNdjsonAsync<WeatherForecast>().ConfigureAwait(false))
            {
                Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] {weatherForecast.Summary}");
            }

            Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] Weather forecasts has been received.");
        }

        private static async Task NegotiateJsonStreamAsync()
        {
            Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] Receving weather forecasts . . .");

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            using HttpResponseMessage response = await httpClient.GetAsync("https://localhost:5001/api/WeatherForecasts/negotiate-stream", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            IAsyncEnumerable<WeatherForecast> weatherForecasts = await response.Content.ReadFromJsonAsync<IAsyncEnumerable<WeatherForecast>>().ConfigureAwait(false);
            await foreach (WeatherForecast weatherForecast in weatherForecasts)
            {
                Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] {weatherForecast.Summary}");
            }

            Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] Weather forecasts has been received.");
        }

        private static async Task NegotiateNdjsonStreamAsync()
        {
            Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] Receving weather forecasts . . .");

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/x-ndjson");

            using HttpResponseMessage response = await httpClient.GetAsync("https://localhost:5001/api/WeatherForecasts/negotiate-stream", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            await foreach (WeatherForecast weatherForecast in response.Content!.ReadFromNdjsonAsync<WeatherForecast>().ConfigureAwait(false))
            {
                Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] {weatherForecast.Summary}");
            }

            Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] Weather forecasts has been received.");
        }

        private static async Task StreamNdjsonAsync()
        {
            static async IAsyncEnumerable<WeatherForecast> streamWeatherForecastsAsync()
            {
                for (int daysFromToday = 1; daysFromToday <= 10; daysFromToday++)
                {
                    WeatherForecast weatherForecast = await _weatherForecaster.GetWeatherForecastAsync(daysFromToday).ConfigureAwait(false);

                    yield return weatherForecast;
                };
            };

            Console.WriteLine("Sending weather forecasts . . .");

            using HttpClient httpClient = new();

            using HttpResponseMessage response = await httpClient.PostAsNdjsonAsync("https://localhost:5001/api/WeatherForecasts/stream", streamWeatherForecastsAsync()).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            Console.WriteLine("Weather forecasts has been send.");
        }
    }
}
