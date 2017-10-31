namespace WeatherDesktop.Interface
{
	public class WeatherResponse: ISharedResponse
    {
		public Shared.WeatherTypes WType;
        public int Temp;
        public string ForcastDescription;	
	}
}