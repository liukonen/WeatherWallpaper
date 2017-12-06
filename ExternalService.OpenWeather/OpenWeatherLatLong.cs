using System.ComponentModel.Composition;
using WeatherDesktop.Interface;

namespace InternalService
{
    [Export(typeof(WeatherDesktop.Interface.ILatLongInterface))]
    [ExportMetadata("ClassName", "OpenWeatherLatLong")]
    class OpenWeatherLatLong : OpenWeatherAPIBase, ILatLongInterface
    {

        public new void Load() { Invoke(); }

        public double Latitude() { if (Response != null) { return Response.coord.lat; } return 0; }

        public double Longitude() { if (Response != null) { return Response.coord.lon; } return 0; }

        public bool worked() { return (Response != null && Response.coord != null); }

        public override string Debug()
        {
            return base.Debug();
        }
    }
}
