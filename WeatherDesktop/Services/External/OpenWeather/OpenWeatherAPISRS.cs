using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherDesktop.Interface;

namespace WeatherDesktop.Services.External.OpenWeather
{
    [Export(typeof(WeatherDesktop.Interface.IsharedSunRiseSetInterface))]
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
