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
         string _zip;
        DateTime _LastUpdate;
        int _HourToUpdate =6;
        Boolean HasUpdatedToday = false;
        SunRiseSetResponse _cache;
        Boolean _firstCall = true;
        string _status;

        public YahooSRS()
        {
            if (string.IsNullOrWhiteSpace(_zip)) { _zip = Shared.GetZip(); }
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
            string URL = string.Format(Properties.Resources.Yahoo_SRS_Url, _zip);
            string results = Shared.CompressedCallSite(URL);
            JavaScriptSerializer jsSerialization = new JavaScriptSerializer();
            YahooSRSObject Response = jsSerialization.Deserialize<YahooSRSObject>(results);
            SunRiseSetResponse sResponse = new SunRiseSetResponse();
             sResponse.SunRise = DateTime.ParseExact(Response.query.results.channel.astronomy.sunrise, "h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            sResponse.SunSet = DateTime.ParseExact(Response.query.results.channel.astronomy.sunset, "h:mm tt", System.Globalization.CultureInfo.InvariantCulture);

            sResponse.Status = "ok";
            return sResponse;
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
