using System;

namespace Internal.flatfile.Objects
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