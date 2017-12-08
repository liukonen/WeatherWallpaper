namespace WeatherDesktop.Interface
{
	public class WeatherResponse : ISharedResponse
    {
		public Shared.SharedObjects.WeatherTypes WType;
        public int Temp;
        public string ForcastDescription;	
	}
}