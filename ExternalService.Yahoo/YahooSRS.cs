using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using WeatherDesktop.Interface;
using WeatherDesktop.Shared;

namespace InternalService
{
    [Export(typeof(WeatherDesktop.Interface.IsharedSunRiseSetInterface))]
    [ExportMetadata("ClassName", "YahooSRS")]
    class YahooSRS : IsharedSunRiseSetInterface
    {
        string _zip;
        DateTime _LastUpdate;
        int _HourToUpdate = 6;
        Boolean HasUpdatedToday = false;
        SunRiseSetResponse _cache;
        Boolean _firstCall = true;
        private Exception _ThrownException = null;

        public Exception ThrownException() { return _ThrownException; }

        public void Load()
        {
            if (string.IsNullOrWhiteSpace(_zip)) { _zip = SharedObjects.ZipObjects.GetZip(); }
            _LastUpdate = DateTime.Now;
            Invoke();
        }

        public string Debug()
        {
            Dictionary<string, string> DebugValues = new Dictionary<string, string>();
            DebugValues.Add("status", (_ThrownException == null) ? _ThrownException.Message : string.Empty);
            return SharedObjects.CompileDebug(DebugValues);
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
            Items.Add(SharedObjects.ZipObjects.ZipMenuItem);
            return Items.ToArray();
        }

        private SunRiseSetResponse LiveCall()
        {
            SunRiseSetResponse sResponse = new SunRiseSetResponse();
            try
            {
                string URL = string.Format(InternalService.Properties.Resources.Yahoo_SRS_Url, _zip);
                string results = SharedObjects.CompressedCallSite(URL);
                JavaScriptSerializer jsSerialization = new JavaScriptSerializer();
                YahooSRSObject Response = jsSerialization.Deserialize<YahooSRSObject>(results);
                sResponse.SunRise = DateTime.ParseExact(Response.query.results.channel.astronomy.sunrise, "h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
                sResponse.SunSet = DateTime.ParseExact(Response.query.results.channel.astronomy.sunset, "h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
                sResponse.Status = "ok";
            }
            catch (Exception x) { _ThrownException = x; }
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
