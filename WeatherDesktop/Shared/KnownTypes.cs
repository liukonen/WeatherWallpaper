using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeatherDesktop.Shared
{
    public static class KnownTypes
    {
        public static Type[] WeatherTypes = new Type[] { typeof(Interface.MSWeather), typeof(Interface.Mock_Weather), typeof(Interface.OpenWeatherMap), typeof(Interface.YahooWeather) };
        public static Type[] SunRiseSetTypes = new Type[] { typeof(Interface.SunRiseSet), typeof(Interface.Mock_SunRiseSet), typeof(Interface.OpenWeatherAPISRS), typeof(Interface.YahooSRS) };
        public static Type[] LatLongTypes = new Type[] { typeof(Interface.SystemLatLong), typeof(Interface.MSWeather), typeof(Interface.OpenWeatherLatLong), typeof(Interface.YahooLatLong), typeof(Interface.GovWeatherLatLong) };
    }
}
