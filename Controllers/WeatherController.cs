using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using TestProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace TestProject.Controllers
{
    public class WeatherController : Controller
    {
        private const string WeatherAPIUrl = "https://api.openweathermap.org/data/2.5/weather";

        private const string APIKey = "50387d177612f05dddedd2d04efd3b1b";

        private static Dictionary<string, DateTime> LastWarningTimestamps = new Dictionary<string, DateTime>();


        public async Task<ActionResult> Index(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                //city = "Kyiv";
                city = GetLastUsedCity();
            }
            else
            {
                SaveLastUsedCity(city);
            }

            WeatherForecast weatherForecast = await GetWeatherForecast(city);

            return View(weatherForecast);
        }

        private async Task<WeatherForecast> GetWeatherForecast(string city)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"{WeatherAPIUrl}?units=metric&q={city}&appid={APIKey}");

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    dynamic weatherData = JsonConvert.DeserializeObject(responseData);

                    WeatherForecast weatherForecast = new WeatherForecast
                    {
                        City = city,
                        Precipitation = weatherData.weather[0].main,
                        MaxTemperature = Convert.ToDouble(weatherData.main.temp_max),
                        MinTemperature = Convert.ToDouble(weatherData.main.temp_min)
                    };

                    weatherForecast.RainWarning = ShouldWarnForRain(weatherForecast.City, weatherForecast.Precipitation);

                    return weatherForecast;
                }
                else
                {
                    throw new Exception($"Failed to retrieve weather forecast. Status code: {response.StatusCode}");
                }
            }
        }

        private bool ShouldWarnForRain(string city, string precipitation)
        {
            if (LastWarningTimestamps.ContainsKey(city))
            {
                return false;
            }

            if (precipitation == "Rain")
            {
                LastWarningTimestamps[city] = DateTime.Now;
                return true;
            }

            return false;
        }

        private void SaveLastUsedCity(string city)
        {
            Response.Cookies.Append("LastUsedCity", city, new CookieOptions
            {
                Expires = DateTime.Now.AddDays(1)
            });
        }

        private string GetLastUsedCity()
        {
            return Request.Cookies["LastUsedCity"];
        }
    }
}

