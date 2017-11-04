using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeatherDesktop.Interface
{
    class OpenWeatherLatLong : OpenWeatherAPIBase, ILatLongInterface
    {
        public OpenWeatherLatLong()
        {
            Invoke();
        }

        public double Latitude() { if (Response != null) { return Response.coord.lat; } return 0; }

        public double Longitude() { if (Response != null) { return Response.coord.lon; } return 0; }

        public bool worked() { return (Response != null && Response.coord != null); }

        public override string Debug()
        {
            return base.Debug();
        }
    }
}
