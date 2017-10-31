using System;

namespace WeatherDesktop.Interface
{
    public class SunRiseSetResponse :ISharedResponse
    {
        public DateTime SunRise;
        public DateTime SunSet;
        public DateTime SolarNoon;
        public string Status;
    }
}
