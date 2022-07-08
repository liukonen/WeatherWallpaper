using System;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;

namespace WeatherDesktop.Services.External.OpenWeather
{
    [Export(typeof(IsharedSunRiseSetInterface))]
    [ExportMetadata("ClassName", "OpenWeatherAPISRS")]
    class OpenWeatherAPISRS : OpenWeatherAPIBase, IsharedSunRiseSetInterface
    {
        public override ISharedResponse Invoke()
        {
            base.Invoke();
            return new SunRiseSetResponse()
            {
                SunRise = FromJsonTime(Response.Sys.Sunrise),
                SunSet = FromJsonTime(Response.Sys.Sunset),
                Status = Status
            };
        }
        private static DateTime FromJsonTime(Int64 JsonTime) => new DateTime(1970, 1, 1).AddSeconds(JsonTime).ToLocalTime();
    }
}
