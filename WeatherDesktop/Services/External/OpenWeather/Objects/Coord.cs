

using Newtonsoft.Json;

namespace WeatherDesktop.Services.External.OpenWeather.Objects
{
    internal partial class Coord
    {

        [JsonProperty("lon")]
        public double Lon { get; set; }

        [JsonProperty("lat")]
        public double Lat { get; set; }
    }
}
