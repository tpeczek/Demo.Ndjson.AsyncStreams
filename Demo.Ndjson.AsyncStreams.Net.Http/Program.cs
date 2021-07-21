using System;
using System.IO;
using System.Text;
using System.Buffers;
using System.Net.Http;
using System.Text.Json;
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

            await NegotiateRawJsonStreamAsync().ConfigureAwait(false);

            await NegotiateJsonStreamAsync().ConfigureAwait(false);

            await NegotiateNdjsonStreamAsync().ConfigureAwait(false);

            await StreamNdjsonAsync().ConfigureAwait(false);

            Console.WriteLine("Press any key to exit . . .");
            Console.ReadKey(true);
        }

        private static async Task ConsumeNdjsonStreamAsync()
        {
            Console.WriteLine($"-- {nameof(ConsumeNdjsonStreamAsync)} --");
            Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] Receving weather forecasts . . .");

            using HttpClient httpClient = new();

            using HttpResponseMessage response = await httpClient.GetAsync("https://localhost:5001/api/WeatherForecasts/stream", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            await foreach (WeatherForecast weatherForecast in response.Content!.ReadFromNdjsonAsync<WeatherForecast>().ConfigureAwait(false))
            {
                Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] {weatherForecast.Summary}");
            }

            Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] Weather forecasts has been received.");
            Console.WriteLine();
        }

        private static async Task NegotiateRawJsonStreamAsync()
        {
            Console.WriteLine($"-- {nameof(NegotiateRawJsonStreamAsync)} --");
            Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] Receving weather forecasts . . .");

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            using HttpResponseMessage response = await httpClient.GetAsync("https://localhost:5001/api/WeatherForecasts/negotiate-stream", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            while (true)
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(128);
                int bytesRead = await responseStream.ReadAsync(buffer);

                Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] ({bytesRead}/{buffer.Length}) {Encoding.UTF8.GetString(buffer)}");

                ArrayPool<byte>.Shared.Return(buffer);

                if (bytesRead == 0)
                {
                    break;
                }
            }

            Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] Weather forecasts has been received.");
            Console.WriteLine();
        }

        private static async Task NegotiateJsonStreamAsync()
        {
            Console.WriteLine($"-- {nameof(NegotiateJsonStreamAsync)} --");
            Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] Receving weather forecasts . . .");

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            using HttpResponseMessage response = await httpClient.GetAsync("https://localhost:5001/api/WeatherForecasts/negotiate-stream", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            await foreach (WeatherForecast weatherForecast in JsonSerializer.DeserializeAsyncEnumerable<WeatherForecast>(responseStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, DefaultBufferSize = 128 }))
            {
                Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] {weatherForecast.Summary}");
            }

            Console.WriteLine($"[{DateTime.UtcNow:hh:mm:ss.fff}] Weather forecasts has been received.");
            Console.WriteLine();
        }

        private static async Task NegotiateNdjsonStreamAsync()
        {
            Console.WriteLine($"-- {nameof(NegotiateNdjsonStreamAsync)} --");
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
            Console.WriteLine();
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

            Console.WriteLine($"-- {nameof(StreamNdjsonAsync)} --");
            Console.WriteLine("Sending weather forecasts . . .");

            using HttpClient httpClient = new();

            using HttpResponseMessage response = await httpClient.PostAsNdjsonAsync("https://localhost:5001/api/WeatherForecasts/stream", streamWeatherForecastsAsync()).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            Console.WriteLine("Weather forecasts has been send.");
            Console.WriteLine();
        }
    }
}
