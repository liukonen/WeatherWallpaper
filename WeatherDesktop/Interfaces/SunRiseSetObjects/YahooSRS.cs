using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using WeatherDesktop.Shared;
using Microsoft.VisualBasic;

namespace WeatherDesktop.Interface
{
    class YahooSRS : IsharedSunRiseSetInterface
    {
        const string YahooURL = "https://query.yahooapis.com/v1/public/yql?q=select%20astronomy%20from%20weather.forecast%20where%20woeid%20in%20(select%20content%20from%20pm.location.zip.region%20where%20zip%3D%22{0}%22%20and%20region%3D%22us%22)&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys&callback=";
        string _zip;
        DateTime _LastUpdate;
        int _HourToUpdate =6;
        Boolean HasUpdatedToday = false;
        SunRiseSetResponse _cache;
        Boolean _firstCall = true;
        string _status;

        public YahooSRS()
        {
            if (string.IsNullOrWhiteSpace(ZipCode)) { Enterzip(); }
            _LastUpdate = DateTime.Now;
            Invoke();
        }

        public string Debug()
        {
            Dictionary<string, string> DebugValues = new Dictionary<string, string>();
            DebugValues.Add("status", _status);
            return Shared.CompileDebug("Yahoo Weather", DebugValues);
        }

        public ISharedResponse Invoke()
        {
            if (_firstCall) { _firstCall = false; _cache = LiveCall(); HasUpdatedToday = true; }

            if (_LastUpdate.Day != DateTime.Today.Day) { HasUpdatedToday = false; }
            if (!HasUpdatedToday && DateTime.Now.Hour == _HourToUpdate)
            {
                _cache = LiveCall(); HasUpdatedToday = true;
            }
            return _cache;

        }

        public MenuItem[] SettingsItems()
        {
            List<MenuItem> Items = new List<MenuItem>();
            Items.Add(Shared.ZipMenuItem);
            return Items.ToArray();
        }

        private SunRiseSetResponse LiveCall()
        {
            string URL = string.Format(YahooURL, ZipCode);
            string results = Shared.CompressedCallSite(URL);
            JavaScriptSerializer jsSerialization = new JavaScriptSerializer();
            YahooSRSObject Response = jsSerialization.Deserialize<YahooSRSObject>(results);
            SunRiseSetResponse sResponse = new SunRiseSetResponse();
            //TimeSpan sr = TimeSpan.Parse( Response.query.results.channel.astronomy.sunrise);
            //TimeSpan ss = TimeSpan.Parse(Response.query.results.channel.astronomy.sunrise);
            /// sResponse.SunRise = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, sr.Hours, sr.Minutes, sr.Seconds);
            //sResponse.SunSet = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, ss.Hours, ss.Minutes, ss.Seconds);
            sResponse.SunRise = DateTime.ParseExact(Response.query.results.channel.astronomy.sunrise, "h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            sResponse.SunSet = DateTime.ParseExact(Response.query.results.channel.astronomy.sunset, "h:mm tt", System.Globalization.CultureInfo.InvariantCulture);

            sResponse.Status = "ok";
            return sResponse;
        }

        public string ZipCode
        {
            get
            {
                if (string.IsNullOrEmpty(_zip)) { _zip = Shared.ReadSettingEncrypted(SystemLevelConstants.ZipCode); }
                return _zip;
            }

            set
            {
                int dumbyvalidator;
                if (int.TryParse(value, out dumbyvalidator))
                {
                    _zip = value;
                    Shared.AddupdateAppSettingsEncrypted(SystemLevelConstants.ZipCode, _zip);
                }
                else { MessageBox.Show("invalid Zip code"); }

            }
        }
        public void Enterzip()
        {
            ZipCode = Interaction.InputBox("Enter New Zip Code", "Zip code", ZipCode.ToString());
        }

    }



    #region Auto Generated code
    [DataContract]
    public class Astronomy
    {

        [DataMember(Name = "sunrise")]
        public string sunrise { get; set; }

        [DataMember(Name = "sunset")]
        public string sunset { get; set; }
    }

    [DataContract]
    public class Channel
    {

        [DataMember(Name = "astronomy")]
        public Astronomy astronomy { get; set; }
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
    public class YahooSRSObject
    {

        [DataMember(Name = "query")]
        public Query query { get; set; }
    }

    #endregion

}
