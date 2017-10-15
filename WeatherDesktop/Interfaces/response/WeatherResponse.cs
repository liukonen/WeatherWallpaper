using System;

namespace WeatherDesktop.Interfaces
{
	public class WeatherResponse
	{
		public Shared.WeatherTypes WType;
		public int Temp;
		public string ForcastDescription;	
	}
}