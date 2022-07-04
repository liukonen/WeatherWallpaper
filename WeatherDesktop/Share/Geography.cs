using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherDesktop.Share
{
    internal class Geography
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Geography(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
