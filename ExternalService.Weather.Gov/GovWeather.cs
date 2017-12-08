using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.Xml;
using WeatherDesktop.Interface;
using WeatherDesktop.Shared;

namespace InternalService
{
    [Export(typeof(WeatherDesktop.Interface.ISharedWeatherinterface))]
    [ExportMetadata("ClassName", "GovWeather")]
    class GovWeather : ISharedWeatherinterface
    {
        const int UpdateInterval = 60;
        private string httpResponse;
        private string iconUrl;
        private string _errors;
        private DateTime LastUpdated = DateTime.MinValue;
        private string _zip;
        private Exception _ThrownException = null;
        
        public Exception ThrownException() { return _ThrownException; }

        public GovWeather()
        {
        }

        public void Load()
        {
            _zip = SharedObjects.ZipObjects.tryGetZip();
        }

        public string Debug()
        {
            Dictionary<string, string> debugValues = new Dictionary<string, string>();
            debugValues.Add("Last updated", LastUpdated.ToString());
            debugValues.Add("Icon url", iconUrl);
            debugValues.Add("zip", _zip);
            return SharedObjects.CompileDebug(debugValues);
        }

        public ISharedResponse Invoke()
        {
            if (SharedObjects.Cache.Exists(this.GetType().Name)) { return (WeatherResponse)SharedObjects.Cache.Value(this.GetType().Name); }
            WeatherResponse response = new WeatherResponse();
            try {
                httpResponse = SharedObjects.CompressedCallSite(string.Format(Properties.Resources.Gov_Weather_Url, _zip), Properties.Resources.Gov_User);
                response = Transform(httpResponse);
                SharedObjects.Cache.Set(this.GetType().Name, response, UpdateInterval);
                LastUpdated = DateTime.Now;
                _ThrownException = null;
            }
            catch(Exception x) { _ThrownException = x; _errors = x.ToString(); }
            return response;
        }

        public MenuItem[] SettingsItems(){ return new MenuItem[] { SharedObjects.ZipObjects.ZipMenuItem };}


        #region "Helpers"

        /// <summary>
        /// Transforms the response string to a weatherResponse object.
        /// </summary>
        /// <param name="Response"></param>
        /// <returns></returns>
        WeatherResponse Transform(string Response)
        {
            int Max = -180;
            int Min = 180;

            WeatherResponse value = new WeatherResponse();
            XmlReader reader = XmlReader.Create(new System.IO.StringReader(Response));
            string ForcastType = string.Empty;
            string type = string.Empty;
            while (reader.Read())
            {
                if ((reader.NodeType == XmlNodeType.Element))
                    switch (reader.Name)
                    {
                        case "point":
                            if (!SharedObjects.LatLong.HasRecord())
                            {
                                double lat, lng;
                                if (double.TryParse(reader.GetAttribute("latitude"), out lat) && double.TryParse(reader.GetAttribute("longitude"), out lng))
                                { SharedObjects.LatLong.set(lat, lng); }
                            }
                            break;
                        case "temperature":
                            type = reader.GetAttribute("type");
                            break;
                        case "value":
                            if (type == "maximum")
                            {
                                int test;
                                test = int.Parse(reader.ReadInnerXml());
                                if (Max == -180) { value.Temp = test; }
                                Max = (test > Max) ? test : Max;
                            }
                            else if (type == "minimum")
                            {
                                int test;
                                test = int.Parse(reader.ReadInnerXml());

                                Min = (test > Min) ? Min : test;
                            }
                            else if (type == "Weather")
                            {
                                if (string.IsNullOrEmpty(ForcastType))
                                {
                                    string coverage = reader.GetAttribute("coverage");
                                    string intensity = reader.GetAttribute("intensity");
                                    string additive = reader.GetAttribute("additive");
                                    ForcastType = reader.GetAttribute("weather-type");
                                    value.ForcastDescription = string.Concat(coverage, " ", additive, (string.IsNullOrWhiteSpace(additive) ? " " : ""), ForcastType, ((intensity == "none")? string.Empty : " (" + intensity + ")"));
                                }
                            }

                            //type = string.Empty;
                            break;
                        case "weather-conditions":
                            type = "Weather";
                            break;
                        case "icon-link":
                            if (string.IsNullOrWhiteSpace(iconUrl)) { iconUrl = reader.ReadInnerXml(); }
                            break;

                    }
            }
            value.ForcastDescription += string.Concat("(", Max.ToString(), "-", Min.ToString(), ")");
            value.WType = extractWeatherType(ForcastType, iconUrl);
            return value;
        }

        /// <summary>
        /// Extracts the weather type from the current type, or if it can't find it, from the weather icon url.
        /// </summary>
        /// <param name="currentType"></param>
        /// <param name="Urlbackup"></param>
        /// <returns></returns>
        static SharedObjects.WeatherTypes extractWeatherType(string currentType, string Urlbackup)
        {
            switch (currentType)
            { case "thunderstorms":
              case  "water spouts":
                    return SharedObjects.WeatherTypes.ThunderStorm;
                case "snow shower":
                case "blowing snow":
                case "frost":
                case "snow":
                    return SharedObjects.WeatherTypes.Snow;
                case "freezing spray":
                case "ice crystals":
                case "ice pellets":
                case "freezing fog":
                case "ice fog":
                    return SharedObjects.WeatherTypes.Frigid;
                case "freezing drizzle":
                case "freezing rain":
                case "drizzle":
                case "rain":
                case "rain shower":
                case "hail":
                    return SharedObjects.WeatherTypes.Rain;
                case "fog":
                    return SharedObjects.WeatherTypes.Fog;
                case "haze":
                    return SharedObjects.WeatherTypes.Haze;
                case "smoke":
                case "volcanic ash":
                    return SharedObjects.WeatherTypes.Smoke;
                case "blowing dust":
                case "blowing sand":
                    return SharedObjects.WeatherTypes.Dust;
            }
            return ExtractTypeFromIcon(Urlbackup);
        }

        /// <summary>
        /// takes the image that would normally be used for an app, and extracts its "weather type"
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static SharedObjects.WeatherTypes ExtractTypeFromIcon(string url)
        {
            string core = url.Substring(url.LastIndexOf("/")).Replace(".jpg", string.Empty);
            if (core.StartsWith("ntsra") || core.StartsWith("tsra")) return SharedObjects.WeatherTypes.ThunderStorm;  //night Thunderstorm
            if (core.StartsWith("nscttsra") || core.StartsWith("scttsra")) return SharedObjects.WeatherTypes.ThunderStorm; //night sky thunderstorm
            if (core.StartsWith("ip")) return SharedObjects.WeatherTypes.Snow;  //Ice Particals
            if (core.StartsWith("nraip") || core.StartsWith("raip")) return SharedObjects.WeatherTypes.Rain; // night rain  / Ice particals
            if (core.StartsWith("mix")) return SharedObjects.WeatherTypes.Snow;
            if (core.StartsWith("nrasn")) return SharedObjects.WeatherTypes.Rain;//snow rain
            if (core.StartsWith("nsn") || core.StartsWith("sn")) return SharedObjects.WeatherTypes.Snow;
            if (core.StartsWith("fzra")) return SharedObjects.WeatherTypes.Rain; //freezing rain
            if (core.StartsWith("nra") || core.StartsWith("ra")) return SharedObjects.WeatherTypes.Rain; // night rain.
            if (core.StartsWith("hi_nshwrs") || core.StartsWith("shra") || core.StartsWith("hi_shwrs")) return SharedObjects.WeatherTypes.Rain; //showers
            if (core == "blizzard") return SharedObjects.WeatherTypes.Snow;
            if (core == "du") return SharedObjects.WeatherTypes.Dust;
            if (core == "fu") return SharedObjects.WeatherTypes.Smoke; //patchy or smoke
            switch (core)
            {
                case "nfg":
                case "fg":
                    return SharedObjects.WeatherTypes.Fog;
                case "nwind":
                case "wind":
                    return SharedObjects.WeatherTypes.Windy;
                case "novc":
                case "ovc":
                case "nbkn":
                case "bkn":
                    return SharedObjects.WeatherTypes.Cloudy;
                case "nsct":
                case "sct":
                case "nfew":
                case "few":
                    return SharedObjects.WeatherTypes.PartlyCloudy;
                case "nskc":
                case "skc":
                    return SharedObjects.WeatherTypes.Clear;
                case "cold":
                    return SharedObjects.WeatherTypes.Frigid;
                case "hot":
                    return SharedObjects.WeatherTypes.Hot;

            }
            return SharedObjects.WeatherTypes.Clear;
        }

            #endregion
        }
}
