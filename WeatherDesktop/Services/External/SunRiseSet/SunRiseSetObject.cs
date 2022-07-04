using Newtonsoft.Json;


namespace WeatherDesktop.Services.External.SunRiseSet
{
    partial class SunRiseSetObject
    {

        [JsonProperty("results")]
        public Results Results { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
