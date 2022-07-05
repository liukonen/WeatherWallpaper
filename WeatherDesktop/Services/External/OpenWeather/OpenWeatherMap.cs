using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherDesktop.Interface;
using WeatherDesktop.Share;
using WeatherDesktop.Services.External.OpenWeather.Objects;

namespace WeatherDesktop.Services.External.OpenWeather
{

    [Export(typeof(ISharedWeatherinterface))]
    [ExportMetadata("ClassName", "OpenWeatherMap")]
    class OpenWeatherMap : OpenWeatherAPIBase, ISharedWeatherinterface
    {

        public OpenWeatherMap() { }

        public new void Load()
        {
            if (string.IsNullOrWhiteSpace(APIKey)) { EnterAPIKey(); }
            if (string.IsNullOrWhiteSpace(ZipCode)) { SharedObjects.ZipObjects.TryGetZip(); }
            Invoke();
        }


        #region Weather values

        public override ISharedResponse Invoke()
        {
            base.Invoke();
            WeatherResponse wresposne = new WeatherResponse
            {
                Temp = (int)Response.Main.Temp,
                ForcastDescription = GenerateForcast(Response.Main, Response.Weather[0]),
                WType = GetWeatherType(Response.Weather[0].Id)
            };
            return wresposne;
        }

        #endregion

        #region Helpers
        private static SharedObjects.WeatherTypes GetWeatherType(int ParseItem)
        {
            double value = ParseItem / 100;

            //first the groups
            switch ((int)Math.Floor(value))
            {
                case 2:
                case 960:
                case 961:
                    return SharedObjects.WeatherTypes.ThunderStorm;
                case 3:
                case 5:
                    return SharedObjects.WeatherTypes.Rain;
                case 6:
                    return SharedObjects.WeatherTypes.Snow;
            }


            switch (ParseItem)
            {
                case 701:
                    return SharedObjects.WeatherTypes.Rain;
                case 711:
                    return SharedObjects.WeatherTypes.Smoke;
                case 721:
                    return SharedObjects.WeatherTypes.Haze;
                case 741:
                    return SharedObjects.WeatherTypes.Fog;
                case 731:
                case 751:
                case 761:
                case 762:
                    return SharedObjects.WeatherTypes.Dust;
                case 800:
                case 951:
                case 952:
                case 953:
                case 955:
                    return SharedObjects.WeatherTypes.Clear;
                case 801:
                case 802:
                    return SharedObjects.WeatherTypes.PartlyCloudy;
                case 803:
                case 804:
                    return SharedObjects.WeatherTypes.Cloudy;
                case 903:
                    return SharedObjects.WeatherTypes.Frigid;
                case 904:
                    return SharedObjects.WeatherTypes.Hot;
                case 905:
                case 954:
                case 956:
                case 957:
                case 958:
                    return SharedObjects.WeatherTypes.Windy;

            }
            return SharedObjects.WeatherTypes.ThunderStorm;// In the act of Some of the Extremes I did not cover... Thumderstorm it is
            //list of items directly not covered: 771 squalls, 781 tornado, 900 tornado, 901 tropical storm, 902 hurricane, 906 hail, 959 severe gale, 962 hurrican
        }

        private static string GenerateForcast(Main Mainweather, Weather WeatherObject)
        {
            var builder = new StringBuilder();
            return builder.AppendLine($"{Mainweather.Temp}, {WeatherObject.Description}")
                .AppendLine($"Humidity: {Mainweather.Humidity} Range: {Mainweather.Temp_min}-{Mainweather.Temp_max}")
                .ToString();
        }

        public override string Debug()
        {
            var debugValues = new Dictionary<string, string>
            {
                { "Temp", Response.Main.Temp.ToString() },
                { "min Temp", Response.Main.Temp_min.ToString() },
                { "max Temp", Response.Main.Temp_max.ToString() },
                { "Debug", base.Debug() }
            };
            return SharedObjects.CompileDebug(debugValues);
        }



        #endregion


    }
}
