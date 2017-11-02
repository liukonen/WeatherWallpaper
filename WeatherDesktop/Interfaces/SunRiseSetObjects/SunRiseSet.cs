using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace WeatherDesktop.Interface
{
    public class SunRiseSet : IsharedSunRiseSetInterface
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
        public MenuItem[] SettingsItems()
        {
            List<MenuItem> returnValue = new List<MenuItem>();
            returnValue.Add(new MenuItem("Hour To Update", ChangehourToUpdate));
            returnValue.Add(new MenuItem("Update Location", ChangeLatLong));
            return returnValue.ToArray();
        }
        #endregion

        #region Events
        private void ChangehourToUpdate(object sender, EventArgs e) { UpdateHour(); }
        private void ChangeLatLong(object sender, EventArgs e) { UpdateLatLong(); }
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
            Invoke();
        }

        #endregion

        #region invoke
        public ISharedResponse Invoke()
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
                JavaScriptSerializer jsSerialization = new JavaScriptSerializer();
                SunRiseSetObject SunRiseSetResponse = jsSerialization.Deserialize<SunRiseSetObject>(value);

                response.Status = SunRiseSetResponse.status;
                if (response.Status.ToLower() == "ok")
                {
                    response.SolarNoon = DateTime.Parse(SunRiseSetResponse.results.solar_noon).ToLocalTime();
                    response.SunRise = DateTime.Parse(SunRiseSetResponse.results.sunrise).ToLocalTime();
                    response.SunSet = DateTime.Parse(SunRiseSetResponse.results.sunset).ToLocalTime();
                }
            }
            catch (Exception x) { response.Status = x.ToString(); }
            return response;
        }
        #endregion

        #region Helpers

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
        public string Debug()
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

        #region Auto Generated Code for JSON Deserilization

        //------------------------------------------------------------------------------
        // <auto-generated>
        //     This code was generated by a tool.
        //     Runtime Version:4.0.30319.42000
        //     http://jsontodatacontract.azurewebsites.net/
        //     Changes to this file may cause incorrect behavior and will be lost if
        //     the code is regenerated.
        // </auto-generated>
        //------------------------------------------------------------------------------



        // Type created for JSON at <<root>>
        [DataContractAttribute()]
        partial class SunRiseSetObject
        {

            [DataMemberAttribute()]
            public Results results;

            [DataMemberAttribute()]
            public string status;
        }

        // Type created for JSON at <<root>> --> results
        [DataContractAttribute(Name = "results")]
        partial class Results
        {

            [DataMemberAttribute()]
            public string sunrise;

            [DataMemberAttribute()]
            public string sunset;

            [DataMemberAttribute()]
            public string solar_noon;

            [DataMemberAttribute()]
            public string day_length;

            [DataMemberAttribute()]
            public string civil_twilight_begin;

            [DataMemberAttribute()]
            public string civil_twilight_end;

            [DataMemberAttribute()]
            public string nautical_twilight_begin;

            [DataMemberAttribute()]
            public string nautical_twilight_end;

            [DataMemberAttribute()]
            public string astronomical_twilight_begin;

            [DataMemberAttribute()]
            public string astronomical_twilight_end;
        }
        #endregion


    }



}