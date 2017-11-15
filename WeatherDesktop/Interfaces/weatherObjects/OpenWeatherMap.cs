using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualBasic;

using WeatherDesktop.Shared;

namespace WeatherDesktop.Interface
{
    class OpenWeatherMap : OpenWeatherAPIBase, ISharedWeatherinterface
    {




        public OpenWeatherMap()
        {
            if (string.IsNullOrWhiteSpace(APIKey)) { EnterAPIKey(); }
            if (string.IsNullOrWhiteSpace(ZipCode)) { Shared.tryGetZip(); }
            Invoke();
        }



        #region Weather values

        public override ISharedResponse Invoke()
        {
            base.Invoke();
            WeatherResponse wresposne = new WeatherResponse();

            wresposne.Temp = (int)Response.main.temp;
            wresposne.ForcastDescription = GenerateForcast(Response.main, Response.weather[0]);
            wresposne.WType = GetWeatherType(Response.weather[0].id);
            return wresposne;
        }         

        #endregion

        #region Helpers
        private Shared.WeatherTypes GetWeatherType(int ParseItem)
        {
            double value = ParseItem / 100;

            //first the groups
            switch ((int)Math.Floor(value))
            {
                case 2:
                case 960:
                case 961:
                    return Shared.WeatherTypes.ThunderStorm;
                case 3:
                case 5:
                    return Shared.WeatherTypes.Rain;
                case 6:
                    return Shared.WeatherTypes.Snow;
            }


            switch (ParseItem)
            {
                case 701:
                    return Shared.WeatherTypes.Rain;
                case 711:
                    return Shared.WeatherTypes.Smoke;
                case 721:
                    return Shared.WeatherTypes.Haze;
                case 741:
                    return Shared.WeatherTypes.Fog;
                case 731:
                case 751:
                case 761:
                case 762:
                    return Shared.WeatherTypes.Dust;
                case 800:
                case 951:
                case 952:
                case 953:
                case 955:
                    return Shared.WeatherTypes.Clear;
                case 801:
                case 802:
                    return Shared.WeatherTypes.PartlyCloudy;
                case 803:
                case 804:
                    return Shared.WeatherTypes.Cloudy;
                case 903:
                    return Shared.WeatherTypes.Frigid;
                case 904:
                    return Shared.WeatherTypes.Hot;
                case 905:
                case 954:
                case 956:
                case 957:
                case 958:
                    return Shared.WeatherTypes.Windy;

            }
            return Shared.WeatherTypes.ThunderStorm;// In the act of Some of the Extremes I did not cover... Thumderstorm it is
            //list of items directly not covered: 771 squalls, 781 tornado, 900 tornado, 901 tropical storm, 902 hurricane, 906 hail, 959 severe gale, 962 hurrican
        }

        private string GenerateForcast(Main Mainweather, Weather WeatherObject)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(Mainweather.temp).Append(",  ").Append(WeatherObject.description).Append(Environment.NewLine);
            sb.Append("Humidity: ").Append(Mainweather.humidity).Append(" Range: ").Append(Mainweather.temp_min).Append("-").Append(Mainweather.temp_max);
            return sb.ToString();
        }

        public override string Debug()
        {
            System.Collections.Generic.Dictionary<string, string> debugValues = new Dictionary<string, string>();
            debugValues.Add("Temp", Response.main.temp.ToString());
            debugValues.Add("min Temp", Response.main.temp_min.ToString());
            debugValues.Add("max Temp", Response.main.temp_max.ToString());
            debugValues.Add("Debug", base.Debug());
            return Shared.CompileDebug("OpenWeatherMap", debugValues);
        }



        #endregion


    }


}

