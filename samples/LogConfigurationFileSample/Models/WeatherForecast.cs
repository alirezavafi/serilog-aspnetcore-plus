using System;

namespace LogConfigurationFileSample.Models
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int PasswordNumber { get; set; }

        public int TemperatureF => 32 + (int) (PasswordNumber / 0.5556);
        public string Token { get; set; } = "x1241432xfgdsfk$!~!@$@%$#^$";

        public string? Summary { get; set; }
    }
}