using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;
using WeatherDesktop.Shared;

namespace ExternalService
{
    [Export(typeof(WeatherDesktop.Interface.IsharedSunRiseSetInterface))]
    [ExportMetadata("ClassName", "SunRiseSet")]
    public class SunRiseSet : IsharedSunRiseSetInterface
    {
        #region Constants
        const char dqoute = '"';
        const string ClassName = "SunRiseSet";
        #endregion

        #region Globals
        double _lat;
        double _long;
        DateTime _LastUpdate;
        int _HourToUpdate;
        Boolean HasUpdatedToday = false;
        SunRiseSetResponse _cache;
        Boolean _firstCall = true;
        Exception _ThrownException = null;
        #endregion

        #region Settings

        public Exception ThrownException() { return _ThrownException; }

        public MenuItem[] SettingsItems()
        {
            List<MenuItem> returnValue = new List<MenuItem>();
            returnValue.Add(new MenuItem("Hour To Update", ChangehourToUpdate));


            //MenuItem i = new MenuItem("Refresh GeoCords from...");
            //foreach (Type item in WeatherDesktop.Shared.KnownTypes.LatLongTypes)
          //  {
          //      i.MenuItems.Add( new MenuItem(item.FullName, TryupdateMenuItem));
           // }


            //returnValue.Add(i);
            return returnValue.ToArray();
        }


        public void TryupdateMenuItem(object sender, EventArgs e)
        {
            MenuItem Current = (MenuItem)sender;
            string Name = Current.Text;
            Type S = Type.GetType(Name);
            ILatLongInterface Item = (ILatLongInterface)Activator.CreateInstance(S);
            if (Item.worked())
            {
                _lat = Item.Latitude();
                _long = Item.Longitude();
                SharedObjects.LatLong.set(_lat, _long);
                MessageBox.Show("Update complete");

            }
            else { MessageBox.Show("Update did not work"); }
            
            
        }
        #endregion

        #region Events
        private void ChangehourToUpdate(object sender, EventArgs e) { UpdateHour(); }
        #endregion

        #region New

        public SunRiseSet()
        {

        }
        public void Load()
        {
            KeyValuePair<double, double> latlong = GetLocationProperty();
            _lat = latlong.Key;
            _long = latlong.Value;

            string HTU = SharedObjects.AppSettings.ReadSetting("HourUpdate");
            if (string.IsNullOrWhiteSpace(HTU)) { HTU = "6"; }
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
        private SunRiseSetResponse LiveCall(double Latitude, double Longitude)
        {
            SunRiseSetResponse response = new SunRiseSetResponse();
            try
            {
                string value;
                if (SharedObjects.Cache.Exists(ClassName)) { value = SharedObjects.Cache.StringValue(ClassName); }
                else
                {
                    string url = string.Format(Properties.Resources.SRS_Url, Latitude.ToString(), Longitude.ToString());
                     value = SharedObjects.CompressedCallSite(url);
                    SharedObjects.Cache.Set(ClassName, value);
                }

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
            catch (Exception x) { response.Status = x.ToString(); _ThrownException = x; }
            return response;
        }
        #endregion

        #region Helpers

        static bool intialgetLatLong()
        {
           bool worked = false;

   
            //if (!string.IsNullOrWhiteSpace(Shared.tryGetZip()))
            //{
               // foreach (Type item in WeatherDesktop.Shared.KnownTypes.LatLongTypes)
               // {
               //     try {
               //         var g_Weather = (Interface.ILatLongInterface)Activator.CreateInstance(item);
               ///         if (g_Weather.worked()) { worked = true; Shared.LatLong.set(g_Weather.Latitude(), g_Weather.Longitude()); }
               //     }
               //     catch { }
               //     if (worked) { break; }
               // }
            //}
            //if (!worked) { 
                if (MessageBox.Show("Lat and Long not yet available, Manual enter (yes), or pick a supplier in sunriseset settings (no)", "Lat Long not set", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    double lat = double.Parse(WeatherDesktop.Shared.SharedObjects.InputBox("Please Enter your Latitude", "Latitude"));
                    double lon = double.Parse(WeatherDesktop.Shared.SharedObjects.InputBox("Please Enter your Longitude", "Longitude"));
                SharedObjects.LatLong.set(lat, lon);
                    worked = true;
              //  }
            }

        
            return worked;
        }


        static void UpdateHour()
        {

            int current;
            try { current = int.Parse(SharedObjects.AppSettings.ReadSetting("HourUpdate")); }
            catch { current = 6; }

            try
            {
                string attempt;
                attempt = WeatherDesktop.Shared.SharedObjects.InputBox("Enter the hour you want to update the call to get sun rise, set info", "Hour update", current.ToString());
                SharedObjects.AppSettings.AddUpdateAppSettings("Hourupdate", int.Parse(attempt).ToString());
            }
            catch { MessageBox.Show("Could not update, please try again"); }
        }

        //Try geting the lat Long from the machine, refactored from MSDN.
        static KeyValuePair<double, double> GetLocationProperty()
        {
            if (SharedObjects.LatLong.HasRecord() || intialgetLatLong())
            { return new KeyValuePair<double, double>(SharedObjects.LatLong.lat, SharedObjects.LatLong.lng); }
            return new KeyValuePair<double, double>(0, 0);
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
            return SharedObjects.CompileDebug("SunRiseSet Service", DebugValues);
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