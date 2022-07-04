using Newtonsoft.Json;
using System;


namespace WeatherDesktop.Services.External.OpenWeather.Objects
{
    internal partial class Sys
    {

        //[JsonProperty("type")]
        //public int Type { get; set; }

        //[JsonProperty("id")]
        //public int Id { get; set; }

        //[JsonProperty("message")]
        //public double Message { get; set; }

        //[JsonProperty("country")]
        //public string Country { get; set; }

        [JsonProperty("sunrise")]
        public Int64 Sunrise { get; set; }

        [JsonProperty("sunset")]
        public Int64 Sunset { get; set; }
    }
}
