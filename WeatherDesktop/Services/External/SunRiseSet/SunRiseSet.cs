using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using WeatherDesktop.Interface;
using WeatherDesktop.Share;
using WeatherDesktop.Shared.Handlers;

namespace WeatherDesktop.Services.External.SunRiseSet
{
    [Export(typeof(IsharedSunRiseSetInterface))]
    [ExportMetadata("ClassName", "ExternalSunRiseSet")]
    public class SunRiseSet : IsharedSunRiseSetInterface
    {
        #region Constants
        const string ClassName = "SunRiseSet";
        #endregion

        #region Globals
        Geography geography;
        DateTime _LastUpdate;
        int _HourToUpdate;
        bool HasUpdatedToday = false;
        SunRiseSetResponse _cache;
        bool _firstCall = true;
        Exception _ThrownException = null;
        #endregion

        #region Settings

        public Exception ThrownException() { return _ThrownException; }

        public MenuItem[] SettingsItems() { return new MenuItem[] { new MenuItem("Hour To Update", ChangehourToUpdate) }; }


        public void TryupdateMenuItem(object sender, EventArgs e)
        {
            MenuItem Current = (MenuItem)sender;
            string Name = Current.Text;
            Type S = Type.GetType(Name);
            ILatLongInterface Item = (ILatLongInterface)Activator.CreateInstance(S);
            if (Item.worked())
            {
                geography = new Geography(Item.Latitude(), Item.Longitude());
                SharedObjects.LatLong.Set(geography.Latitude, geography.Longitude);
                MessageBox.Show("Update complete");
            }
            else { MessageBox.Show("Update did not work"); }


        }
        #endregion

        #region Events
        private void ChangehourToUpdate(object sender, EventArgs e) => UpdateHour();

        #endregion

        #region New

        public SunRiseSet() { }

        public void Load()
        {
            geography= GetLocationProperty();

            _HourToUpdate = int.TryParse(AppSetttingsHandler.Read("HourUpdate"), out int value) ? value : 6;
            _LastUpdate = DateTime.Now;
            Invoke();
        }

        #endregion

        #region invoke

        public ISharedResponse Invoke()
        {
            if (_firstCall) { _firstCall = false; _cache = LiveCall(geography); HasUpdatedToday = true; }

            if (_LastUpdate.Day != DateTime.Today.Day) { HasUpdatedToday = false; }
            if (!HasUpdatedToday && DateTime.Now.Hour == _HourToUpdate)
            {
                _cache = LiveCall(geography); HasUpdatedToday = true;
            }
            return _cache;
        }
        #endregion

        #region Live API call
        private SunRiseSetResponse LiveCall(Geography geo)
        {
            SunRiseSetResponse response = new SunRiseSetResponse();
            try
            {
                string value;
                if (SharedObjects.Cache.Exists(ClassName)) { value = SharedObjects.Cache.StringValue(ClassName); }
                else
                {
                    string url = string.Format(Properties.Resources.SRS_Url, geo.Latitude.ToString(), geo.Longitude.ToString());
                    value = SharedObjects.CompressedCallSite(url);
                    SharedObjects.Cache.Set(ClassName, value);
                }

                var SunRiseSetResponse = JsonConvert.DeserializeObject<SunRiseSetObject>(value);


                response.Status = SunRiseSetResponse.Status;
                if (response.Status.ToLower() == "ok")
                {
                    response.SolarNoon = DateTime.Parse(SunRiseSetResponse.Results.Solar_noon).ToLocalTime();
                    response.SunRise = DateTime.Parse(SunRiseSetResponse.Results.Sunrise).ToLocalTime();
                    response.SunSet = DateTime.Parse(SunRiseSetResponse.Results.Sunset).ToLocalTime();
                }
            }
            catch (Exception x) { response.Status = x.ToString(); _ThrownException = x; }
            return response;
        }
        #endregion

        #region Helpers

        static bool IntialgetLatLong()
        {
            bool worked = false;
            if (MessageBox.Show("Lat and Long not yet available, Manual enter (yes), or pick a supplier in sunriseset settings (no)", "Lat Long not set", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                double lat = double.Parse(SharedObjects.InputBox("Please Enter your Latitude", "Latitude"));
                double lon = double.Parse(SharedObjects.InputBox("Please Enter your Longitude", "Longitude"));
                SharedObjects.LatLong.Set(lat, lon);
                worked = true;
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
                attempt = SharedObjects.InputBox("Enter the hour you want to update the call to get sun rise, set info", "Hour update", current.ToString());
                SharedObjects.AppSettings.AddUpdateAppSettings("Hourupdate", int.Parse(attempt).ToString());
            }
            catch { MessageBox.Show("Could not update, please try again"); }
        }

        static Geography GetLocationProperty()
        {
            return (SharedObjects.LatLong.HasRecord() || IntialgetLatLong())
                ? new Geography(SharedObjects.LatLong.Lat, SharedObjects.LatLong.Lng)
                :new Geography(0, 0);
        }
        #endregion

        #region Debug values
        public string Debug()
        {
            Dictionary<string, string> DebugValues = new Dictionary<string, string>
            {
                {"Houre to update", _HourToUpdate.ToString()},
                {"Last update", _LastUpdate.ToString()},
                {"Latitude", geography.Latitude.ToString() },
                {"Longitude", geography.Longitude.ToString() },
                {"SunRise", _cache.SunRise.ToString() },
                {"SunSet", _cache.SunSet.ToString() },
                {"Status", _cache.Status }
            };
            return SharedObjects.CompileDebug(DebugValues);
        }
        #endregion


    }
}

