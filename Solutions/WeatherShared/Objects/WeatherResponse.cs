using WeatherShared.Enums;
using WeatherShared.Interface;

namespace WeatherDesktop.Objects
{
	public class WeatherResponse : ISharedResponse
    {
		public WeatherTypes WType;
        public int Temp;
        public string ForcastDescription;	
	}
}