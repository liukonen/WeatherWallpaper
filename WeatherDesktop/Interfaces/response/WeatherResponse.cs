using System;

namespace WeatherDesktop.Interfaces
{
	public class WeatherResponse: SharedResponse
    {
		public Shared.WeatherTypes WType;
		public int Temp;
		public string ForcastDescription;	
	}

    public abstract class SharedResponse { }
}