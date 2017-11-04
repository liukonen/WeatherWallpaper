using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeatherDesktop.Interface
{
    class OpenWeatherAPISRS :OpenWeatherAPIBase, IsharedSunRiseSetInterface
    {
        public override ISharedResponse Invoke()
        {
            base.Invoke();
            SunRiseSetResponse srsResponse = new SunRiseSetResponse();
            srsResponse.SunRise = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Response.sys.sunrise).ToLocalTime();
            srsResponse.SunSet = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Response.sys.sunset).ToLocalTime();
            srsResponse.Status = Status;
            return srsResponse;
        }

    }
}
