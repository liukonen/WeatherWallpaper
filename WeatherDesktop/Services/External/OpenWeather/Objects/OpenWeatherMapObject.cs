using Newtonsoft.Json;
using System;


namespace WeatherDesktop.Services.External.OpenWeather.Objects
{
    internal partial class OpenWeatherMapObject
    {

        [JsonProperty("coord")]
        public Coord Coord { get; set; }

        [JsonProperty("sys")]
        public Sys Sys { get; set; }

        [JsonProperty("weather")]
        public Weather[] Weather { get; set; }

        //[JsonProperty("@base")]
        //public string Base { get; set; }

        [JsonProperty("main")]
        public Main Main { get; set; }

        //[JsonProperty("wind")]
        //public Wind Wind { get; set; }

        //[JsonProperty("clouds")]
        //public Clouds Clouds { get; set; }

        //[JsonProperty("dt")]
        //public Int64 Dt { get; set; }

        //[JsonProperty("id")]
        //public int Id { get; set; }

        //[JsonProperty("name")]
        //public string Name { get; set; }

        //[JsonProperty("cod")]
        //public int Cod { get; set; }
    }
}
