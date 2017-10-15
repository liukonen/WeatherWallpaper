using System;
using System.Collections.Generic;

namespace WeatherDesktop.Interfaces
{
    public class SunRiseSet
    {
        #region Constants
        const string _path = "https://api.sunrise-sunset.org/json?lat={0}&lng={1}&date=today&formatted=0";
        const char dqoute = '"';
        #endregion

        #region Globals
        double _lat;
        double _long;
        DateTime _LastUpdate;
        int _HourToUpdate;
        Boolean HasUpdatedToday = false;
        SunRiseSetResponse _cache;
        Boolean _firstCall = true;
        #endregion

        #region New
        public SunRiseSet()
        {
            KeyValuePair<double, double> latlong = GetLocationProperty();
            _lat = latlong.Key;
            _long = latlong.Value;
            _HourToUpdate = 6;
            _LastUpdate = DateTime.Now;
        }

        public SunRiseSet(double Latitude, double Longitude) : this(Latitude, Longitude, 6)
        {
        }

        public SunRiseSet(double Latitude, double Longitude, int HourToUpdate)
        {
            _lat = Latitude;
            _long = Longitude;
            _HourToUpdate = HourToUpdate;
            _LastUpdate = DateTime.Now;
        }
        #endregion

        #region invoke
        public SunRiseSetResponse Invoke()
        {
            if (_firstCall) { _firstCall = false; _cache = LiveCall(_lat, _long); HasUpdatedToday = true; }

            if (_LastUpdate.Day != DateTime.Today.Day) { HasUpdatedToday = false; }
            if (!HasUpdatedToday && DateTime.Now.Hour == _HourToUpdate)
            {
                _cache = LiveCall(_lat, _long); HasUpdatedToday = true;
            }
            return _cache;
        }
        #endregion

        #region Live API call
        private static SunRiseSetResponse LiveCall(double Latitude, double Longitude)
        {
            SunRiseSetResponse response = new SunRiseSetResponse();
            try
            {
                string url = string.Format(_path, Latitude.ToString(), Longitude.ToString());
                string value = Shared.CompressedCallSite(url);
                Dictionary<string, string> values = PoormansJson(value);
                response.Status = values["status"];
                response.SolarNoon = DateTime.Parse(values["solar_noon"]).ToLocalTime();
                response.SunRise = DateTime.Parse(values["sunrise"]).ToLocalTime();
                response.SunSet = DateTime.Parse(values["sunset"]).ToLocalTime();
            }
            catch (Exception x) { response.Status = x.ToString(); }
            return response;
        }
        #endregion

        #region Helpers
        private static Dictionary<string, string> PoormansJson(string json)
        {
            const string colon = ":";
            Dictionary<string, string> returnValue = new Dictionary<string, string>();
            string parseValue = System.Text.RegularExpressions.Regex.Unescape(json);
            List<string> Parsed1 = new List<string>(parseValue.Substring(parseValue.IndexOf("results") + 10).Split(','));
            foreach (string element in Parsed1)
            {
                if (element.Contains(colon))
                {
                    int firstIndex = element.IndexOf(colon, StringComparison.Ordinal);
                    string key = element.Substring(0, firstIndex).Replace(dqoute.ToString(), string.Empty);
                    string dictValue = element.Substring(firstIndex + 1).Replace(dqoute.ToString(), string.Empty);
                    if (dictValue.EndsWith("}", StringComparison.Ordinal)) { dictValue = dictValue.Substring(0, dictValue.Length - 1); }
                    returnValue.Add(key, dictValue);
                }
            }
            return returnValue;
        }

        //Try geting the lat Long from the machine, refactored from MSDN.
        static KeyValuePair<double, double> GetLocationProperty()
        {
            System.Device.Location.GeoCoordinateWatcher watcher = new System.Device.Location.GeoCoordinateWatcher();
            // Do not suppress prompt, and wait 1000 milliseconds to start.
            watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));
            System.Device.Location.GeoCoordinate coord = watcher.Position.Location;
            if (coord.IsUnknown != true) { return new KeyValuePair<double, double>(coord.Latitude, coord.Longitude); }
            System.Windows.Forms.MessageBox.Show("Could not get Lat Long from machine.");
            return new KeyValuePair<double, double>(0, 0);
        }
        #endregion

        #region Debug values
        public string Debug()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("--------").Append(Environment.NewLine).Append("SunRiseSet").Append("--------").Append(Environment.NewLine);
            sb.Append("hour to update = ").Append(_HourToUpdate).Append(Environment.NewLine);
            sb.Append("LastUpdate = ").Append(_LastUpdate.ToString()).Append(Environment.NewLine);
            sb.Append("LatLong ").Append(_lat).Append(",").Append(_long).Append(Environment.NewLine).Append(Environment.NewLine).Append(Environment.NewLine);
            return sb.ToString();
        }
        #endregion
    }
}

/*output sample
     {
      "results":
      {
        "sunrise":"2015-05-21T05:05:35+00:00",
        "sunset":"2015-05-21T19:22:59+00:00",
        "solar_noon":"2015-05-21T12:14:17+00:00",
        "day_length":51444,
        "civil_twilight_begin":"2015-05-21T04:36:17+00:00",
        "civil_twilight_end":"2015-05-21T19:52:17+00:00",
        "nautical_twilight_begin":"2015-05-21T04:00:13+00:00",
        "nautical_twilight_end":"2015-05-21T20:28:21+00:00",
        "astronomical_twilight_begin":"2015-05-21T03:20:49+00:00",
        "astronomical_twilight_end":"2015-05-21T21:07:45+00:00"
      },
       "status":"OK"
    }
    
Notes on poormansJson    
Yes, I could have used NewtonSoft JSON to parse the object, and / or created serializable conditions to generate objects from the JSON, but for something
this small, its a lot easier (and less CPU cycles) to just generate a dictionary based off a string parse.
 
 */