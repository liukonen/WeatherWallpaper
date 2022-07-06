using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using WeatherDesktop.Interface;
using WeatherDesktop.Share;
using WeatherDesktop.Shared.Handlers;
using WeatherDesktop.Shared.Extentions;

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

        public Exception ThrownException() => _ThrownException; 

        public MenuItem[] SettingsItems() => new MenuItem[] { new MenuItem("Hour To Update", ChangehourToUpdate) }; 


        public void TryupdateMenuItem(object sender, EventArgs e)
        {
            var Current = (MenuItem)sender;
            var Name = Current.Text;
            var S = Type.GetType(Name);
            var Item = (ILatLongInterface)Activator.CreateInstance(S);
            if (Item.worked())
            {
                geography = new Geography(Item.Latitude(), Item.Longitude());
                LatLongHandler.Set(geography.Latitude, geography.Longitude);
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
            geography = GetLocationProperty();
            _HourToUpdate = int.TryParse(AppSetttingsHandler.Read("HourUpdate"), out int value)
                ? value : 6;
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
            var response = new SunRiseSetResponse();
            try
            {
                string value;
                if (MemCacheHandler.Instance.Exists(ClassName)) { value = MemCacheHandler.Instance.GetItem<string>(ClassName); }
                else
                {
                    var url = string.Format(Properties.Resources.SRS_Url, geo.Latitude.ToString(), geo.Longitude.ToString());
                    value = WebHandler.Instance.CallSite(url);
                    MemCacheHandler.Instance.SetItem(ClassName, value);
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
            var worked = false;
            if (MessageBox.Show("Lat and Long not yet available, Manual enter (yes), or pick a supplier in sunriseset settings (no)", "Lat Long not set", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var lat = double.Parse(InputHandler.InputBox("Please Enter your Latitude", "Latitude"));
                var lon = double.Parse(InputHandler.InputBox("Please Enter your Longitude", "Longitude"));
                LatLongHandler.Set(lat, lon);
                worked = true;
            }
            return worked;
        }


        static void UpdateHour()
        {
            var current = int.TryParse(AppSetttingsHandler.Read("HourUpdate"), out int response)
                ? response : 6;
            try
            {
                var attempt = InputHandler.InputBox("Enter the hour you want to update the call to get sun rise, set info", "Hour update", current.ToString());
                AppSetttingsHandler.Write("Hourupdate", int.Parse(attempt).ToString());
            }
            catch { MessageBox.Show("Could not update, please try again"); }
        }

        static Geography GetLocationProperty()
        {
            return (LatLongHandler.HasRecord() || IntialgetLatLong())
                ? new Geography(LatLongHandler.Lat, LatLongHandler.Lng)
                : new Geography(0, 0);
        }
        #endregion

        #region Debug values
        public string Debug() =>

             new Dictionary<string, string>
            {
                {"Houre to update", _HourToUpdate.ToString()},
                {"Last update", _LastUpdate.ToString()},
                {"Latitude", geography.Latitude.ToString() },
                {"Longitude", geography.Longitude.ToString() },
                {"SunRise", _cache.SunRise.ToString() },
                {"SunSet", _cache.SunSet.ToString() },
                {"Status", _cache.Status }
            }.CompileDebug();

        #endregion
    }
}

