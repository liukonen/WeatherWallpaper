using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeatherDesktop.Interface
{
    class wundergroundWeather : wundergroundAPIBase, ISharedWeatherinterface
    {
        //wundergroundAPIBase cache;

        public override ISharedResponse Invoke()
        {
            //cache = new wundergroundAPIBase();
            //base.Invoke();
            Call();
            return ReturnValue(SharedType.Weather);
        }
    }
}
