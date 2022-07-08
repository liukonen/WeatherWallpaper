using System;
using System.Collections.Generic;
using System.Device.Location;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;

namespace WeatherDesktop.Services.Internal
{
    [Export(typeof(ILatLongInterface))]
    [ExportMetadata("ClassName", "SystemLatLong")]
    class SystemLatLong : ILatLongInterface
    {
        private bool _DidItWork = false;
        KeyValuePair<double, double> _LatLong = new KeyValuePair<double, double>(0, 0);
        public SystemLatLong()
        {
            if (_LatLong.Key == 0 && _LatLong.Value == 0)
                _LatLong = GetLocationProperty(out _DidItWork);
        }

        static KeyValuePair<double, double> GetLocationProperty(out bool worked)
        {
            var watcher = new GeoCoordinateWatcher();
            // Do not suppress prompt, and wait 1000 milliseconds to start.
            watcher.TryStart(false, TimeSpan.FromMilliseconds(3000));
            GeoCoordinate coord = watcher.Position.Location;
            worked = !coord.IsUnknown;
            return (worked) ?
                new KeyValuePair<double, double>(coord.Latitude, coord.Longitude)
                : new KeyValuePair<double, double>();
        }
        public bool worked() => _DidItWork;
        public double Latitude() => _LatLong.Key; 
        public double Longitude() => _LatLong.Value; 

    }

}
