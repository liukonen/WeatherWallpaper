using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.Xml;
using WeatherDesktop.Interface;

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
            _zip = Shared.tryGetZip();
        }

        public string Debug()
        {
            Dictionary<string, string> debugValues = new Dictionary<string, string>();
            debugValues.Add("Last updated", LastUpdated.ToString());
            debugValues.Add("Icon url", iconUrl);
            debugValues.Add("zip", _zip);
            return Shared.CompileDebug("Weather by NOAA", debugValues);
        }

        public ISharedResponse Invoke()
        {
            if (Shared.Cache.Exists(this.GetType().Name)) { return (WeatherResponse)Shared.Cache.Value(this.GetType().Name); }
            WeatherResponse response = new WeatherResponse();
            try {
                httpResponse = Shared.CompressedCallSite(string.Format(Properties.Resources.Gov_Weather_Url, _zip), Properties.Resources.Gov_User);
                response = Transform(httpResponse);
                Shared.Cache.Set(this.GetType().Name, response, UpdateInterval);
                LastUpdated = DateTime.Now;
                _ThrownException = null;
            }
            catch(Exception x) { _ThrownException = x; _errors = x.ToString(); }
            return response;
        }

        public MenuItem[] SettingsItems(){ return new MenuItem[] { Shared.ZipMenuItem };}


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
                            if (!Shared.LatLong.HasRecord())
                            {
                                double lat, lng;
                                if (double.TryParse(reader.GetAttribute("latitude"), out lat) && double.TryParse(reader.GetAttribute("longitude"), out lng))
                                { Shared.LatLong.set(lat, lng); }
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
        static Shared.WeatherTypes extractWeatherType(string currentType, string Urlbackup)
        {
            switch (currentType)
            { case "thunderstorms":
              case  "water spouts":
                    return Shared.WeatherTypes.ThunderStorm;
                case "snow shower":
                case "blowing snow":
                case "frost":
                case "snow":
                    return Shared.WeatherTypes.Snow;
                case "freezing spray":
                case "ice crystals":
                case "ice pellets":
                case "freezing fog":
                case "ice fog":
                    return Shared.WeatherTypes.Frigid;
                case "freezing drizzle":
                case "freezing rain":
                case "drizzle":
                case "rain":
                case "rain shower":
                case "hail":
                    return Shared.WeatherTypes.Rain;
                case "fog":
                    return Shared.WeatherTypes.Fog;
                case "haze":
                    return Shared.WeatherTypes.Haze;
                case "smoke":
                case "volcanic ash":
                    return Shared.WeatherTypes.Smoke;
                case "blowing dust":
                case "blowing sand":
                    return Shared.WeatherTypes.Dust;
            }
            return ExtractTypeFromIcon(Urlbackup);
        }

        /// <summary>
        /// takes the image that would normally be used for an app, and extracts its "weather type"
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static Shared.WeatherTypes ExtractTypeFromIcon(string url)
        {
            string core = url.Substring(url.LastIndexOf("/")).Replace(".jpg", string.Empty);
            if (core.StartsWith("ntsra") || core.StartsWith("tsra")) return Shared.WeatherTypes.ThunderStorm;  //night Thunderstorm
            if (core.StartsWith("nscttsra") || core.StartsWith("scttsra")) return Shared.WeatherTypes.ThunderStorm; //night sky thunderstorm
            if (core.StartsWith("ip")) return Shared.WeatherTypes.Snow;  //Ice Particals
            if (core.StartsWith("nraip") || core.StartsWith("raip")) return Shared.WeatherTypes.Rain; // night rain  / Ice particals
            if (core.StartsWith("mix")) return Shared.WeatherTypes.Snow;
            if (core.StartsWith("nrasn")) return Shared.WeatherTypes.Rain;//snow rain
            if (core.StartsWith("nsn") || core.StartsWith("sn")) return Shared.WeatherTypes.Snow;
            if (core.StartsWith("fzra")) return Shared.WeatherTypes.Rain; //freezing rain
            if (core.StartsWith("nra") || core.StartsWith("ra")) return Shared.WeatherTypes.Rain; // night rain.
            if (core.StartsWith("hi_nshwrs") || core.StartsWith("shra") || core.StartsWith("hi_shwrs")) return Shared.WeatherTypes.Rain; //showers
            if (core == "blizzard") return Shared.WeatherTypes.Snow;
            if (core == "du") return Shared.WeatherTypes.Dust;
            if (core == "fu") return Shared.WeatherTypes.Smoke; //patchy or smoke
            switch (core)
            {
                case "nfg":
                case "fg":
                    return Shared.WeatherTypes.Fog;
                case "nwind":
                case "wind":
                    return Shared.WeatherTypes.Windy;
                case "novc":
                case "ovc":
                case "nbkn":
                case "bkn":
                    return Shared.WeatherTypes.Cloudy;
                case "nsct":
                case "sct":
                case "nfew":
                case "few":
                    return Shared.WeatherTypes.PartlyCloudy;
                case "nskc":
                case "skc":
                    return Shared.WeatherTypes.Clear;
                case "cold":
                    return Shared.WeatherTypes.Frigid;
                case "hot":
                    return Shared.WeatherTypes.Hot;

            }
            return Shared.WeatherTypes.Clear;
        }

            #endregion
        }
}
