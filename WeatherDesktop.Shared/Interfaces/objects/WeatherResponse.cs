namespace WeatherDesktop.Interface
{
	public class WeatherResponse : ISharedResponse
    {
		public Share.SharedObjects.WeatherTypes WType;
        public int Temp;
        public string ForcastDescription;	
	}
}