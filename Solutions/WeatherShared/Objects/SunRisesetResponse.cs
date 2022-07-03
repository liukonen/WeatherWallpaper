using System;
using WeatherShared.Interface;

namespace WeatherShared.Objects
{
    public class SunRiseSetResponse : ISharedResponse
    {
        public DateTime SunRise;
        public DateTime SunSet;
        public DateTime SolarNoon;
        public string Status;
    }
}
