using System.ComponentModel.Composition;
using WeatherDesktop.Interface;

namespace WeatherDesktop.Services.External.OpenWeather
{
    [Export(typeof(ILatLongInterface))]
    [ExportMetadata("ClassName", "OpenWeatherLatLong")]
    class OpenWeatherLatLong : OpenWeatherAPIBase, ILatLongInterface
    {
        public new void Load() => Invoke(); 
        public double Latitude() => (Response != null) ? Response.Coord.Lat: 0; 
        public double Longitude() => (Response != null) ? Response.Coord.Lon: 0; 
        public bool worked() => (Response != null && Response.Coord != null); 
        public override string Debug() => base.Debug();       
    }
}
