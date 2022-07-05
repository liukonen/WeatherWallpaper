using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherDesktop.Shared.Handlers;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.Xml;
using WeatherDesktop.Interface;
using WeatherDesktop.Share;
using Newtonsoft.Json.Linq;
using WeatherDesktop.Shared.Extentions;

namespace WeatherDesktop.Services.External
{
    [Export(typeof(ISharedWeatherinterface))]
    [ExportMetadata("ClassName", "GovWeather3")]
    class GovWeather3 : ISharedWeatherinterface
    {
        
        private readonly string Gov_User = Properties.Gov3.User;
        const int UpdateInterval = 60;
        private string httpResponseHourly;
        private string httpResponseDaily;
        //private string _errors;
        private DateTime LastUpdated = DateTime.MinValue;
        Tuple<double, double> LatLong;
        private Exception _ThrownException = null;

        private Tuple<double, double> Grid;

        string ISharedInterface.Debug()
        {
            return new Dictionary<string, string>
            {
                { "Last updated", LastUpdated.ToString() }
            }.CompileDebug();
        }

        ISharedResponse ISharedInterface.Invoke()
        {
            if (MemCacheHandler.Instance.Exists(this.GetType().Name))  return MemCacheHandler.Instance.GetItem<WeatherResponse>(this.GetType().Name); 
            WeatherResponse response = new WeatherResponse();
            try
            {

                httpResponseHourly = SharedObjects.CompressedCallSite($"https://api.weather.gov/gridpoints/MKX/{Grid.Item1},{Grid.Item2}/forecast/hourly", Gov_User);
                httpResponseDaily = SharedObjects.CompressedCallSite($"https://api.weather.gov/gridpoints/MKX/{Grid.Item1},{Grid.Item2}/forecast", Gov_User);
                response = Transform(httpResponseDaily, httpResponseHourly);

                MemCacheHandler.Instance.SetItem(this.GetType().Name,response, UpdateInterval);
                LastUpdated = DateTime.Now;
                _ThrownException = null;
            }
            catch (Exception x) { _ThrownException = x;}
            return response;
        }

        void ISharedInterface.Load()
        {
            LatLong = GetLocationProperty();
            Grid = LookUpPoints(LatLong);
        }

        public MenuItem[] SettingsItems() { return new MenuItem[] { SharedObjects.ZipObjects.ZipMenuItem }; }

        Exception ISharedInterface.ThrownException() { return _ThrownException; }



        private static WeatherResponse Transform(string Daily, string Hourly)
        {
            WeatherResponse response = new WeatherResponse();
            string shortDescription;
            JObject item = JObject.Parse(Hourly);
            var X = item["properties"]["periods"][0];
            shortDescription = X.Value<string>("shortForecast");
            response.Temp = X.Value<int>("temperature");
            response.ForcastDescription = DetailedForcast(Daily);
            response.WType = Convert(shortDescription);
            return response;
        }
        private static string DetailedForcast(string DailyForcast)
        {
            JObject item = JObject.Parse(DailyForcast);
            var X = item["properties"]["periods"][0];
            return X.Value<string>("detailedForecast");
        }

        private static SharedObjects.WeatherTypes Convert(string description)
        {
            if (description.ToLower().Contains("thunderstorm")) { return SharedObjects.WeatherTypes.ThunderStorm; }
            if (description.ToLower().Contains("partly cloudy")) { return SharedObjects.WeatherTypes.PartlyCloudy; }
            if (description.ToLower().Contains("snow")) { return SharedObjects.WeatherTypes.Snow; }
            if (description.ToLower().Contains("cloudy")) { return SharedObjects.WeatherTypes.Cloudy; }
            if (description.ToLower().Contains("rain")) { return SharedObjects.WeatherTypes.Rain; }
            if (description.ToLower().Contains("dust")) { return SharedObjects.WeatherTypes.Dust; }
            if (description.ToLower().Contains("Fog")) { return SharedObjects.WeatherTypes.Fog; }
            if (description.ToLower().Contains("frigid")) { return SharedObjects.WeatherTypes.Frigid; }
            if (description.ToLower().Contains("haze")) { return SharedObjects.WeatherTypes.Haze; }
            if (description.ToLower().Contains("hot")) { return SharedObjects.WeatherTypes.Hot; }
            if (description.ToLower().Contains("smoke")) { return SharedObjects.WeatherTypes.Smoke; }
            if (description.ToLower().Contains("windy")) { return SharedObjects.WeatherTypes.Windy; }
            return SharedObjects.WeatherTypes.Clear;
        }

        static Tuple<double, double> GetLocationProperty()
        {
            if (SharedObjects.LatLong.HasRecord() || IntialgetLatLong())
            { return new Tuple<double, double>(SharedObjects.LatLong.Lat, SharedObjects.LatLong.Lng); }
            return new Tuple<double, double>(0, 0);
        }

        static bool IntialgetLatLong()
        {
            bool worked = false;
            if (MessageBox.Show(Properties.Gov3.LatLongLookupMessage, Properties.Gov3.LatLongLookupTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                double lat = Prompt("Latitude");
                double lon = Prompt("Longitude");
                SharedObjects.LatLong.Set(lat, lon);
                worked = true;
            }
            return worked;
        }

        private static double Prompt(string type) => double.Parse(SharedObjects.InputBox(string.Format(Properties.Gov3.LatLongInput, type), type));

        private Tuple<double, double> LookUpPoints(Tuple<double, double> LatLong)
        {
            string Lookup = String.Join(",", "GovWeather3Points", LatLong.Item1.ToString(), LatLong.Item2.ToString());
            string Item = SharedObjects.AppSettings.ReadSetting(Lookup);
            if (string.IsNullOrWhiteSpace(Item))
            {
                string ponstResponse = SharedObjects.CompressedCallSite($"https://api.weather.gov/points/{LatLong.Item1},{LatLong.Item2}", Gov_User);

                JObject item = JObject.Parse(ponstResponse);
                var X = item["properties"];
                Tuple<double, double> response = new Tuple<double, double>(X.Value<double>("gridX"), X.Value<double>("gridY"));
                SharedObjects.AppSettings.AddUpdateAppSettings(Lookup, String.Join(",", response.Item1.ToString(), response.Item2.ToString()));
                return response;
            }
            else
            {
                string[] SS = Item.Split(',');
                return new Tuple<double, double>(Double.Parse(SS[0]), double.Parse(SS[1]));
            }

        }
    }
}

