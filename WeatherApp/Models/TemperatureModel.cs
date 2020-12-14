using System;

namespace WeatherApp.Models
{
    public class TemperatureModel
    {
        public string City { get; set; }
        public DateTime DateTime { get; set; }
        public double Temperature { get; set; }

        public override string ToString()
        {

            return $"À {DateTime.ToLocalTime()} à {City}, il faisait {Temperature}°C";
        }
    }
}