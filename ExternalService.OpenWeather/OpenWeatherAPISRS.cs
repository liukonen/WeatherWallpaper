using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;

namespace InternalService
{
    [Export(typeof(WeatherDesktop.Interface.IsharedSunRiseSetInterface))]
    [ExportMetadata("ClassName", "OpenWeatherAPISRS")]
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
