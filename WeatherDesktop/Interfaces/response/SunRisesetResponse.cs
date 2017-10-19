using System;

namespace WeatherDesktop.Interfaces
{
    public class SunRiseSetResponse :SharedResponse
    {
        public DateTime SunRise;
        public DateTime SunSet;
        public DateTime SolarNoon;
        public string Status;
    }
}
