using System;
namespace TestProject.Models
{
    public class WeatherForecast
    {
        public string? City { get; set; }
        public string Precipitation { get; set; }
        public double MaxTemperature { get; set; }
        public double MinTemperature { get; set; }
        public bool RainWarning { get; set; }
    }
}

