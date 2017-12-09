﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using WeatherDesktop.Interface;
using WeatherDesktop.Share;

namespace InternalService
{
    [Export(typeof(WeatherDesktop.Interface.ISharedWeatherinterface))]
    [ExportMetadata("ClassName", "YahooWeather")]
    public class YahooWeather : ISharedWeatherinterface
    {
        const int _HardWiredUpdateInterval = 30;

        private int _UpdateInterval = 0;
        private WeatherResponse _cache;
        private string _status;
        private DateTime _lastCall;
        string _zip;
        bool HasBeenCalled = false;
        private Exception _ThrownException = null;

        #region IWeather Objects
        public Exception ThrownException() { return _ThrownException; }

        public string Debug()
        {
            Dictionary<string, string> DebugValues = new Dictionary<string, string>();
            DebugValues.Add("status", _status);
            return SharedObjects.CompileDebug(DebugValues);
        }

        int UpdateInterval
        {
            get
            {
                return (_UpdateInterval == 0 && !int.TryParse(SharedObjects.AppSettings.ReadSetting(this.GetType().Name + ".UpdateInterval"), out _UpdateInterval)) ? _HardWiredUpdateInterval : _UpdateInterval;
            }
            set
            {
                SharedObjects.AppSettings.AddUpdateAppSettings(this.GetType().Name + ".UpdateInterval", value.ToString());
            }
        }

        public ISharedResponse Invoke()
        {
            if (!HasBeenCalled || DateTime.Now > _lastCall.AddMinutes(UpdateInterval))
            {
                _cache = new WeatherResponse();
                YahooWeatherObject result = new YahooWeatherObject();
                try
                {
                    result = LiveCall();
                    HasBeenCalled = true;
                    _lastCall = DateTime.Now;
                    _cache.Temp = int.Parse(result.query.results.channel.item.condition.temp);
                    _cache.ForcastDescription = RemoveMarkup(result.query.results.channel.item.description);
                    _cache.WType = GetWeatherType(int.Parse(result.query.results.channel.item.condition.code));
                }
                catch (Exception ex) { _status = ex.Message; _ThrownException = ex; }
            }
            return _cache;
        }

        YahooWeatherObject LiveCall()
        {
            string results = string.Empty;
            string URL = string.Empty;
            YahooWeatherObject Response = new YahooWeatherObject();
            try
            {
                if (SharedObjects.Cache.Exists(this.GetType().Name)) return (YahooWeatherObject)SharedObjects.Cache.Value(this.GetType().Name);
                URL = string.Format(Properties.Resources.Yahoo_Weather_Url, _zip);
                results = SharedObjects.CompressedCallSite(URL);
                JavaScriptSerializer jsSerialization = new JavaScriptSerializer();
                Response = jsSerialization.Deserialize<YahooWeatherObject>(results);
                SharedObjects.Cache.Set(this.GetType().Name, Response, _HardWiredUpdateInterval);

            }
            catch (Exception x)
            {
                Exception eResults = new Exception(results, x);
                Exception Mask = new Exception("Error calling yahoo..." + URL, eResults);
                _ThrownException = x;
            }
            return Response;
        }

        private static string RemoveMarkup(string MarkedupHTML)
        {
            return System.Text.RegularExpressions.Regex.Replace(MarkedupHTML, "<.*?>", String.Empty);
        }

        public MenuItem[] SettingsItems()
        {
            List<MenuItem> Items = new List<MenuItem>();
            Items.Add(SharedObjects.ZipObjects.ZipMenuItem);
            return Items.ToArray();

        }
        #endregion

        public YahooWeather()
        {

        }
        public void Load()
        {
            if (UpdateInterval > 0 && _UpdateInterval == 0) { EnterInterval(); }
            if (string.IsNullOrWhiteSpace(_zip)) { _zip = SharedObjects.ZipObjects.TryGetZip(); }
            Invoke();
        }

        SharedObjects.WeatherTypes GetWeatherType(int code)
        {
            switch (code)
            {
                case 31:
                case 32:
                case 33:
                case 34:
                    return SharedObjects.WeatherTypes.Clear;
                case 26:
                case 27:
                case 28:
                    return SharedObjects.WeatherTypes.Cloudy;
                case 19:
                    return SharedObjects.WeatherTypes.Dust;
                case 20:
                    return SharedObjects.WeatherTypes.Fog;
                case 25:
                    return SharedObjects.WeatherTypes.Frigid;
                case 21:
                    return SharedObjects.WeatherTypes.Haze;
                case 36:
                    return SharedObjects.WeatherTypes.Hot;
                case 29:
                case 30:
                case 44:
                    return SharedObjects.WeatherTypes.PartlyCloudy;
                case 5:
                case 6:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 17:
                case 18:
                case 35:
                case 40:
                    return SharedObjects.WeatherTypes.Rain;
                case 22:
                    return SharedObjects.WeatherTypes.Smoke;
                case 7:
                case 13:
                case 14:
                case 15:
                case 16:
                case 41:
                case 42:
                case 43:
                case 46:
                    return SharedObjects.WeatherTypes.Snow;
                case 23:
                case 24:
                    return SharedObjects.WeatherTypes.Windy;
                case 3:
                case 4:
                case 37:
                case 38:
                case 39:
                case 45:
                case 47:
                default:
                    return SharedObjects.WeatherTypes.ThunderStorm;
            }

        }


        public void EnterInterval()
        {
            UpdateInterval = int.Parse(WeatherDesktop.Share.SharedObjects.InputBox("Please enter a update time value in minutes, between 10 and 120", "Yahoo Update Interval", _HardWiredUpdateInterval.ToString()));

        }

        #region Autogenerated code

        [DataContract]
        public class Condition
        {

            [DataMember(Name = "code")]
            public string code { get; set; }

            [DataMember(Name = "date")]
            public string date { get; set; }

            [DataMember(Name = "temp")]
            public string temp { get; set; }

            [DataMember(Name = "text")]
            public string text { get; set; }
        }

        [DataContract]
        public class Item
        {

            [DataMember(Name = "condition")]
            public Condition condition { get; set; }

            [DataMember(Name = "description")]
            public string description { get; set; }
        }

        [DataContract]
        public class Channel
        {

            [DataMember(Name = "item")]
            public Item item { get; set; }
        }

        [DataContract]
        public class Results
        {

            [DataMember(Name = "channel")]
            public Channel channel { get; set; }
        }

        [DataContract]
        public class Query
        {

            [DataMember(Name = "count")]
            public int count { get; set; }

            [DataMember(Name = "created")]
            public DateTime created { get; set; }

            [DataMember(Name = "lang")]
            public string lang { get; set; }

            [DataMember(Name = "results")]
            public Results results { get; set; }
        }

        [DataContract]
        public class YahooWeatherObject
        {

            [DataMember(Name = "query")]
            public Query query { get; set; }
        }
        #endregion
    }
}


/* codes

 Extreme (just Thunderstorm 0,1,2,3200	not available

    0	tornado
1	tropical storm
2	hurricane
3	severe thunderstorms
4	thunderstorms
5	mixed rain and snow
6	mixed rain and sleet
7	mixed snow and sleet
8	freezing drizzle
9	drizzle
10	freezing rain
11	showers
12	showers
13	snow flurries
14	light snow showers
15	blowing snow
16	snow
17	hail
18	sleet
19	dust
20	foggy
21	haze
22	smoky
23	blustery
24	windy
25	cold
26	cloudy
27	mostly cloudy (night)
28	mostly cloudy (day)
29	partly cloudy (night)
30	partly cloudy (day)
31	clear (night)
32	sunny
33	fair (night)
34	fair (day)
35	mixed rain and hail
36	hot
37	isolated thunderstorms
38	scattered thunderstorms
39	scattered thunderstorms
40	scattered showers
41	heavy snow
42	scattered snow showers
43	heavy snow
44	partly cloudy
45	thundershowers
46	snow showers
47	isolated thundershowers
3200	not available
 */
