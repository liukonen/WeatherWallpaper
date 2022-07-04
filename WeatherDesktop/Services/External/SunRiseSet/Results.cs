using Newtonsoft.Json;


namespace WeatherDesktop.Services.External.SunRiseSet
{
        public partial class Results
    {

        [JsonProperty("sunrise")]
        public string Sunrise { get; set; }

        [JsonProperty("sunset")]
        public string Sunset { get; set; }

        [JsonProperty("solar_noon")]
        public string Solar_noon { get; set; }

        [JsonProperty("day_length")]
        public string Day_length { get; set; }

    }
}
