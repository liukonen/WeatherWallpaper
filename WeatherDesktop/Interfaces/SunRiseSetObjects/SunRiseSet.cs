using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WeatherDesktop.Interface
{
    public class SunRiseSet: ISharedInterface
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

        #region Settings
        public  MenuItem[] SettingsItems()
        {
            List<MenuItem> returnValue = new List<MenuItem>();
            returnValue.Add(new MenuItem("Hour To Update", ChangehourToUpdate));
            returnValue.Add(new MenuItem("Update Location", ChangeLatLong));
            return returnValue.ToArray();
        }
        #endregion

        #region Events
        private void ChangehourToUpdate(object sender, EventArgs e){ UpdateHour(); }
        private void ChangeLatLong(object sender, EventArgs e){UpdateLatLong();}
        #endregion
                
        #region New

        public SunRiseSet()
        {
            KeyValuePair<double, double> latlong = GetLocationProperty();
            _lat = latlong.Key;
            _long = latlong.Value;

            string HTU = Shared.ReadSetting("HourUpdate");
            if (string.IsNullOrWhiteSpace(HTU))
            {
                ChangehourToUpdate(new object(), new EventArgs());
                HTU = Shared.ReadSetting("HourUpdate");
            }
            _HourToUpdate = int.Parse(HTU);
            _LastUpdate = DateTime.Now;
        }

        #endregion

        #region invoke
        public  ISharedResponse Invoke()
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


        static void UpdateLatLong()
        {
            switch (MessageBox.Show("get weather from MSWeather (yes), from System (no) or manual entry", "where to get weather", MessageBoxButtons.YesNoCancel))
            {

                case DialogResult.Yes:
                    MSWeather weather = new MSWeather();
                    WeatherDesktop.Interface.Shared.AddupdateAppSettingsEncrypted("LatLong", string.Concat(weather.Latitude, ",", weather.Longitude));
                    break;

                case DialogResult.No:
                    System.Device.Location.GeoCoordinateWatcher watcher = new System.Device.Location.GeoCoordinateWatcher();
                    // Do not suppress prompt, and wait 1000 milliseconds to start.
                    watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));
                    System.Device.Location.GeoCoordinate coord = watcher.Position.Location;
                    if (coord.IsUnknown != true)
                    {
                        WeatherDesktop.Interface.Shared.AddupdateAppSettingsEncrypted("LatLong", string.Concat(coord.Latitude, ",", coord.Longitude));
                    }
                    else { MessageBox.Show("Could not update weather."); }
                    break;
                default:
                    try
                    {
                        double lat = double.Parse(Microsoft.VisualBasic.Interaction.InputBox("Please Enter your Latitude", "Latitude"));
                        double lon = double.Parse(Microsoft.VisualBasic.Interaction.InputBox("Please Enter your Longitude", "Longitude"));
                        WeatherDesktop.Interface.Shared.AddupdateAppSettingsEncrypted("LatLong", string.Concat(lat, ",", lon));

                    }
                    catch (Exception x)
                    {
                        MessageBox.Show(x.Message, "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    break;
            }
        }

        static void UpdateHour()
        {

            int current;
            try { current = int.Parse(WeatherDesktop.Interface.Shared.ReadSetting("HourUpdate")); }
            catch { current = 6; }

            try
            {
                string attempt;
                attempt = Microsoft.VisualBasic.Interaction.InputBox("Enter the hour you want to update the call to get sun rise, set info", "Hour update", current.ToString());
                WeatherDesktop.Interface.Shared.AddUpdateAppSettings("Hourupdate", int.Parse(attempt).ToString());
            }
            catch { MessageBox.Show("Could not update, please try again"); }
        }
    
        //Try geting the lat Long from the machine, refactored from MSDN.
        static KeyValuePair<double, double> GetLocationProperty()
        {
            string LatLong = WeatherDesktop.Interface.Shared.ReadSettingEncrypted("LatLong");
            if (string.IsNullOrWhiteSpace(LatLong))
            {
                UpdateLatLong();
                LatLong = WeatherDesktop.Interface.Shared.ReadSettingEncrypted("LatLong");
                if (string.IsNullOrWhiteSpace(LatLong))
                {
                    MessageBox.Show("Can not get Lat Long, please restart", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }                
            }
            string[] LatLongParse = LatLong.Split(',');
            return new KeyValuePair<double, double>(double.Parse(LatLongParse[0]), double.Parse(LatLongParse[1]));
        }
        #endregion

        #region Debug values
        public  string Debug()
        {
            Dictionary<string, string> DebugValues = new Dictionary<string, string>();
            DebugValues.Add("Houre to update", _HourToUpdate.ToString());
            DebugValues.Add("Last update", _LastUpdate.ToString());
            DebugValues.Add("Latitude", _lat.ToString());
            DebugValues.Add("Longitude", _long.ToString());
            DebugValues.Add("SunRise", _cache.SunRise.ToString());
            DebugValues.Add("SunSet", _cache.SunSet.ToString());
            DebugValues.Add("SolarNoon", _cache.SolarNoon.ToString());
            DebugValues.Add("Status", _cache.Status);
            return Shared.CompileDebug("SunRiseSet Service", DebugValues);
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