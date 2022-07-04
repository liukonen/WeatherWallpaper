
using Newtonsoft.Json;

namespace WeatherDesktop.Services.External.OpenWeather.Objects
{
    internal partial class Main
    {

        [JsonProperty("temp")]
        public double Temp { get; set; }

        [JsonProperty("humidity")]
        public int Humidity { get; set; }

        //[JsonProperty("pressure")]
        //public double Pressure { get; set; }

        [JsonProperty("temp_min")]
        public double Temp_min { get; set; }

        [JsonProperty("temp_max")]
        public double Temp_max { get; set; }
    }
}
