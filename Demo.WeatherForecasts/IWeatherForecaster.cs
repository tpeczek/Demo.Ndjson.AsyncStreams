using System.Threading.Tasks;

namespace Demo.WeatherForecasts
{
    public interface IWeatherForecaster
    {
        Task<WeatherForecast> GetWeatherForecastAsync(int daysFromToday);
    }
}
