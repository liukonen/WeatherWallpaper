using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace WeatherDesktop.Interface 
{
    partial class wundergroundAPIBase : ISharedInterface
    {
        const string c_ClassName = "wunderground";
        const string c_apiKeyName = " - apikey";
        Dictionary<SharedType, ISharedResponse> Cache = new Dictionary<SharedType, ISharedResponse>();
        Interface.WeatherResponse Weather = new WeatherResponse();
        Interface.LatLongResponse LatLong = new LatLongResponse();
        Interface.SunRiseSetResponse SRS = new SunRiseSetResponse();
        string _zip;
        string _apikey;
        string _url = string.Empty;
        string _errors = string.Empty;
        public enum SharedType { Weather = 0, LatLong = 1, SRS = 2}
        private DateTime _lastudated = DateTime.MinValue;

        public string Debug()
        {
            return _url + _errors;
        }

        public wundergroundAPIBase()
        {
            _apikey = Shared.ReadSettingEncrypted(c_ClassName + c_apiKeyName);
            if (string.IsNullOrWhiteSpace(_apikey))
            {
                EnterAPIKey();
            }
            _zip = Shared.tryGetZip();
        }

        /// <summary>
        /// This version will always return a dumby... use seperate functions to return values
        /// </summary>
        /// <returns></returns>
        public virtual ISharedResponse Invoke()
        {
            return new WeatherResponse();
        }

        public void Call()
        {
            bool exists = Shared.Cache.Exists(c_ClassName);
            if (Cache.Count == 0 && exists) { Cache = (Dictionary<SharedType, ISharedResponse>)Shared.Cache.Value(c_ClassName); }
            if (!exists)
            {
                try
                {
                    string response = Shared.CompressedCallSite(string.Format(Properties.Resources.wunderground_Url, _zip, _apikey));
                    transformXML(response);
                    Cache = new Dictionary<SharedType, ISharedResponse>();
                    Cache.Add(SharedType.Weather, Weather);
                    Cache.Add(SharedType.LatLong, LatLong);
                    Cache.Add(SharedType.SRS, SRS);
                    _lastudated = DateTime.Now;
                    Shared.Cache.Set(c_ClassName, Cache, 60);
                }
                catch (Exception x)
                {
                    WeatherDesktop.Shared.ErrorHandler.Send(x);
                    _errors = x.ToString();
                }

            }
            
        }

        public ISharedResponse ReturnValue(SharedType Type)
        {
            return Cache[Type];
        }

        public MenuItem[] SettingsItems()
        {
            return new MenuItem[] { Shared.ZipMenuItem, new MenuItem("Change API Key", ChangeAPI) }; 
        }

        private void ChangeAPI(object sender, EventArgs e)
        {
            EnterAPIKey();
        }

    private void EnterAPIKey()
    {
        _apikey = Microsoft.VisualBasic.Interaction.InputBox("Please enter your wunderground api key.");
        if (!string.IsNullOrWhiteSpace(_apikey))Shared.AddupdateAppSettingsEncrypted(c_ClassName + c_apiKeyName, _apikey);
    }


        private void transformXML(string xml)
        {
            int SunsetHour =0, SunriseHour =0, SunsetMinute =0, SunRiseMinute =0;
            int activeType = 0;

            string forcast = string.Empty ;
            string currentTemp = string.Empty;
            WeatherResponse value = new WeatherResponse();
            XmlReader reader = XmlReader.Create(new System.IO.StringReader(xml));
            while (reader.Read())
            {
                if ((reader.NodeType == XmlNodeType.Element))
                    switch (reader.Name)
                    {
                        case "latitude":
                            LatLong.Latitude = double.Parse(reader.ReadInnerXml());
                            break;
                        case "longitude":
                            LatLong.Longitude = double.Parse(reader.ReadInnerXml());
                            break;
                        case "weather":
                            forcast = reader.ReadInnerXml();
                            break;
                        case "temperature_string":
                             currentTemp = reader.ReadInnerXml();
                            break;
                        case "temp_f":
                            Weather.Temp = Convert.ToInt32(double.Parse(reader.ReadInnerXml()));
                            break;
                        case "icon_url":
                            _url = reader.ReadInnerXml();
                           Weather.WType = ConvertImageToType(_url);
                            break;
                        case "sunset":
                            activeType = 1;
                            break;
                        case "sunrise":
                            activeType = 2;
                            break;
                        case "hour":
                            int TestHour = 0;
                            if (!int.TryParse(reader.ReadInnerXml(), out TestHour)) { TestHour = 0; } ;
                            if (activeType > 0)
                            {
                                if (activeType == 1 && SunsetHour == 0) { SunsetHour = (TestHour > 0) ? TestHour : 1; }
                                else if (SunriseHour == 0) { SunriseHour = (TestHour > 0) ? TestHour : 1; }
                            }
                            break;
                        case "minute":
                            int TestMin = 0; int.TryParse(reader.ReadInnerXml(), out TestMin);
                            if (activeType > 0)
                            {
                                if (activeType == 1 && SunsetMinute == 0) { SunsetMinute = (TestMin > 0) ? TestMin : 1; }
                                else if (SunRiseMinute == 0) { SunRiseMinute = (TestMin > 0) ? TestMin : 1; }

                            }
                            break;                           
                    }
            }
            Weather.ForcastDescription = forcast + " " + currentTemp;
            SRS.Status = "ok";
            SRS.SunRise = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, SunriseHour, SunRiseMinute, 0);
            SRS.SunSet = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, SunsetHour, SunsetMinute, 0);
        }

        private static Shared.WeatherTypes ConvertImageToType(string url)
        {
            string parsed = url.Substring(url.LastIndexOf("/") +1).Replace(".gif", string.Empty).Replace("nt_", string.Empty);
            switch (parsed)
            {
                case "chanceflurries":
                case "chancesleet":
                case "chancesnow":
                case "sleet":
                case "snow":
                case "flurries":
                    return Shared.WeatherTypes.Snow;
                case "chancerain":
                case "rain":
                    return Shared.WeatherTypes.Rain;
                case "chancetstorms":
                case "tstorm":
                    return Shared.WeatherTypes.ThunderStorm;
                case "cloudy":
                case "mostlycloudy":
                    return Shared.WeatherTypes.Cloudy;
                case "partlycloudy":
                case "partlysunny":
                    return Shared.WeatherTypes.PartlyCloudy;
                case "fog":
                    return Shared.WeatherTypes.Fog;
                case "hazy":
                    return Shared.WeatherTypes.Haze;
                case "clear":
                case "mostlysunny":
                    return Shared.WeatherTypes.Clear;
                default:
                    return Shared.WeatherTypes.Windy;
            }

        }
    }
}
