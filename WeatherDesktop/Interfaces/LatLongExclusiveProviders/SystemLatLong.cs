using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Device.Location;

namespace WeatherDesktop.Interface
{
    class SystemLatLong : ILatLongInterface
    {
        private bool _DidItWork = false;
        KeyValuePair<double, double> _LatLong = new KeyValuePair<double, double>(0, 0);
        public SystemLatLong()
        {
           if (_LatLong.Key == 0 && _LatLong.Value == 0)
            {
                _LatLong = GetLocationProperty(out _DidItWork);
            }
        }

        static KeyValuePair<double, double> GetLocationProperty(out bool worked)
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();

            // Do not suppress prompt, and wait 1000 milliseconds to start.
            watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));
            GeoCoordinate coord = watcher.Position.Location;
            if (coord.IsUnknown != true)
            {
                worked = true;
                return new KeyValuePair<double, double>(coord.Latitude, coord.Longitude);
            }
            worked = false;
            return new KeyValuePair<double, double>();
        }

   

        public bool worked()
        {
            return _DidItWork;
        }
        public double Latitude() { return _LatLong.Key; }



        public double Longitude() { return _LatLong.Value; }

    }




}

