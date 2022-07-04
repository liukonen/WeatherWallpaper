using System;
using System.Xml;
using WeatherDesktop.Share;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;
using System.Collections.Generic;
using WeatherDesktop.Shared.Handlers;

namespace WeatherDesktop.Services.External
{
    [Export(typeof(ISharedWeatherinterface))]
    [ExportMetadata("ClassName", "GovWeather2")]
    class GovWeather : ISharedWeatherinterface
    {
        const int UpdateInterval = 60;
        private string httpResponse;
        private string iconUrl;
        //private string _errors;
        private DateTime LastUpdated = DateTime.MinValue;
        private string _zip;
        private Exception _ThrownException = null;


        public Exception ThrownException() { return _ThrownException; }

        public GovWeather()
        {
        }

        public void Load()
        {
            _zip = SharedObjects.ZipObjects.TryGetZip();
        }

        public string Debug()
        {
            var debugValues = new Dictionary<string, string>
            {
                { "Last updated", LastUpdated.ToString() },
                { "Icon url", iconUrl },
                { "zip", _zip }
            };
            return SharedObjects.CompileDebug(debugValues);
        }

        public ISharedResponse Invoke()
        {
            if (SharedObjects.Cache.Exists(this.GetType().Name)) { return MemCacheHandler.Instance.GetItem<WeatherResponse>(this.GetType().Name); }
            var response = new WeatherResponse();
            try
            {
                httpResponse = SharedObjects.CompressedCallSite(string.Format(Properties.Gov2.Gov_Weather_Url, _zip), Properties.Gov2.Gov_User);
                response = Transform(httpResponse);
                MemCacheHandler.Instance.SetItem(this.GetType().Name, response, UpdateInterval);
                LastUpdated = DateTime.Now;
                _ThrownException = null;
            }
            catch (Exception x) { _ThrownException = x; } //_errors = x.ToString(); }
            return response;
        }

        public MenuItem[] SettingsItems() { return new MenuItem[] { SharedObjects.ZipObjects.ZipMenuItem }; }


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

            var value = new WeatherResponse();
            var reader = XmlReader.Create(new System.IO.StringReader(Response));
            var ForcastType = string.Empty;
            var type = string.Empty;
            while (reader.Read())
            {
                if ((reader.NodeType == XmlNodeType.Element))
                    switch (reader.Name)
                    {
                        case "point":
                            if (!SharedObjects.LatLong.HasRecord())
                            {
                                if (double.TryParse(reader.GetAttribute("latitude"), out double lat)
                                    && double.TryParse(reader.GetAttribute("longitude"), out double lng))
                                { SharedObjects.LatLong.Set(lat, lng); }
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

                                    value.ForcastDescription = string.Concat(
                                        coverage,
                                        " ",
                                        additive,
                                        (string.IsNullOrWhiteSpace(additive) ? " " : ""),
                                        ForcastType,
                                        ((intensity == "none") ? string.Empty : " (" + intensity + ")")
                                        );
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
            value.WType = ExtractWeatherType(ForcastType, iconUrl);
            return value;
        }



        static SharedObjects.WeatherTypes ExtractWeatherType(string currentType, string Urlbackup)
        {

            if (itemlookup.ContainsKey(currentType)) return itemlookup[currentType];
            //return ExtractTypeFromIcon(Urlbackup);
            var core = Urlbackup.Substring(Urlbackup.LastIndexOf("/")).Replace(".jpg", string.Empty);
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

            if (IconLookup.ContainsKey(core)) return IconLookup[core];
            return SharedObjects.WeatherTypes.Clear;
        }
        #endregion


        #region itemTables
        private readonly static Dictionary<string, SharedObjects.WeatherTypes> IconLookup = new Dictionary<string, SharedObjects.WeatherTypes>()
        {
            { "nfg", SharedObjects.WeatherTypes.Fog },
            { "fg", SharedObjects.WeatherTypes.Fog },
            { "nwind", SharedObjects.WeatherTypes.Windy },
            { "wind", SharedObjects.WeatherTypes.Windy },
            { "novc", SharedObjects.WeatherTypes.Cloudy },
            { "ovc", SharedObjects.WeatherTypes.Cloudy },
            { "nbkn", SharedObjects.WeatherTypes.Cloudy },
            { "bkn", SharedObjects.WeatherTypes.Cloudy },
            { "nsct", SharedObjects.WeatherTypes.PartlyCloudy },
            { "sct", SharedObjects.WeatherTypes.PartlyCloudy },
            { "nfew", SharedObjects.WeatherTypes.PartlyCloudy },
            { "few", SharedObjects.WeatherTypes.PartlyCloudy },
            { "nskc", SharedObjects.WeatherTypes.Clear },
            { "skc", SharedObjects.WeatherTypes.Clear },
            { "cold", SharedObjects.WeatherTypes.Frigid },
            { "hot", SharedObjects.WeatherTypes.Hot },
        };

        private readonly static Dictionary<string, SharedObjects.WeatherTypes> itemlookup = new Dictionary<string, SharedObjects.WeatherTypes>()
        {

            { "thunderstorms", SharedObjects.WeatherTypes.ThunderStorm },
            { "water spouts", SharedObjects.WeatherTypes.ThunderStorm },
            { "snow shower" , SharedObjects.WeatherTypes.Snow },
            { "blowing snow" , SharedObjects.WeatherTypes.Snow },
            { "frost" , SharedObjects.WeatherTypes.Snow },
            { "snow" , SharedObjects.WeatherTypes.Snow },
            { "freezing spray" , SharedObjects.WeatherTypes.Frigid },
            { "ice crystals" , SharedObjects.WeatherTypes.Frigid },
            { "ice pellets" , SharedObjects.WeatherTypes.Frigid },
            { "freezing fog" , SharedObjects.WeatherTypes.Frigid },
            { "ice fog" , SharedObjects.WeatherTypes.Frigid },
            { "freezing drizzle" , SharedObjects.WeatherTypes.Rain },
            { "freezing rain" , SharedObjects.WeatherTypes.Rain },
            { "drizzle" , SharedObjects.WeatherTypes.Rain },
            { "rain" , SharedObjects.WeatherTypes.Rain },
            { "rain shower" , SharedObjects.WeatherTypes.Rain },
            { "hail" , SharedObjects.WeatherTypes.Rain },
            { "fog" , SharedObjects.WeatherTypes.Fog },
            { "haze" , SharedObjects.WeatherTypes.Haze },
            { "smoke" , SharedObjects.WeatherTypes.Smoke },
            { "volcanic ash" , SharedObjects.WeatherTypes.Smoke },
            { "blowing dust" , SharedObjects.WeatherTypes.Dust },
            { "blowing sand" , SharedObjects.WeatherTypes.Dust }
        };
        #endregion
    }
}
