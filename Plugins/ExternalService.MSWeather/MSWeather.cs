/*
API I found online for Outlook weather (msn). 
 */
using System;
using System.Xml;
using System.Collections.Generic;
using System.Windows.Forms;
using WeatherDesktop.Share;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;

namespace ExternalService
{
    /// <summary>
    /// Description of MSWeather.
    /// </summary>
    [Export(typeof(WeatherDesktop.Interface.ISharedWeatherinterface))]
    [Export(typeof(WeatherDesktop.Interface.ILatLongInterface))]
    [ExportMetadata("ClassName", "MSWeather")]
    public class MSWeather : ISharedWeatherinterface, ILatLongInterface
    {
        #region Constants
        const string forcastFormat = "{3}, {2}. [{1}-{0}] Precipitation {4}%.";
        //const string ClassName = "MSWeather";
        #endregion

        #region Globals
        private string _zipcode;
        private int _cacheTimeout = 15;
        private DateTime _lastCall;
        private WeatherResponse _cacheValue;
        private KeyValuePair<double, double> _latLong;
        private Boolean HasBeenCalled = false;
        private int _skycode;
        private string _Status = "functional";
        private Exception _ThrownException = null;
        #endregion

        #region Settings

        public MenuItem[] SettingsItems()
        {
            List<MenuItem> returnValue = new List<MenuItem>();
            returnValue.Add(SharedObjects.ZipObjects.ZipMenuItem);
            return returnValue.ToArray();
        }

        #endregion

        #region New

        public MSWeather()
        {

        }
        public void Load()
        {
            int zip = 0;
            string zipcode = SharedObjects.ZipObjects.Rawzip;
            if (string.IsNullOrWhiteSpace(zipcode) || !int.TryParse(zipcode, out zip))
            { SharedObjects.ZipObjects.ChangeZipClick(new object(), new EventArgs()); }
            _zipcode = SharedObjects.ZipObjects.Rawzip;
            Invoke();
        }

        #endregion

        #region invoke
        public Exception ThrownException() { return _ThrownException; }

        public ISharedResponse Invoke()
        {
            if (!HasBeenCalled || DateTime.Now > _lastCall.AddMinutes(_cacheTimeout))
            {
                try
                {
                    string response = LiveCall(_zipcode, _cacheTimeout);
                    _cacheValue = TransformWeather(response);
                    _latLong = TransformerLatLong(response);
                    HasBeenCalled = true;
                    _lastCall = DateTime.Now;
                }
                catch (Exception ex)
                {
                    _ThrownException = ex;
                    _Status = ex.Message;
                    if (_cacheValue != null) { return _cacheValue; }
                    else
                    {
                        WeatherResponse DumbyResponse = new WeatherResponse();
                        DumbyResponse.ForcastDescription = ex.Message;
                        return DumbyResponse;
                    }
                }

            }
            return _cacheValue;
        }
        #endregion

        #region Live API call

        public  string LiveCall(string zipcode, int CacheTimeout)
        {
            if (SharedObjects.Cache.Exists(this.GetType().Name)) { return SharedObjects.Cache.StringValue(this.GetType().Name); }
            string webresponse = SharedObjects.CompressedCallSite(string.Format(Properties.Resources.MSWeather_Weather_Url, zipcode.ToString()));
            SharedObjects.Cache.Set(this.GetType().Name, webresponse, CacheTimeout);
            return webresponse;
        }

        private WeatherResponse TransformWeather(string webresponse)
        {
            WeatherResponse response = new WeatherResponse();
            System.Text.StringBuilder forcast = new System.Text.StringBuilder();

            XmlReader reader = XmlReader.Create(new System.IO.StringReader(webresponse));
            while (reader.Read())
            {
                if ((reader.NodeType == XmlNodeType.Element))
                    switch (reader.Name)
                    {
                         case "current":
                            response.Temp = int.Parse(reader.GetAttribute("temperature"));
                            _skycode = int.Parse(reader.GetAttribute("skycode"));
                            string skyTxt = reader.GetAttribute("skytext");
                            response.WType = ConvertType(skyTxt);
                            forcast.Append("Now ").Append(response.Temp).Append(" ").Append(skyTxt).Append(Environment.NewLine);
                            break;
                        case "forecast":
                            string low = reader.GetAttribute("low");
                            string high = reader.GetAttribute("high");
                            string skyText = reader.GetAttribute("skytextday");
                            DateTime forcastDay = DateTime.Parse(reader.GetAttribute("date"));
                            string day = reader.GetAttribute("day");
                            string percept = reader.GetAttribute("precip");
                            if (string.IsNullOrWhiteSpace(percept)) { percept = "0"; }
                            if (forcastDay >= DateTime.Today) { forcast.Append(string.Format(forcastFormat, low, high, skyText, day, percept)).Append(Environment.NewLine); }
                            break;
                        case "toolbar":
                            int Timeout = int.Parse(reader.GetAttribute("timewindow"));
                            if (Timeout > _cacheTimeout) _cacheTimeout = Timeout;
                            break;
                    }
            }
            response.ForcastDescription = forcast.ToString();
            return response;
        }

        private KeyValuePair<double, double> TransformerLatLong(string webresponse)
        {
            KeyValuePair<double, double> response = new KeyValuePair<double, double>();
            double lat, lng;
            XmlReader reader = XmlReader.Create(new System.IO.StringReader(webresponse));
            while (reader.Read())
            {
                if ((reader.NodeType == XmlNodeType.Element))
                    switch (reader.Name)
                    {
                        case "weather":
                            lat = double.Parse(reader.GetAttribute("lat")); lng = double.Parse(reader.GetAttribute("long"));
                            if (!SharedObjects.LatLong.HasRecord()) { SharedObjects.LatLong.Set(lat, lng); }
                            response = new KeyValuePair<double, double>(lat, lng); return response;
                    }
            }
            return response;
        }

        #endregion

        #region Helpers
        private static SharedObjects.WeatherTypes ConvertType(string SkyText)
        {
            if (SkyText.IndexOf("thunderstorm", StringComparison.OrdinalIgnoreCase) != -1) { return SharedObjects.WeatherTypes.ThunderStorm; }
            else if (SkyText.IndexOf("rain", StringComparison.OrdinalIgnoreCase) != -1) { return SharedObjects.WeatherTypes.Rain; }
            else if (SkyText.IndexOf("snow", StringComparison.OrdinalIgnoreCase) != -1) { return SharedObjects.WeatherTypes.Snow; }
            else if (SkyText.IndexOf("partly", StringComparison.OrdinalIgnoreCase) != -1) { return SharedObjects.WeatherTypes.PartlyCloudy; }
            else
            {
                switch (SkyText.ToLower())
                {
                    case "cloudy": return SharedObjects.WeatherTypes.Cloudy;
                    case "dust": return SharedObjects.WeatherTypes.Dust;
                    case "fog": return SharedObjects.WeatherTypes.Fog;
                    case "showers": return SharedObjects.WeatherTypes.Rain;
                    case "haze": return SharedObjects.WeatherTypes.Haze;
                    case "smoke": return SharedObjects.WeatherTypes.Smoke;
                    case "windy": return SharedObjects.WeatherTypes.Windy;
                    case "Frigid": return SharedObjects.WeatherTypes.Frigid;
                    case "Hot": return SharedObjects.WeatherTypes.Hot;
                    default: return SharedObjects.WeatherTypes.Clear;
                }
            }

        }

        //found to be not that reliable
        private static SharedObjects.WeatherTypes ConvertType(int skycode)
        {
            switch (skycode)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 17:
                case 35:
                case 37:
                case 38:
                case 47:
                    return SharedObjects.WeatherTypes.ThunderStorm;
                case 5:
                case 7:
                case 9:
                case 10:
                case 11:
                case 12:
                case 18:
                case 39:
                case 40:
                case 45:
                    return SharedObjects.WeatherTypes.Rain;
                case 6:
                case 8:
                case 13:
                case 14:
                case 15:
                case 16:
                case 41:
                case 42:
                case 43:
                case 46:
                    return SharedObjects.WeatherTypes.Snow;
                case 19:
                    return SharedObjects.WeatherTypes.Dust;
                case 20:
                    return SharedObjects.WeatherTypes.Fog;
                case 21:
                    return SharedObjects.WeatherTypes.Haze;
                case 22:
                    return SharedObjects.WeatherTypes.Smoke;
                case 23:
                case 24:
                    return SharedObjects.WeatherTypes.Windy;
                case 25:
                    return SharedObjects.WeatherTypes.Frigid;
                case 26:
                    return SharedObjects.WeatherTypes.Cloudy;
                case 27:
                case 28:
                case 29:
                case 30:
                case 33:
                case 34:
                    return SharedObjects.WeatherTypes.PartlyCloudy;
                case 36:
                    return SharedObjects.WeatherTypes.Hot;
                default:
                    return SharedObjects.WeatherTypes.Clear;
            }
        }

        #endregion

        #region Debug values
        public string Debug()
        {
            Dictionary<string, string> DebugValues = new Dictionary<string, string>();
            DebugValues.Add("Skykey", _skycode.ToString());
            DebugValues.Add("Last Updated", _lastCall.ToString());
            DebugValues.Add("Cache timeout", _cacheTimeout.ToString());
            DebugValues.Add("Latitude", _latLong.Key.ToString());
            DebugValues.Add("Longitude", _latLong.Value.ToString());
            return SharedObjects.CompileDebug(DebugValues);
        }

        public double Latitude() { return _latLong.Key; }

        public double Longitude() { return _latLong.Value; }


        public bool worked()
        {
            return (_latLong.Key != 0 && _latLong.Value != 0);
        }

        #endregion
    }
}
/*
 Trusting the values from Donavon Yelton on https://stackoverflow.com/questions/12142094/msn-weather-api-list-of-conditions
  
 0, 1 ,2, 3 ,4, 17, 35 - Thunderstorm
5 - Rain/Snow mix
6 - Sleet/Snow mix
7 - Rain/Snow/Sleet mix
8,9 - Icy   // 9  = light rain
10 - Rain/Sleet mix
11 - Light Rain
12 - Rain
13 - Light Snow
14,16,42,43 - Snow
15 - Blizzard
18,40 - Showers
19 - Dust
20 - Fog
21 - Haze
22 - Smoke
23,24 - Windy
25 - Frigid
26 - Cloudy
27,29,33 - Partly Cloudy (night)
28,30,34 - Partly Cloudy
31 - Clear (night)
32 - Clear
36 - Hot
37,38 - Scattered Thunderstorms
39 - Scattered Showers
41 - Scattered Snow Showers
44 - N/A
45 - Scattered Rain Showers (night)
46 - Scattered Snow Showers (night)
47 - Scattered Thunderstorms (night)
 
 
 output of file
 <weatherdata xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
<weather weatherlocationcode="wc:USWI0441" weatherlocationname="Menomonee Falls, Village of, WI" zipcode="53051" url="http://a.msn.com/54/en-US/ct43.184,-88.123?ctsrc=outlook" imagerelativeurl="http://blob.weather.microsoft.com/static/weather4/en-us/" degreetype="F" provider="Foreca" attribution="http://www.foreca.com/" attribution2="Foreca" lat="43.184" long="-88.123" timezone="-5" alert="" entityid="20984" encodedlocationname="53051%2C+WI">
<current temperature="57" skycode="11" skytext="Rain" date="2017-10-13" observationtime="21:00:00" observationpoint="53051, WI" feelslike="57" humidity="96" winddisplay="0 mph Northeast" day="Friday" shortday="Fri" windspeed="0 mph"/>
<forecast low="49" high="58" skycodeday="27" skytextday="Cloudy" date="2017-10-12" day="Thursday" shortday="Thu" precip=""/>
<forecast low="55" high="63" skycodeday="26" skytextday="Cloudy" date="2017-10-13" day="Friday" shortday="Fri" precip="90"/>
<forecast low="49" high="64" skycodeday="11" skytextday="Rain" date="2017-10-14" day="Saturday" shortday="Sat" precip="100"/>
<forecast low="39" high="63" skycodeday="30" skytextday="Partly Sunny" date="2017-10-15" day="Sunday" shortday="Sun" precip="100"/>
<forecast low="47" high="60" skycodeday="30" skytextday="Partly Sunny" date="2017-10-16" day="Monday" shortday="Mon" precip="0"/>
<toolbar timewindow="60" minversion="1.0.1965.0"/>
</weather>
</weatherdata>
 
 */