using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeatherDesktop.Shared
{
    public static class KnownTypes
    {
        public static Type[] WeatherTypes = new Type[] { typeof(Interface.MSWeather), typeof(Interface.Mock_Weather), typeof(Interface.OpenWeatherMap) };
        public static Type[] SunRiseSetTypes = new Type[] { typeof(Interface.SunRiseSet), typeof(Interface.Mock_SunRiseSet) };
        public static Type[] LatLongTypes = new Type[] { typeof(Interface.SystemLatLong), typeof(Interface.MSWeather) };
    }
}
