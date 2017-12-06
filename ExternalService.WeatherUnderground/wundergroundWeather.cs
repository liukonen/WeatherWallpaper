using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeatherDesktop.Interface;
using System.ComponentModel.Composition;

namespace InternalService
{
    [Export(typeof(WeatherDesktop.Interface.ISharedWeatherinterface))]
    [ExportMetadata("ClassName", "wundergroundWeather")]
    class wundergroundWeather : wundergroundAPIBase, ISharedWeatherinterface
    {
        //wundergroundAPIBase cache;
       // public void Load() { }
        public override ISharedResponse Invoke()
        {
            //cache = new wundergroundAPIBase();
            //base.Invoke();
            Call();
            return ReturnValue(SharedType.Weather);
        }
    }
}
